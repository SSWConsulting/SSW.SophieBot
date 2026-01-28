Param(
    [string] $botProjectFolder,
    [string] $publishingProfile,
    [string] $crossTrainedLUDirectory
)

. ($PSScriptRoot + "/PublishUtils.ps1")

$settings = Get-Settings -botProjectFolder $botProjectFolder -publishingProfile $publishingProfile

$endpoint = $settings.AppSettings.qna.endpoint.TrimEnd("/")
$projectName = $settings.AppSettings.qna.projectName
$deploymentName = $settings.AppSettings.qna.deploymentName
$apiKey = $settings.AppSettings.qna.endpointKey

Write-Host "=== Custom Question Answering Deployment ==="
Write-Host "Bot Project Folder: $botProjectFolder"
Write-Host "Endpoint: $endpoint"
Write-Host "Project: $projectName"
Write-Host "Deployment: $deploymentName"

# Try to find the QnA file - check multiple locations
$possiblePaths = @(
    $(Join-Path $crossTrainedLUDirectory "/SSWSophieBot.en-us.qna"),
    $(Join-Path $botProjectFolder "/knowledge-base/source/SSWSophieBotQnA.source.en-us.qna"),
    $(Join-Path $botProjectFolder "/knowledge-base/en-us/SSWSophieBot.en-us.qna")
)

$inputFilePath = $null
$qnaContent = $null

foreach ($path in $possiblePaths) {
    Write-Host "Checking: $path"
    if (Test-Path $path) {
        $content = Get-Content -Path $path -Raw -Encoding UTF8
        Write-Host "  File size: $($content.Length) chars"
        
        # Check if file has actual QnA content (not just imports)
        # Use (?m) for multiline mode so ^ matches start of each line
        if ($content -match "(?m)^#\s*\?\s*.+" -and $content.Length -gt 100) {
            $inputFilePath = $path
            $qnaContent = $content
            Write-Host "  Found QnA content!"
            break
        }
        else {
            Write-Host "  No QnA questions found (pattern: # ? question)"
            # Show first 200 chars for debugging
            $preview = if ($content.Length -gt 200) { $content.Substring(0, 200) } else { $content }
            Write-Host "  Preview: $preview"
        }
    }
    else {
        Write-Host "  File not found"
    }
}

if (-not $inputFilePath -or -not $qnaContent) {
    Write-Error "Could not find a QnA file with content in any of the expected locations"
    Write-Host "Searched paths:"
    foreach ($path in $possiblePaths) {
        Write-Host "  - $path (exists: $(Test-Path $path))"
    }
    exit 1
}

Write-Host "=== Using QnA File ==="
Write-Host "Path: $inputFilePath"
Write-Host "File size: $($qnaContent.Length) characters"

# Parse QnA pairs from the .qna file
$qnaPairs = [System.Collections.ArrayList]@()
$currentQuestion = ""
$currentAnswer = ""
$currentAlternateQuestions = [System.Collections.ArrayList]@()
$inAnswer = $false
$qnaId = 1

# Normalize line endings and split
$qnaContent = $qnaContent -replace "`r`n", "`n"
$lines = $qnaContent -split "`n"

Write-Host "Total lines in file: $($lines.Count)"

