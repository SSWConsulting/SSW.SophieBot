using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.Entities;

namespace SSW.SophieBot.LUIS.Migrator
{
    public class LuisMigratorSchemaManager : LuisSchemaManager
    {
        public LuisMigratorSchemaManager(
            HttpClient httpClient,
            ILUISAuthoringClient luisAuthoringClient,
            IServiceProvider serviceProvider,
            IOptions<RecognizerSchemaOptions> schemaOptions,
            IOptions<LuisOptions> luisOptions,
            ILogger<LuisSchemaManager> logger)
            : base(httpClient, luisAuthoringClient, serviceProvider, schemaOptions, luisOptions, logger)
        {

        }

        protected override List<Type> ChooseModelTypesToPublish()
        {
            return SchemaOptions.ModelTypes
                .Where(modelType => modelType == typeof(SswPersonNames)
                || modelType == typeof(Contact)
                || modelType == typeof(FirstName)
                || modelType == typeof(LastName)
                || modelType == typeof(PersonName))
                .ToList();
        }
    }
}
