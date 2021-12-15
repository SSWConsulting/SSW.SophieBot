using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SSW.SophieBot.Entities;

namespace SSW.SophieBot.LUIS.Migrator
{
    public class LuisMigratorSchemaManager : RecognizerSchemaManagerBase
    {
        public LuisMigratorSchemaManager(
            IServiceProvider serviceProvider,
            IOptions<RecognizerSchemaOptions> schemaOptions)
            : base(serviceProvider, schemaOptions)
        {

        }

        public override Task PublishSchemaAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public override async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var modelTypes = new List<Type>
            {
                typeof(SswPersonNames)
            };

            foreach (var modelType in modelTypes)
            {
                var modelInstance = (IRecognizerModel)ServiceProvider.GetRequiredService(modelType);

                await foreach (var seedResult in modelInstance.SeedAsync(cancellationToken))
                {
                    if (!seedResult)
                    {
                        throw new RecognizerSchemaException($"Failed to seed model {modelType.FullName}");
                    }
                }
            }
        }
    }
}
