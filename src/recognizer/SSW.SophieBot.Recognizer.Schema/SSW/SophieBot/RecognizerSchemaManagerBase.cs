using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public abstract class RecognizerSchemaManagerBase : IRecognizerSchemaManager
    {
        protected IServiceProvider ServiceProvider { get; }

        protected RecognizerSchemaOptions Options { get; }

        public RecognizerSchemaManagerBase(
            IServiceProvider serviceProvider,
            IOptions<RecognizerSchemaOptions> options)
        {
            ServiceProvider = serviceProvider;
            Options = options.Value;
        }

        public abstract Task PublishSchemaAsync(CancellationToken cancellationToken = default);

        public virtual async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            var modelTypes = Options.ModelTypes;
            foreach (var modelType in modelTypes)
            {
                var modelInstance = (IModel)ServiceProvider.GetRequiredService(modelType);

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
