using Microsoft.Extensions.Logging;
using SSW.SophieBot.Entities;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestSswPeopleNamesClEntity : SswPersonNames
    {
        public TestSswPeopleNamesClEntity(
            ILuisService luisService,
            IPeopleApiClient peopleApiClient,
            ILogger<SswPersonNames> logger)
            : base(luisService, peopleApiClient, logger)
        {

        }
    }
}
