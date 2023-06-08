## Schema Reference for `appsettings.json`

| Field Name      | Field Type | Description    | Used By | Sample Value |
| --------------- | ---------- | ------------------------ | ------- | ------------ |
| customFunctions | list       | Custom functions for language generation |    [Adaptive expressions](https://learn.microsoft.com/en-us/azure/bot-service/bot-builder-concept-adaptive-expressions?view=azure-bot-service-4.0&tabs=arithmetic#functions)     | []           |
| defaultLanguage | string     | Default language for language recognizers         |     Bot Framework Composer    | en-us        |
| defaultLocale   | string     | Custom functions for language generation   |   Bot Framework      | en-us        |
| importedLibraries   | list     | Imported skills   |   Bot Framework Composer     | []       |
| languages   | list     | Supported languages   |   Bot Framework     | ["en-us"]       |
| Logging   | object     | Logging configuration for Asp.Net Core   |   SophieBot     |       |
| luFeatures   | object     | Language Understanding feature togglings   |   Bot Framework Composer     |       |
| luis   | object     | LUIS resource provisioning   |   Bot Framework Composer     |       |
| luis.name   | string     | LUIS app name   |   Bot Framework Composer     |   SSWSophieBot    |
| luis.defaultLanguage   | string     | default language for LUIS app   |   Bot Framework Composer     |   en-us    |
| luis.authoringEndpoint   | string     | Endpoint for LUIS Authoring resource   |   Bot Framework Composer     |   https://australiaeast.api.cognitive.microsoft.com/    |
| luis.authoringRegion   | string     | Region setting for LUIS Authoring resource   |   Bot Framework Composer     |   australiaeast    |
| luis.endpoint   | string     | Query endpoint for LUIS Prediction resource   |   Bot Framework Composer     |   https://australiaeast.api.cognitive.microsoft.com/    |
| luis.environment   | string     | Environment setting for LUIS app creation   |   Bot Framework Composer     |   composer    |
| MicrosoftAppId   | guid     | Id of registered application for Azure Bot Service   |   Bot Framework     |   XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX    |
| publishTargets   | list     | Multiple publish profiles for deploying through composer   |   Bot Framework Composer     |       |
| publishTargets.name   | string     | Name of publish profile   |   Bot Framework Composer     |   SSW.SophieBot    |
| publishTargets.type   | string     | Type of publish profile   |   Bot Framework Composer     |   azurePublish    |
| publishTargets.configuration   | string     | JSON string of the publish profile   |   Bot Framework Composer     |    (Can be provisioned through Composer)   |
| qna   | object     | QnA Maker resource provisioning   |   Bot Framework Composer     |      |
| qna.hostname   | string     | server url for the QnA resource   |   Bot Framework Composer     |   https://sswsophiebot-qna.azurewebsites.net/qnamaker   |
| qna.knowledgebaseid   | guid     | KB id for the QnA resource   |   Bot Framework Composer     |   XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX   |
| qna.qnaRegion   | string     | Region setting for the QnA resource   |   Bot Framework Composer     |   westus   |
| qna.endpoint   | string     | Query endpoint for the QnA resource   |   Bot Framework Composer     |   https://sswsophiebot-qna.cognitiveservices.azure.com/   |
| qna.SSWSophieBot_en_us_qna   | guid     | QnA app id   |   Bot Framework Composer     |   XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX   |
| runtime   | object     | Runtime setting for composer   |   Bot Framework Composer     |      |
| runtime.command   | string     | Startup command to run the .NET app   |   Bot Framework Composer     |   dotnet run --project SSW.SophieBot.csproj   |
| runtime.customRuntime   | boolean     | Whether runtime setting should be used   |   Bot Framework Composer     |   true   |
| runtime.key   | string     | Runtime key marking adaptive and sdk  |   Bot Framework Composer     |   adaptive-runtime-dotnet-webapp   |
| runtime.path   | string     | Root path of the bot app  |   Bot Framework Composer     |   ../   |
| runtimeSettings   | object     | Runtime setting for the bot app  |   Bot Framework     |      |
| runtimeSettings.adapters   | list     | List of [adapters](https://learn.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.botadapter?view=botbuilder-dotnet-stable) used by the bot app  |   Bot Framework     |      |
| runtimeSettings.features   | object     | Feature settings for the bot app |   Bot Framework Composer    |      |
| runtimeSettings.components   | list     | List of custom components to be registered in bot app |   Bot Framework Composer    |      |
| runtimeSettings.skills   | object     | List of bot skills |   Bot Framework Composer    |      |
| runtimeSettings.telemetry   | object     | Telemtry settings |   Bot Framework Composer    |      |
| runtimeSettings.storage   | string     | Bot state storage configuration |   Bot Framework Composer    |   CosmosDbPartitionedStorage   |
| skillConfiguration   | object     | Configurations for bot skill |   Bot Framework Composer    |      |
| skillHostEndpoint   | string     | Hosting endpoint for bot skill |   Bot Framework Composer    |   http://localhost:3980/api/skills   |
| HttpClient   | object     | API configurations for data source | SophieBot    |    |
| HttpClient.BaseAddress   | string     | Base url of data source endpoints | SophieBot    |  https://ssw-people.azurewebsites.net  |
| HttpClient.Endpoints   | object     |   List of API endpoints of data source  | SophieBot  | |
| HttpClient.Endpoints.Name   | string     | Endpoint name    |  SophieBot | GetProfile |
| HttpClient.Endpoints.Path   | string     | Endpoint url path    |  SophieBot | /api/employees |
| HttpClient.Endpoints.RequestHeaders   | object     | Optional headers for this endpoint    |  SophieBot |  |
| App   | object     | Application configurations for bot app    |  SophieBot |  |
| App.TenantId   | guid     | Tenant id of bot resources    |  SophieBot | XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX |
| App.PeopleSiteUrl   | string     | Optional url of employees site    |  SophieBot | https://www.ssw.com.au/people |
| App.CRMHost   | string     | CRM address    |  SophieBot | https://ssw.crm6.dynamics.com|
| App.CRMBookingEndpoint   | string     | Optional url for booking report page  |  SophieBot | https://app.powerbi.com |
| App.CRMProjectUrl   | string     | Optional url for all projects list page  |  SophieBot | https://ssw.crm6.dynamics.com |
| App.CRMActiveProjectUrl   | string     | Optional url for active projects list page  |  SophieBot | https://ssw.crm6.dynamics.com |
| App.CRMClientUrl   | string     | Optional url for clients list page  |  SophieBot | https://ssw.crm6.dynamics.com |
| App.CRMUserUrl   | string     | Optional url for users list page  |  SophieBot | https://ssw.crm6.dynamics.com |
| App.ProductUrl   | string     | Optional url for project home page  |  SophieBot | https://sswsophie.com/sophiebot/ |
| App.SharePointUrl   | string     | Optional url for sharepoint home page  |  SophieBot | https://sswcom.sharepoint.com |
| App.EmployeeResponsibilitiesUrl   | string     | Optional url for employee responsibilities page  |  SophieBot | https://sswcom.sharepoint.com |
| App.BaseUrl   | string     | Base url for bot app  |  SophieBot | https://sophiebot.azurewebsites.net |
| App.AppInsights   | object     | App insights configuration for bot app  |  SophieBot |  |
| App.AppInsights.CloudRoleName   | string     | App insights cloud role name  |  SophieBot | SSWSophieBot |
| App.AppInsights.CloudRoleInstance   | string     | App insights cloud role instance  |  SophieBot | SSW.SophieBot |
| App.AppInsights.AppID   | string    | App id for accessing app insights query endpoints | XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX |
| App.AppInsights.ApiKey   | string    | App key for accessing app insights query endpoints | XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX |
| App.AppInsights.WorkbookLink   | string    | Workbook link for app insights | https://portal.azure.com |