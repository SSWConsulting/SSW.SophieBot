Param(
    [string] $botProjectFolder,
    [string] $botProjectName,
    [string] $publishingProfile,
    [string] $outputFolder,
    [string] $generatedFolder = $(Join-Path $botProjectFolder generated)
)

. ($PSScriptRoot + "/PublishUtils.ps1")

$botProjectFilePath = $(Join-Path $botProjectFolder $botProjectName)
if (-not (Test-Path $botProjectFilePath)) {
    Write-Error "Project file not found: $botProjectFilePath"
    exit 1
}

$settings = Get-Settings -botProjectFolder $botProjectFolder -publishingProfile $publishingProfile
Merge-AppSettings -appSettings $settings.AppSettings -publishConfig $settings.PublishConfig -generatedFolder $generatedFolder

$settings.AppSettings | ConvertTo-Json -depth 100 | Out-File $settings.AppSettingsFile.FullName -Force -Encoding utf8

$runtimeIdentifier = $settings.PublishConfig.runtimeIdentifier ?? "win-x64"
dotnet publish $botProjectFilePath -c Release -o "$outputFolder" --self-contained true -r $runtimeIdentifier