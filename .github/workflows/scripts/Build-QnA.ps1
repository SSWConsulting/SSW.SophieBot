Param(
    [string] $botProjectFolder,
    [string] $publishingProfile,
	[string] $crossTrainedLUDirectory
)

. ($PSScriptRoot + "/PublishUtils.ps1")

$settings = Get-Settings -botProjectFolder $botProjectFolder -publishingProfile $publishingProfile

$inputFilePath = $(Join-Path $crossTrainedLUDirectory "/SSWSophieBot.en-us.qna")
$endpoint = $settings.AppSettings.qna.endpoint.TrimEnd("/")
$projectName = $settings.AppSettings.qna.projectName
$deploymentName = $settings.AppSettings.qna.deploymentName
$apiKey = $settings.AppSettings.qna.endpointKey

# Read and parse the QnA file
$qnaContent = Get-Content -Path $inputFilePath -Raw

# Convert .qna format to Custom Question Answering format
# Parse QnA pairs from the .qna file
$qnaPairs = @()
$currentQuestion = ""
$currentAnswer = ""
$currentAlternateQuestions = @()
$inAnswer = $false

foreach ($line in ($qnaContent -split "`n")) {
    $line = $line.Trim()
    
    if ($line -match "^#\s*\?\s*(.+)$") {
        # New question found
        if ($currentQuestion -and $currentAnswer) {
            $qnaPairs += @{
                questions = @($currentQuestion) + $currentAlternateQuestions
                answer = $currentAnswer.Trim()
            }
        }
        $currentQuestion = $Matches[1].Trim()
        $currentAnswer = ""
        $currentAlternateQuestions = @()
        $inAnswer = $false
    }
    elseif ($line -match "^-\s*(.+)$" -and $currentQuestion -and -not $inAnswer) {
        # Alternate question
        $currentAlternateQuestions += $Matches[1].Trim()
    }
    elseif ($line -match "^```markdown$" -or $line -match "^```$") {
        $inAnswer = -not $inAnswer
    }
    elseif ($inAnswer) {
        $currentAnswer += $line + "`n"
    }
}

# Add last QnA pair
if ($currentQuestion -and $currentAnswer) {
    $qnaPairs += @{
        questions = @($currentQuestion) + $currentAlternateQuestions
        answer = $currentAnswer.Trim()
    }
}

# Create the import payload for Custom Question Answering
$importPayload = @{
    qnaDocuments = $qnaPairs | ForEach-Object {
        @{
            id = [guid]::NewGuid().ToString()
            questions = $_.questions
            answer = $_.answer
            metadata = @{}
        }
    }
}

$headers = @{
    "Ocp-Apim-Subscription-Key" = $apiKey
    "Content-Type" = "application/json"
}

# Update the knowledge base using REST API
$updateUri = "$endpoint/language/query-knowledgebases/projects/$projectName/qnas?api-version=2021-10-01"

Write-Host "Updating Custom Question Answering project: $projectName"
Write-Host "Endpoint: $endpoint"
Write-Host "Found $($qnaPairs.Count) QnA pairs"

try {
    # Clear existing QnAs and add new ones (PATCH operation)
    $body = @{
        add = $importPayload.qnaDocuments
    } | ConvertTo-Json -Depth 10

    $response = Invoke-RestMethod -Uri $updateUri -Method Patch -Headers $headers -Body $body
    Write-Host "Knowledge base updated successfully"

    # Deploy to production
    $deployUri = "$endpoint/language/query-knowledgebases/projects/$projectName/deployments/$deploymentName`?api-version=2021-10-01"
    
    Write-Host "Deploying to $deploymentName..."
    $deployResponse = Invoke-RestMethod -Uri $deployUri -Method Put -Headers $headers -Body "{}"
    Write-Host "Deployment successful"
}
catch {
    Write-Error "Failed to update Custom Question Answering: $_"
    Write-Error $_.Exception.Response
    exit 1
}
