#!/usr/bin/env python3
"""Convert SophieBot .lu files to a CLU project, then import, train, and deploy."""

from __future__ import annotations

import json
import os
import re
import sys
import time
import urllib.error
import urllib.request
from collections import defaultdict
from pathlib import Path

API_VERSION = "2023-04-01"
PROJECT = "sswsophiebot-clu"
DEPLOYMENT = "production"
MODEL = "production"
LANGUAGE = "en"

PREBUILT_MAP = {
    "datetimeV2": "DateTime",
    "datetime": "DateTime",
    "personName": "Person.Name",
    "geographyV2": "Geography.Location",
    "email": "Email",
    "number": "Quantity.Number",
}

LABEL_RE = re.compile(r"\{@([A-Za-z0-9_]+)(?:=((?:[^{}]|\{@[^}]*\})*))?\}")
LIST_HEADER_RE = re.compile(r"^@\s*list\s+([A-Za-z0-9_]+)\s*=?\s*$")
LIST_CANON_RE = re.compile(r"^-\s*([A-Za-z0-9_ -]+)\s*:\s*$")
LIST_SYN_RE = re.compile(r"^-\s*(.+?)\s*$")
INTENT_HEADER_RE = re.compile(r"^#\s+([A-Za-z0-9_]+)\s*$")


def parse_labeled_utterance(raw: str):
    """Return (plain_text, [{category, offset, length}]). Nested labels become multiple entities."""

    def walk(text: str, base: int):
        out = []
        found = []
        i = 0
        while i < len(text):
            if text.startswith("{@", i):
                depth = 0
                j = i
                while j < len(text):
                    if text.startswith("{@", j):
                        depth += 1
                        j += 2
                        continue
                    if text[j] == "}":
                        depth -= 1
                        j += 1
                        if depth == 0:
                            break
                        continue
                    j += 1
                inner = text[i + 2 : j - 1]
                if "=" in inner:
                    name, value = inner.split("=", 1)
                    value = value.strip()
                else:
                    name, value = inner, ""
                name = name.strip()
                child_text, child_entities = walk(value, base + len("".join(out)))
                start = base + len("".join(out))
                if child_text:
                    found.append(
                        {
                            "category": name,
                            "offset": start,
                            "length": len(child_text),
                        }
                    )
                found.extend(child_entities)
                out.append(child_text)
                i = j
            else:
                out.append(text[i])
                i += 1
        return "".join(out), found

    plain, entities = walk(raw, 0)
    seen = set()
    unique = []
    for ent in entities:
        key = (ent["category"], ent["offset"], ent["length"])
        if key in seen or ent["length"] <= 0:
            continue
        seen.add(key)
        unique.append(ent)
    return plain, unique


def parse_lu(path: Path):
    intents = set()
    utterances = []
    list_entities = defaultdict(lambda: defaultdict(list))
    learned_entities = set()
    current_intent = None
    current_list = None
    current_canon = None

    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.rstrip()
        stripped = line.strip()
        if not stripped:
            continue

        intent_match = INTENT_HEADER_RE.match(stripped)
        if intent_match:
            current_intent = intent_match.group(1)
            current_list = None
            current_canon = None
            intents.add(current_intent)
            continue

        list_match = LIST_HEADER_RE.match(stripped)
        if list_match:
            current_list = list_match.group(1)
            current_canon = None
            current_intent = None
            continue

        if stripped.startswith("@ "):
            current_list = None
            current_canon = None
            ml = re.match(r"^@\s*ml\s+([A-Za-z0-9_]+)", stripped)
            if ml:
                learned_entities.add(ml.group(1))
            continue

        if current_list:
            indent = len(line) - len(line.lstrip(" \t"))
            canon_match = LIST_CANON_RE.match(stripped)
            if canon_match and indent <= 1:
                current_canon = canon_match.group(1).strip()
                list_entities[current_list][current_canon]
                continue
            syn_match = LIST_SYN_RE.match(stripped)
            if syn_match and current_canon:
                syn = syn_match.group(1).strip().strip("'\"")
                if syn:
                    list_entities[current_list][current_canon].append(syn)
            continue

        if current_intent and stripped.startswith("-"):
            utt = stripped[1:].strip()
            if not utt or utt.startswith("^"):
                continue
            # Skip unexpanded entity placeholders like {@contact} with no value
            if re.search(r"\{@[A-Za-z0-9_]+\}", utt):
                continue
            text, ents = parse_labeled_utterance(utt)
            text = re.sub(r"\s+", " ", text).strip()
            if not text:
                continue
            item = {
                "text": text,
                "language": LANGUAGE,
                "intent": current_intent,
            }
            if ents:
                item["entities"] = ents
                for ent in ents:
                    if ent["category"] not in PREBUILT_MAP:
                        learned_entities.add(ent["category"])
            utterances.append(item)

    intents.add("None")
    return intents, utterances, list_entities, learned_entities


