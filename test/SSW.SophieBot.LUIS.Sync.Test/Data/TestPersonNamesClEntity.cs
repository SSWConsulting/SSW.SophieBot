using Microsoft.Extensions.Logging;
using SSW.SophieBot.Entities;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestPersonNamesClEntity : PersonNames
    {
        public TestPersonNamesClEntity(
            ILuisService luisService,
            IPeopleApiClient peopleApiClient,
            ILogger<PersonNames> logger)
            : base(luisService, peopleApiClient, logger)
        {

        }
    }
}
