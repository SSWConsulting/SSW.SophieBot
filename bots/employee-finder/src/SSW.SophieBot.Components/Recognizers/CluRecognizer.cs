// Copyright (c) SSW. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SSW.SophieBot.Components.Recognizers
{
    /// <summary>
    /// Composer recognizer that calls Conversational Language Understanding and
    /// emits LUIS-shaped entities so existing SophieBot dialogs keep working.
    /// </summary>
    public class CluRecognizer : Recognizer
    {
        [JsonProperty("$kind")]
        public const string Kind = "SSW.CluRecognizer";

        private const string DefaultProjectName = "sswsophiebot-clu";
        private const string DefaultDeploymentName = "production";
        private const string ApiVersion = "2023-04-01";

        private static readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        private static readonly Regex NowRegex = new Regex(
            @"\b(right now|currently|at the moment|now)\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TodayRegex = new Regex(
            @"\btoday\b",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Dictionary<string, string> SiteAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["sydney"] = "sydney",
            ["melbourne"] = "melbourne",
            ["brisbane"] = "brisbane",
            ["newcastle"] = "newcastle",
            ["hangzhou"] = "hangzhou",
            ["china"] = "china",
            ["remote"] = "remote",
            ["remotely"] = "remote",
        };

        [JsonProperty("projectName")]
        public StringExpression ProjectName { get; set; } = DefaultProjectName;

        [JsonProperty("deploymentName")]
        public StringExpression DeploymentName { get; set; } = DefaultDeploymentName;

        [JsonProperty("endpoint")]
        public StringExpression Endpoint { get; set; } = "=settings.qna.endpoint";

        [JsonProperty("endpointKey")]
        public StringExpression EndpointKey { get; set; } = "=settings.qna.endpointKey";

        public override async Task<RecognizerResult> RecognizeAsync(
            DialogContext dialogContext,
            Activity activity,
            CancellationToken cancellationToken,
            Dictionary<string, string> telemetryProperties = null,
            Dictionary<string, double> telemetryMetrics = null)
        {
            var result = new RecognizerResult
            {
                Text = activity.Text,
                Intents = new Dictionary<string, IntentScore>(),
                Entities = new JObject(),
            };

            if (string.IsNullOrWhiteSpace(activity.Text))
            {
                result.Intents["None"] = new IntentScore { Score = 1.0 };
                return result;
            }

            try
            {
                var (endpoint, _) = Endpoint.TryGetValue(dialogContext.State);
                var (endpointKey, _) = EndpointKey.TryGetValue(dialogContext.State);
                var (projectName, _) = ProjectName.TryGetValue(dialogContext.State);
                var (deploymentName, _) = DeploymentName.TryGetValue(dialogContext.State);

                if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(endpointKey))
                {
                    throw new InvalidOperationException("CLU endpoint or endpointKey is missing.");
                }

                projectName = string.IsNullOrWhiteSpace(projectName) ? DefaultProjectName : projectName;
                deploymentName = string.IsNullOrWhiteSpace(deploymentName) ? DefaultDeploymentName : deploymentName;

                var prediction = await PredictAsync(endpoint, endpointKey, projectName, deploymentName, activity.Text, cancellationToken)
                    .ConfigureAwait(false);
                MapPrediction(activity.Text, prediction, result);
            }
            catch (Exception ex)
            {
                result.Intents["None"] = new IntentScore { Score = 1.0 };
                result.Properties["error"] = ex.Message;
            }

            TrackRecognizerResult(
                dialogContext,
                "CluRecognizerResult",
                FillRecognizerResultTelemetryProperties(result, telemetryProperties, dialogContext),
                telemetryMetrics);

            return result;
        }

        private static async Task<JObject> PredictAsync(
            string endpoint,
            string endpointKey,
            string projectName,
            string deploymentName,
            string text,
            CancellationToken cancellationToken)
        {
            var url = $"{endpoint.TrimEnd('/')}/language/:analyze-conversations?api-version={ApiVersion}";
            var body = new JObject
            {
                ["kind"] = "Conversation",
                ["analysisInput"] = new JObject
                {
                    ["conversationItem"] = new JObject
                    {
                        ["id"] = "1",
                        ["participantId"] = "1",
                        ["text"] = text,
                    }
                },
                ["parameters"] = new JObject
                {
                    ["projectName"] = projectName,
                    ["deploymentName"] = deploymentName,
                    ["stringIndexType"] = "Utf16CodeUnit",
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body.ToString(Formatting.None), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Ocp-Apim-Subscription-Key", endpointKey);

            using var response = await HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"CLU prediction failed ({(int)response.StatusCode}): {json}");
            }

            return JObject.Parse(json);
        }

        private static void MapPrediction(string text, JObject response, RecognizerResult result)
        {
            var prediction = response.SelectToken("result.prediction") as JObject ?? new JObject();
            var intents = prediction["intents"] as JArray ?? new JArray();
            var entities = prediction["entities"] as JArray ?? new JArray();

            if (intents.Count == 0)
            {
                var top = prediction.Value<string>("topIntent") ?? "None";
                result.Intents[top] = new IntentScore { Score = 1.0 };
            }
            else
            {
                foreach (var intent in intents)
                {
                    var name = intent.Value<string>("category") ?? "None";
                    var score = intent.Value<double?>("confidenceScore") ?? 0;
                    result.Intents[name] = new IntentScore { Score = score };
                }
            }

            if (!result.Intents.ContainsKey("None"))
            {
                result.Intents["None"] = new IntentScore { Score = 0 };
            }

            var mapped = new JObject();
            var instance = new JObject();
            var luisEntities = new JObject();
            JObject datetimeV2 = null;

            foreach (var entity in entities.OfType<JObject>())
            {
                var category = entity.Value<string>("category");
                var entityText = entity.Value<string>("text") ?? string.Empty;
                var offset = entity.Value<int?>("offset") ?? 0;
                var length = entity.Value<int?>("length") ?? entityText.Length;
                if (string.IsNullOrEmpty(category))
                {
                    continue;
                }

                AddInstance(instance, category, entityText, offset, length, entity.Value<double?>("confidenceScore") ?? 0);

                if (IsDateTimeCategory(category))
                {
                    datetimeV2 = ToLuisDatetimeV2(entity, entityText);
                    AddValue(mapped, "datetimeV2", datetimeV2);
                    AddValue(mapped, "datetime", new JObject
                    {
                        ["timex"] = new JArray(datetimeV2?["values"]?[0]?["timex"] ?? entityText),
                        ["type"] = datetimeV2?.Value<string>("type") ?? "date",
                    });
                    continue;
                }

                if (category.Equals("site", StringComparison.OrdinalIgnoreCase)
                    || category.Equals("remote", StringComparison.OrdinalIgnoreCase))
                {
                    var canonical = CanonicalSite(entityText);
                    AddValue(mapped, "site", new JArray(canonical));
                    AddValue(luisEntities, "site", new JArray(canonical));
                    continue;
                }

                if (category.Equals("geographyV2", StringComparison.OrdinalIgnoreCase)
                    || category.Equals("location", StringComparison.OrdinalIgnoreCase))
                {
                    AddValue(mapped, "geographyV2", new JObject
                    {
                        ["type"] = "city",
                        ["location"] = entityText,
                    });
                    continue;
                }

                if (category.Equals("personName", StringComparison.OrdinalIgnoreCase)
                    || category.Equals("Person.Name", StringComparison.OrdinalIgnoreCase))
                {
                    AddValue(mapped, "personName", entityText);
                    AddContact(mapped, entityText);
                    continue;
                }

                if (category.Equals("contact", StringComparison.OrdinalIgnoreCase))
                {
                    AddContact(mapped, entityText);
                    continue;
                }

                if (category.Equals("firstName", StringComparison.OrdinalIgnoreCase)
                    || category.Equals("lastName", StringComparison.OrdinalIgnoreCase))
                {
                    AddContactPart(mapped, category, entityText);
                    continue;
                }

                AddValue(mapped, category, entityText);
            }

            ApplyLocationFallback(text, mapped);
            datetimeV2 = ApplyDatetimeFallback(text, mapped, datetimeV2);

            if (mapped["$instance"] == null && instance.HasValues)
            {
                mapped["$instance"] = instance;
            }

            result.Entities = mapped;

            if (datetimeV2 != null)
            {
                luisEntities["datetimeV2"] = new JArray(datetimeV2);
            }

            result.Properties["luisResult"] = new JObject
            {
                ["prediction"] = new JObject
                {
                    ["topIntent"] = prediction.Value<string>("topIntent"),
                    ["intents"] = prediction["intents"],
                    ["entities"] = luisEntities,
                }
            };
        }

        private static bool IsDateTimeCategory(string category)
        {
            return category.Equals("datetimeV2", StringComparison.OrdinalIgnoreCase)
                || category.Equals("datetime", StringComparison.OrdinalIgnoreCase)
                || category.Equals("DateTime", StringComparison.OrdinalIgnoreCase);
        }

        private static JObject ToLuisDatetimeV2(JObject entity, string entityText)
        {
            var resolutions = entity["resolutions"] as JArray ?? new JArray();
            var first = resolutions.FirstOrDefault() as JObject ?? new JObject();
            var timex = first.Value<string>("timex") ?? entityText;
            var subKind = (first.Value<string>("dateTimeSubKind") ?? first.Value<string>("resolutionKind") ?? "Date").ToLowerInvariant();
            var type = subKind.Contains("range") ? "daterange"
                : subKind.Contains("duration") ? "duration"
                : subKind.Contains("time") && !subKind.Contains("date") ? "time"
                : "date";

            if (NowRegex.IsMatch(entityText) || string.Equals(timex, "PRESENT_REF", StringComparison.OrdinalIgnoreCase))
            {
                timex = "PRESENT_REF";
                type = "date";
            }

            var resolution = new JObject();
            if (first["begin"] != null || first["start"] != null)
            {
                resolution["start"] = first["begin"] ?? first["start"];
                resolution["end"] = first["end"];
                type = "daterange";
            }
            else
            {
                resolution["value"] = first.Value<string>("value") ?? entityText;
            }

            return new JObject
            {
                ["type"] = type,
                ["values"] = new JArray(new JObject
                {
                    ["timex"] = timex,
                    ["resolution"] = new JArray(resolution),
                })
            };
        }

        private static JObject ApplyDatetimeFallback(string text, JObject mapped, JObject existing)
        {
            if (mapped["datetimeV2"] != null)
            {
                return existing;
            }

            string timex = null;
            if (NowRegex.IsMatch(text))
            {
                timex = "PRESENT_REF";
            }
            else if (TodayRegex.IsMatch(text))
            {
                timex = DateTime.UtcNow.ToString("yyyy-MM-dd");
            }

            if (timex == null)
            {
                return existing;
            }

            var datetimeV2 = new JObject
            {
                ["type"] = "date",
                ["values"] = new JArray(new JObject
                {
                    ["timex"] = timex,
                    ["resolution"] = new JArray(new JObject
                    {
                        ["value"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    }),
                })
            };

            AddValue(mapped, "datetimeV2", datetimeV2);
            AddValue(mapped, "datetime", new JObject
            {
                ["timex"] = new JArray(timex),
                ["type"] = "date",
            });
            return datetimeV2;
        }

        private static void ApplyLocationFallback(string text, JObject mapped)
        {
            if (mapped["site"] != null || mapped["geographyV2"] != null)
            {
                return;
            }

            foreach (var pair in SiteAliases)
            {
                if (Regex.IsMatch(text, $@"\b{Regex.Escape(pair.Key)}\b", RegexOptions.IgnoreCase))
                {
                    if (pair.Value == "remote")
                    {
                        AddValue(mapped, "remote", "remote");
                    }
                    else
                    {
                        AddValue(mapped, "site", new JArray(pair.Value));
                    }
                    return;
                }
            }
        }

        private static string CanonicalSite(string text)
        {
            return SiteAliases.TryGetValue(text.Trim(), out var canonical) ? canonical : text.Trim().ToLowerInvariant();
        }

        private static void AddContact(JObject mapped, string fullName)
        {
            var parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var contact = mapped["contact"] as JArray ?? new JArray();
            var item = contact.FirstOrDefault() as JObject ?? new JObject();
            if (parts.Length > 0)
            {
                item["firstName"] = new JArray(parts[0]);
            }
            if (parts.Length > 1)
            {
                item["lastName"] = new JArray(string.Join(" ", parts.Skip(1)));
            }
            if (contact.Count == 0)
            {
                contact.Add(item);
                mapped["contact"] = contact;
            }
        }

        private static void AddContactPart(JObject mapped, string part, string value)
        {
            var contact = mapped["contact"] as JArray ?? new JArray();
            var item = contact.FirstOrDefault() as JObject ?? new JObject();
            item[part] = new JArray(value);
            if (contact.Count == 0)
            {
                contact.Add(item);
                mapped["contact"] = contact;
            }
        }

        private static void AddValue(JObject mapped, string name, JToken value)
        {
            if (mapped[name] is JArray array)
            {
                array.Add(value);
                return;
            }

            if (mapped[name] != null)
            {
                mapped[name] = new JArray(mapped[name], value);
                return;
            }

            mapped[name] = new JArray(value);
        }

        private static void AddInstance(JObject instance, string category, string text, int offset, int length, double score)
        {
            var arr = instance[category] as JArray ?? new JArray();
            arr.Add(new JObject
            {
                ["startIndex"] = offset,
                ["endIndex"] = offset + length,
                ["text"] = text,
                ["score"] = score,
                ["type"] = category,
            });
            instance[category] = arr;
        }
    }
}
