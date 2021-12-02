using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.DataSync.Domain.Employees;
using SSW.SophieBot.DataSync.Domain.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.HttpClients
{
    public class CrmClient
    {
        private readonly HttpClient _httpClient;
        private readonly AuthClient _authClient;
        private readonly CrmOptions _crmOptions;
        private readonly SyncOptions _syncOptions;
        private readonly ILogger<CrmClient> _logger;

        public CrmClient(
            HttpClient httpClient,
            AuthClient authClient,
            IOptions<CrmOptions> crmOptions,
            IOptions<SyncOptions> syncOptions,
            ILogger<CrmClient> logger)
        {
            _httpClient = httpClient;
            _authClient = authClient;
            _crmOptions = crmOptions.Value;
            _syncOptions = syncOptions.Value;
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Add(
                HeaderNames.ContentType, "application/json;charset=utf8");
            _httpClient.DefaultRequestHeaders.Add(
                HeaderNames.Accept, "application/json");
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, string.Empty);
        }

        public virtual async Task<OdataPagedResponse<CrmEmployee>> GetPagedEmployeesAsync(string nextLink = null, CancellationToken cancellationToken = default)
        {
            await AddHeadersAsync(cancellationToken);

            Uri apiUrl;
            if (!string.IsNullOrWhiteSpace(nextLink))
            {
                apiUrl = new Uri(nextLink);
            }
            else
            {
                apiUrl = new Uri(new Uri(_crmOptions.BaseUri), "systemusers");

                var crmSitesOdata = await GetSitesAsync(cancellationToken);
                if (crmSitesOdata.Value != null && crmSitesOdata.Value.Any())
                {
                    var siteIds = crmSitesOdata.Value.Select(site => site.Siteid.EnsureSurroundsWith("'"));
                    var queryStrings = new Dictionary<string, string>
                    {
                        ["$filter"] = "Microsoft.Dynamics.CRM.In(PropertyName=@p1,PropertyValues=@p2)",
                        ["@p1"] = "'siteid'",
                        ["@p2"] = $"[{string.Join(",", siteIds)}]"
                    };

                    apiUrl = new Uri(QueryHelpers.AddQueryString(apiUrl.OriginalString, queryStrings));
                }
                else
                {
                    _logger.LogError("Failed to get sites list or it's empty");
                    return new OdataPagedResponse<CrmEmployee>();
                }
            }

            var crmEmployeesOdata = await _httpClient.GetFromJsonAsync<OdataPagedResponse<CrmEmployee>>(
                apiUrl,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                cancellationToken);

            return crmEmployeesOdata ?? new OdataPagedResponse<CrmEmployee>();
        }

        public async Task<OdataResponse<List<CrmSite>>> GetSitesAsync(CancellationToken cancellationToken = default)
        {
            await AddHeadersAsync(cancellationToken);
            var apiUrl = new Uri(new Uri(_crmOptions.BaseUri), "sites");

            var crmSitesOdata = await _httpClient.GetFromJsonAsync<OdataResponse<List<CrmSite>>>(
                apiUrl,
                new JsonSerializerOptions(JsonSerializerDefaults.Web),
                cancellationToken);

            return crmSitesOdata ?? new OdataResponse<List<CrmSite>>();
        }

        private async Task AddHeadersAsync(CancellationToken cancellationToken = default)
        {
            var accessToken = await _authClient.GetAccessTokenAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                _logger.LogWarning("Failed to get access token for Crm.");
            }

            if (_httpClient.DefaultRequestHeaders.Contains(HeaderNames.Authorization))
            {
                _httpClient.DefaultRequestHeaders.Remove(HeaderNames.Authorization);
            }

            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, accessToken);

            if (!_httpClient.DefaultRequestHeaders.Contains("Prefer"))
            {
                _httpClient.DefaultRequestHeaders.Add("Prefer", $"odata.maxpagesize={_syncOptions.EmployeeSync.MaxRetrieveCountPerRequest}");
            }
        }
    }
}
