function Get-Settings {
    param(
        [string] $botProjectFolder,
        [string] $publishingProfile
    )
    
    if (-not (Test-Path $botProjectFolder)) {
        Write-Error "Project folder not found: $botProjectFolder"
        exit 1
    }

    $settingsFolder = $(Join-Path $botProjectFolder "settings")
    $appSettingsFile = Get-ChildItem -Path $(Join-Path $settingsFolder *) -Include appsettings.json -Force
    if (-not $appSettingsFile) {
        Write-Error "Appsettings file not found: $appSettingsFile"
        exit 1
    }

    $appSettings = Get-Content $appSettingsFile.FullName | ConvertFrom-Json
    $publishTarget = $appSettings.publishTargets | Where-Object { $_.name -eq $publishingProfile }
    if ($publishTarget.Count -eq 0) {
        Write-Error "No publishing profile found for name: $publishingProfile"
        exit 1
    }

    $publishConfiguration = $publishTarget.configuration | ConvertFrom-Json 

    return [PSCustomObject]@{
        AppSettingsFile = $appSettingsFile;
        AppSettings     = $appSettings;
        PublishConfig   = $publishConfiguration
    }
}

function Merge-AppSettings {
    param(
        $appSettings,
        $publishConfig,
        [string] $generatedFolder
    )
    
    Merge-LUIS -settings $appSettings -publishConfig $publishConfig -generatedFolder $generatedFolder
    Merge-CosmosDb -settings $appSettings -publishConfig $publishConfig
    Merge-BlobStorage -settings $appSettings -publishConfig $publishConfig
    Merge-ApplicationInsights -settings $appSettings -publishConfig $publishConfig
    Merge-ScmHostDomain -settings $appSettings -publishConfig $publishConfig
    Merge-MicrosoftApp -settings $appSettings -publishConfig $publishConfig
}

function Merge-LUIS {
    param(
        $settings,
        $publishConfig,
        [string] $generatedFolder
    )

    if (-not (Test-Path $generatedFolder)) {
        Write-Error "GeneratedFolder not found: $generatedFolder"
        exit 1
    }
    
    $luisSettingsFile = Get-ChildItem -Path $generatedFolder -Filter "luis.settings.*.json" -file
    if (-not $luisSettingsFile) {
        Write-Error "LUIS settings file not found in generated folder: $generatedFolder"
        exit 1
    }

    $luisSettings = Get-Content $luisSettingsFile.FullName | ConvertFrom-Json
    $publishLuisConfig = $publishConfig.settings.luis

    $settings.luis | Add-Member -Type NoteProperty -Name authoringEndpoint -Value $publishLuisConfig.authoringEndpoint -Force
    $settings.luis | Add-Member -Type NoteProperty -Name authoringKey -Value $publishLuisConfig.authoringKey -Force
    $settings.luis | Add-Member -Type NoteProperty -Name authoringRegion -Value $publishLuisConfig.region -Force
    $settings.luis | Add-Member -Type NoteProperty -Name endpoint -Value $publishLuisConfig.endpoint -Force
    $settings.luis | Add-Member -Type NoteProperty -Name endpointKey -Value $publishLuisConfig.endpointKey -Force
    $settings.luis | Add-Member -Type NoteProperty -Name environment -Value $publishConfig.environment -Force
    $settings.luis | Add-Member -Type NoteProperty -Name region -Value $publishLuisConfig.region -Force
    
    $luisConfig = @{}
    $luisSettings.luis.PSObject.Properties | Foreach-Object { $luisConfig[$_.Name] = $_.Value }
    foreach ($key in $luisConfig.Keys) { $settings.luis | Add-Member -Type NoteProperty -Name $key -Value $luisConfig[$key] -Force }
}

# TODO: Merge-Qna

function Merge-CosmosDb {
    param(
        $settings,
        $publishConfig
    )

    $cosmosDbConfig = $publishConfig.settings.cosmosDb
    if ($cosmosDbConfig) {
        $settings | Add-Member -Type NoteProperty -Name CosmosDbPartitionedStorage -Value $cosmosDbConfig -Force

        $runtimeSettings = $settings.runtimeSettings
        if ($runtimeSettings) {
            $runtimeSettings | Add-Member -Type NoteProperty -Name storage -Value CosmosDbPartitionedStorage -Force
        }
    }
}

function Merge-BlobStorage {
    param(
        $settings,
        $publishConfig
    )

    $blobConfig = $publishConfig.settings.blobStorage
    if ($blobConfig.connectionString -And $blobConfig.container) {
        $blobSettings = [PSCustomObject]@{
            connectionString = $blobConfig.connectionString;
            containerName    = $blobConfig.container
        }

        $featuresConfig = $settings.runtimeSettings.features

        if ($featuresConfig) {
            $featuresConfig | Add-Member -Type NoteProperty -Name blobTranscript -Value $blobSettings -Force
        }
    }
}

function Merge-ApplicationInsights {
    param(
        $settings,
        $publishConfig
    )

    $appInsightsConfig = $publishConfig.settings.applicationInsights
    if ($appInsightsConfig) {
        $telemetryOptions = $settings.runtimeSettings.telemetry.options
        
        if ($telemetryOptions) {
            $telemetryOptions | Add-Member -Type NoteProperty -Name connectionString -Value $appInsightsConfig.connectionString -Force
            $telemetryOptions | Add-Member -Type NoteProperty -Name instrumentationKey -Value $appInsightsConfig.InstrumentationKey -Force
        }
    }
}

function Merge-ScmHostDomain {
    param(
        $settings,
        $publishConfig
    )

    $settings | Add-Member -Type NoteProperty -Name scmHostDomain -Value $publishConfig.scmHostDomain -Force
}

function Merge-MicrosoftApp {
    param(
        $settings,
        $publishConfig
    )

    $settings | Add-Member -Type NoteProperty -Name MicrosoftAppId -Value $publishConfig.settings.MicrosoftAppId -Force
    $settings | Add-Member -Type NoteProperty -Name MicrosoftAppPassword -Value $publishConfig.settings.MicrosoftAppPassword -Force
}

# TODO: Merge-LuResources
# TODO: Merge-QnaResources