foreach ($line in $lines) {
    $trimmedLine = $line.Trim()
    
    # Skip empty lines, comments, and metadata
    if ([string]::IsNullOrWhiteSpace($trimmedLine) -or 
        $trimmedLine.StartsWith(">") -or 
        $trimmedLine.StartsWith("[import]") -or
        $trimmedLine -match "^<a id") {
        continue
    }
    
    # Match question line: # ? Question text or ## ? Question text
    if ($trimmedLine -match "^#{1,2}\s*\?\s*(.+)$") {
        # Save previous QnA pair if exists and has content
        if ($currentQuestion -and $currentAnswer.Trim()) {
            $null = $qnaPairs.Add(@{
                id = $qnaId
                questions = @($currentQuestion) + @($currentAlternateQuestions)
                answer = $currentAnswer.Trim()
            })
            $qnaId++
        }
        $currentQuestion = $Matches[1].Trim()
        $currentAnswer = ""
        $currentAlternateQuestions = [System.Collections.ArrayList]@()
        $inAnswer = $false
    }
    # Match code fence for answer block (``` or ```markdown)
    elseif ($trimmedLine -match "^``````") {
        $inAnswer = -not $inAnswer
    }
    # Collect answer content when inside code block
    elseif ($inAnswer) {
        $currentAnswer += $line + "`n"
    }
    # Match alternate question: - Alternate text (only when we have a question and not in answer)
    elseif ($trimmedLine -match "^-\s+(.+)$" -and $currentQuestion -and -not $inAnswer) {
        $altQ = $Matches[1].Trim()
        # Only add if it looks like a question (not a markdown list item starting with * or [)
        if ($altQ.Length -gt 3 -and -not $altQ.StartsWith("*") -and -not $altQ.StartsWith("[")) {
            $null = $currentAlternateQuestions.Add($altQ)
        }
    }
}

# Add last QnA pair
if ($currentQuestion -and $currentAnswer.Trim()) {
    $null = $qnaPairs.Add(@{
        id = $qnaId
        questions = @($currentQuestion) + @($currentAlternateQuestions)
        answer = $currentAnswer.Trim()
    })
}

Write-Host "=== Parsing Complete ==="
Write-Host "Found $($qnaPairs.Count) QnA pairs"

if ($qnaPairs.Count -eq 0) {
    Write-Warning "No QnA pairs found. Check the file format."
    exit 0
}

# Show sample parsed pairs
Write-Host "=== Sample QnA Pairs ==="
$sampleCount = [Math]::Min(3, $qnaPairs.Count)
for ($i = 0; $i -lt $sampleCount; $i++) {
    $pair = $qnaPairs[$i]
    Write-Host "[$($pair.id)] Q: $($pair.questions[0])"
    Write-Host "    Alternates: $($pair.questions.Count - 1)"
}

# Build JSON Patch payload for Custom Question Answering REST API
$qnaOperations = $qnaPairs | ForEach-Object {
    @{
        op = "add"
        value = @{
            id = $_.id
            answer = $_.answer
            source = "SSWSophieBot"
            questions = $_.questions
            metadata = @{}
        }
    }
}

$headers = @{
    "Ocp-Apim-Subscription-Key" = $apiKey
    "Content-Type" = "application/json"
}

$updateUri = "$endpoint/language/query-knowledgebases/projects/$projectName/qnas?api-version=2021-10-01"

Write-Host "=== Updating Knowledge Base ==="
Write-Host "URI: $updateUri"
Write-Host "Sending $($qnaOperations.Count) QnA operations"

try {
    $body = $qnaOperations | ConvertTo-Json -Depth 10 -Compress
    Write-Host "Request body size: $($body.Length) characters"
    
    $response = Invoke-RestMethod -Uri $updateUri -Method Patch -Headers $headers -Body $body -ContentType "application/json"
    Write-Host "Knowledge base updated successfully"

    # Deploy to production
    $deployUri = "$endpoint/language/query-knowledgebases/projects/$projectName/deployments/$deploymentName`?api-version=2021-10-01"
    
    Write-Host "=== Deploying to '$deploymentName' ==="
    $deployResponse = Invoke-RestMethod -Uri $deployUri -Method Put -Headers $headers -ContentType "application/json"
    Write-Host "Deployment successful!"
}
catch {
    Write-Error "Failed to update Custom Question Answering"
    Write-Error "Error: $($_.Exception.Message)"
    
    try {
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $responseBody = $reader.ReadToEnd()
            Write-Error "API Response: $responseBody"
        }
    }
    catch {
        Write-Error "Could not read error response"
    }
    
    exit 1
}

Write-Host "=== Complete ==="
