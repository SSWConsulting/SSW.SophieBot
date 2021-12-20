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
            var migrationTypes = new List<Type>
            {
                typeof(PersonNames),
                typeof(Contact),
                typeof(FirstName),
                typeof(LastName),
                typeof(PersonName)
            };

            return SchemaOptions.ModelTypes
                .Where(modelType => migrationTypes.Contains(modelType))
                .ToList();
        }
    }
}
