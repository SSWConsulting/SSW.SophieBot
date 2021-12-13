using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Extensions.Options;
using SSW.SophieBot.Entities;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestSswPeopleNamesClEntity : SswPersonNames
    {
        public TestSswPeopleNamesClEntity(
            ILUISAuthoringClient luisAuthoringClient,
            IPeopleApiClient peopleApiClient,
            IOptions<LuisOptions> luisOptions) : base(luisAuthoringClient, peopleApiClient, luisOptions)
        {

        }
    }
}
