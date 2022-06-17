Param(
    [string] $botProjectFolder,
    [string] $publishingProfile,
	[string] $crossTrainedLUDirectory
)

. ($PSScriptRoot + "/PublishUtils.ps1")

$settings = Get-Settings -botProjectFolder $botProjectFolder -publishingProfile $publishingProfile

$inputFilePath = $(Join-Path $crossTrainedLUDirectory "/SSWSophieBot.en-us.qna")
$endpoint = $settings.AppSettings.qna.endpoint.TrimEnd("/") + "/qnamaker/v4.0"
$kbId = $settings.AppSettings.qna.knowledgebaseid
$subscriptionKey  = $settings.AppSettings.qna.subscriptionKey

bf qnamaker:kb:replace -i $inputFilePath --endpoint $endpoint --kbId $kbId --subscriptionKey $subscriptionKey
bf qnamaker:kb:publish --endpoint $endpoint --kbId $kbId --subscriptionKey $subscriptionKey