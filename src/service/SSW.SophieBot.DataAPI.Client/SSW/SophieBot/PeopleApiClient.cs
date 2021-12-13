using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.Employees;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SSW.SophieBot
{
    public class PeopleApiClient : IPeopleApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly PeopleApiOptions _options;
        private readonly ILogger<PeopleApiClient> _logger;

        public PeopleApiClient(HttpClient httpClient, IOptions<PeopleApiOptions> options, ILogger<PeopleApiClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IAsyncEnumerable<IEnumerable<Employee>> GetPagedEmployeesAsync()
        {
            return AsyncEnumerable.Empty<IEnumerable<Employee>>();
        }
    }
}