def build_project(intents, utterances, list_entities, learned_entities):
    entities = []

    for name, prebuilt in PREBUILT_MAP.items():
        entities.append(
            {
                "category": name,
                "compositionSetting": "combineComponents",
                "prebuilts": [{"category": prebuilt}],
            }
        )

    for name, canons in list_entities.items():
        sublists = []
        for key, synonyms in canons.items():
            values = list(dict.fromkeys([key] + synonyms))
            values = [v for v in values if v]
            if not values:
                continue
            sublists.append(
                {
                    "listKey": key,
                    "synonyms": [{"language": LANGUAGE, "values": values}],
                }
            )
        if not sublists:
            continue
        entities.append(
            {
                "category": name,
                "compositionSetting": "combineComponents",
                "list": {"sublists": sublists},
            }
        )
        learned_entities.discard(name)

    reserved = set(PREBUILT_MAP) | set(list_entities)
    for name in sorted(learned_entities):
        if name in reserved:
            continue
        entities.append({"category": name, "compositionSetting": "combineComponents"})

    # Drop entity labels whose category was not imported
    known = {e["category"] for e in entities}
    cleaned = []
    seen_text = set()
    for utt in utterances:
        key = utt["text"].strip().lower()
        if key in seen_text:
            continue
        seen_text.add(key)
        if "entities" in utt:
            utt = dict(utt)
            text_len = len(utt["text"])
            utt["entities"] = [
                e
                for e in utt["entities"]
                if e["category"] in known
                and e["offset"] >= 0
                and e["length"] > 0
                and e["offset"] + e["length"] <= text_len
            ]
            if not utt["entities"]:
                utt.pop("entities")
        cleaned.append(utt)

    return {
        "projectFileVersion": API_VERSION,
        "stringIndexType": "Utf16CodeUnit",
        "metadata": {
            "projectKind": "Conversation",
            "projectName": PROJECT,
            "multilingual": False,
            "description": "SophieBot replacement for retired LUIS",
            "language": LANGUAGE,
        },
        "assets": {
            "projectKind": "Conversation",
            "intents": [{"category": name} for name in sorted(intents)],
            "entities": entities,
            "utterances": cleaned,
        },
    }


def request(method, url, key, body=None, timeout=60):
    data = None if body is None else json.dumps(body).encode("utf-8")
    req = urllib.request.Request(url, data=data, method=method)
    req.add_header("Ocp-Apim-Subscription-Key", key)
    if body is not None:
        req.add_header("Content-Type", "application/json")
    try:
        with urllib.request.urlopen(req, timeout=timeout) as resp:
            raw = resp.read()
            payload = json.loads(raw.decode("utf-8")) if raw else {}
            return resp.status, dict(resp.headers), payload
    except urllib.error.HTTPError as exc:
        raw = exc.read()
        payload = {}
        try:
            payload = json.loads(raw.decode("utf-8")) if raw else {}
        except json.JSONDecodeError:
            payload = {"raw": raw.decode("utf-8", errors="replace")}
        return exc.code, dict(exc.headers), payload


