using Microsoft.Azure.ApplicationInsights.Query;
using Microsoft.Azure.ApplicationInsights.Query.Models;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using SSW.SophieBot.Components.Models;
using System;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Services
{
    public class AppInsightsService : ITelemetryService
    {
        public const string QueryBaseUrl = "https://api.applicationinsights.io/v1";
        private readonly AppInsightsSettings _settings;

        public AppInsightsService(IOptions<AppInsightsSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<UsageByUserQueryResult> GetUsageByUserAsync(
            int? spanDays = null,
            int? maxItemCount = null,
            Func<string, string> userNameGroupKeyFunc = null)
        {
            var validPastDays = Math.Min(90, Math.Max(0, spanDays ?? 7));
            var validMaxItemCount = Math.Min(50, Math.Max(0, maxItemCount ?? 10));
            var userNameGroupKey = "user_AccountId";
            if (userNameGroupKeyFunc != null)
            {
                userNameGroupKey = userNameGroupKeyFunc.Invoke(userNameGroupKey);
            }

            var query = $@"customEvents
| where  name == 'LuisResult'
    and isnotnull(customDimensions.intent)
    and isnotempty(user_AccountId)
| summarize usageCount = dcount(tostring(customDimensions.activityId), 2) by user={userNameGroupKey}
| order by usageCount desc
| limit {validMaxItemCount}";

            var credentials = new ApiKeyClientCredentials(_settings.ApiKey);
            using var client = new ApplicationInsightsDataClient(credentials)
            {
                BaseUri = new Uri(QueryBaseUrl)
            };

            using HttpOperationResponse<QueryResults> response =
                await client.Query.ExecuteWithHttpMessagesAsync(_settings.AppID, query, $"P{validPastDays}D");

            if (!response.Response.IsSuccessStatusCode)
            {
                return new UsageByUserQueryResult(); // TODO: improve failure handling
            }

            return new UsageByUserQueryResult(response.Body).OrderByUsageCountDesc();
        }
    }
}
