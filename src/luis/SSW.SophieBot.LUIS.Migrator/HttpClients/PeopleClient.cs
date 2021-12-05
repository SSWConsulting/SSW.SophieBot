using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using SSW.SophieBot.ClosedListEntity;
using SSW.SophieBot.Employees;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace SSW.SophieBot.LUIS.Migrator.HttpClients
{
    public class PeopleClient : IPeopleClient
    {
        private readonly HttpClient _httpClient;
        private readonly PeopleApiOptions _options;
        private readonly ILogger<PeopleClient> _logger;

        public PeopleClient(HttpClient httpClient, IOptions<PeopleApiOptions> options, ILogger<PeopleClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_options.BaseUri);
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Authorization, _options.Authorization);
            _httpClient.DefaultRequestHeaders.Add("Tenant", _options.Tenant);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async IAsyncEnumerable<IEnumerable<Employee>> GetAsyncPagedEmployees(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // TODO: this is temporary. People API currently doesn't support paging query, so we're just getting all active employees for now
            var apiUri = new Uri(new Uri(_options.BaseUri.EnsureEndsWith("/")), "employees");

            var employees = await _httpClient.GetFromJsonAsync<IEnumerable<Employee>>(
                apiUri,
                HttpClientHelper.SystemTextJsonSerializerOptions,
                cancellationToken);

            yield return employees;
        }
    }
}
