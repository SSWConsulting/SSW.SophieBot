using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public abstract class RecognizerSchemaManagerBase : IRecognizerSchemaManager
    {
        protected IServiceProvider ServiceProvider { get; }

        protected RecognizerSchemaOptions SchemaOptions { get; }

        public RecognizerSchemaManagerBase(
            IServiceProvider serviceProvider,
            IOptions<RecognizerSchemaOptions> schemaOptions)
        {
            ServiceProvider = serviceProvider;
            SchemaOptions = schemaOptions.Value;
        }

        public abstract Task PublishSchemaAsync(CancellationToken cancellationToken = default);

        public virtual async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            foreach (var modelType in ChooseModelTypesToPublish())
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

        protected virtual List<Type> ChooseModelTypesToPublish()
        {
            return SchemaOptions.ModelTypes.ToList();
        }

        protected virtual List<Type> ChooseModelTypesToSeed()
        {
            return ChooseModelTypesToPublish();
        }
    }
}
