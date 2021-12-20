using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SSW.SophieBot.Entities
{
    [Model("site")]
    public class Site : IClosedList
    {
        private readonly ILuisService _luisService;
        private readonly IPeopleApiClient _peopleApiClient;
        private readonly ILogger<Site> _logger;

        public ICollection<SubClosedList> SubLists { get; } = new List<SubClosedList>();

        public Site(
            ILuisService luisService,
            IPeopleApiClient peopleApiClient,
            ILogger<Site> logger)
        {
            _luisService = luisService;
            _peopleApiClient = peopleApiClient;
            _logger = logger;
        }

        public async IAsyncEnumerable<bool> SeedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