def wait_job(url, key, timeout_s=900):
    deadline = time.time() + timeout_s
    while time.time() < deadline:
        status, _, payload = request("GET", url, key)
        job = payload.get("status") or payload.get("jobStatus")
        print(f"  job {job} ({status})")
        if job in {"succeeded", "Succeeded"}:
            return payload
        if job in {"failed", "Failed", "cancelled", "Cancelled"}:
            raise RuntimeError(f"CLU job failed: {json.dumps(payload)[:1000]}")
        time.sleep(8)
    raise TimeoutError(f"Timed out waiting for {url}")


def main():
    endpoint = os.environ["CQA_ENDPOINT"].rstrip("/")
    key = os.environ["CQA_KEY"]
    lu_path = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(
        "bots/employee-finder/src/SSW.SophieBot/language-understanding/en-us/SSWSophieBot.en-us.lu"
    )

    intents, utterances, list_entities, learned = parse_lu(lu_path)
    project = build_project(intents, utterances, list_entities, learned)
    out = Path("/tmp/sswsophiebot-clu.json")
    out.write_text(json.dumps(project, indent=2), encoding="utf-8")
    print(
        f"Wrote {out} intents={len(project['assets']['intents'])} "
        f"entities={len(project['assets']['entities'])} "
        f"utterances={len(project['assets']['utterances'])}"
    )

    import_url = (
        f"{endpoint}/language/authoring/analyze-conversations/projects/{PROJECT}"
        f"/:import?api-version={API_VERSION}"
    )
    status, headers, payload = request("POST", import_url, key, project)
    print("import", status, payload)
    if status not in (200, 202):
        raise SystemExit(f"Import failed: {payload}")
    job_url = headers.get("Operation-Location") or headers.get("operation-location")
    if job_url:
        wait_job(job_url, key)

    train_url = (
        f"{endpoint}/language/authoring/analyze-conversations/projects/{PROJECT}"
        f"/:train?api-version={API_VERSION}"
    )
    status, headers, payload = request(
        "POST", train_url, key, {"modelLabel": MODEL, "trainingMode": "standard"}
    )
    print("train", status, payload)
    if status == 400 and "trainingMode" in json.dumps(payload):
        status, headers, payload = request("POST", train_url, key, {"modelLabel": MODEL})
        print("train-retry", status, payload)
    if status not in (200, 202):
        raise SystemExit(f"Train failed: {payload}")
    job_url = headers.get("Operation-Location") or headers.get("operation-location")
    if job_url:
        wait_job(job_url, key, timeout_s=1800)

    deploy_url = (
        f"{endpoint}/language/authoring/analyze-conversations/projects/{PROJECT}"
        f"/deployments/{DEPLOYMENT}?api-version={API_VERSION}"
    )
    status, headers, payload = request(
        "PUT", deploy_url, key, {"trainedModelLabel": MODEL}
    )
    print("deploy", status, payload)
    if status not in (200, 202):
        raise SystemExit(f"Deploy failed: {payload}")
    job_url = headers.get("Operation-Location") or headers.get("operation-location")
    if job_url:
        wait_job(job_url, key)

    predict_url = f"{endpoint}/language/:analyze-conversations?api-version={API_VERSION}"
    for text in (
        "Who is in the Sydney office right now?",
        "hi",
        "Is Adam booked or free?",
        "who is Adam Cogan?",
    ):
        status, _, payload = request(
            "POST",
            predict_url,
            key,
            {
                "kind": "Conversation",
                "analysisInput": {
                    "conversationItem": {
                        "id": "1",
                        "participantId": "1",
                        "text": text,
                    }
                },
                "parameters": {
                    "projectName": PROJECT,
                    "deploymentName": DEPLOYMENT,
                    "stringIndexType": "Utf16CodeUnit",
                },
            },
        )
        pred = (
            payload.get("result", {})
            .get("prediction", {})
        )
        print(
            f"predict [{text!r}] intent={pred.get('topIntent')} "
            f"entities={[e.get('category')+':'+e.get('text','') for e in pred.get('entities', [])]}"
        )


if __name__ == "__main__":
    main()
