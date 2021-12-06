using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.DataSync.Crm.Config;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.HttpClients
{
    // TODO: do polly
    public class AuthClient
    {
        private readonly HttpClient _httpClient;
        private readonly CrmOptions _crmOptions;
        private readonly ILogger<AuthClient> _logger;
        private readonly string _encodedCreds;
        private string _accessToken;

        public AuthClient(HttpClient httpClient, IOptions<CrmOptions> crmOptions, ILogger<AuthClient> logger)
        {
            _httpClient = httpClient;
            _crmOptions = crmOptions.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_crmOptions.GetFormatedTokenEndpoint());
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            if (!string.IsNullOrWhiteSpace(_crmOptions.AppId) && !string.IsNullOrWhiteSpace(_crmOptions.AppSecret))
            {
                var clientCreds = $"{_crmOptions.AppId}:{Uri.EscapeDataString(_crmOptions.AppSecret)}";
                var credBytes = Encoding.UTF8.GetBytes(clientCreds);
                _encodedCreds = Convert.ToBase64String(credBytes);
            }
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(_accessToken)) // TODO: refresh token here
            {
                return _accessToken;
            }

            if (string.IsNullOrWhiteSpace(_encodedCreds))
            {
                _logger.LogWarning("Failed to get access token from Crm due to empty credential. Please check CrmAppId and CrmAppSecret configurations");
                return default;
            }

            var body = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = _crmOptions.AccessTokenScope
            };

            var request = new HttpRequestMessage(HttpMethod.Post, string.Empty);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _encodedCreds);
            var content = new FormUrlEncodedContent(body);
            request.Content = content;

            using var accessTokenResponse = await _httpClient.SendAsync(request, cancellationToken);

            accessTokenResponse.EnsureSuccessStatusCode();

            _accessToken = await GetAccessTokenFromResponseAsync(accessTokenResponse, cancellationToken);

            return _accessToken;
        }

        private static async Task<string> GetAccessTokenFromResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            var accessTokenString = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(accessTokenString))
            {
                using var accessTokenJson = JsonDocument.Parse(accessTokenString);
                if (accessTokenJson.RootElement.ValueKind == JsonValueKind.Object
                    && accessTokenJson.RootElement.TryGetProperty("access_token", out var accessToken))
                {
                    return accessToken.GetString();
                }
            }

            return default;
        }
    }
}
