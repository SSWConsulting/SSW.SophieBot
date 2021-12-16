using Microsoft.Extensions.DependencyInjection.Extensions;
using SSW.SophieBot;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RecognizerSchemaServiceCollectionExtensions
    {
        public static IServiceCollection AddRecognizerSchema<TSchema>(
            this IServiceCollection services,
            Action<RecognizerSchemaServiceOptions> action = null)
            where TSchema : class
        {
            var modelDescriptors = RecognizerSchemaHelper.GetAllModelTypes<TSchema>();
            var modelTypes = modelDescriptors.Select(modelDescriptor => modelDescriptor.ModelType);

            foreach (var modelType in modelTypes)
            {
                services.AddTransient(modelType);
            }

            services.PostConfigure<RecognizerSchemaOptions>(options =>
            {
                options.AddModelTypes(modelTypes);
            });

            var serviceOptions = new RecognizerSchemaServiceOptions();
            action?.Invoke(serviceOptions);

            if (serviceOptions.SchemaManagerType != null)
            {
                services.RemoveAll<IRecognizerSchemaManager>();
                services.AddTransient(typeof(IRecognizerSchemaManager), serviceOptions.SchemaManagerType);
            }

            return services;
        }
    }
}
