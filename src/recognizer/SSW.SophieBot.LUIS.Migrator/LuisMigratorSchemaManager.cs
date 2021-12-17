using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.Entities;

namespace SSW.SophieBot.LUIS.Migrator
{
    public class LuisMigratorSchemaManager : LuisSchemaManager
    {
        public LuisMigratorSchemaManager(
            ILuisService luisService,
            IServiceProvider serviceProvider,
            IOptions<RecognizerSchemaOptions> schemaOptions,
            ILogger<LuisSchemaManager> logger)
            : base(luisService, serviceProvider, schemaOptions, logger)
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
