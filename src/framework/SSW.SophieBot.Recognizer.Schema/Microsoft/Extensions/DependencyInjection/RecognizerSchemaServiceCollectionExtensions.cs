using SSW.SophieBot;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RecognizerSchemaServiceCollectionExtensions
    {
        public static IServiceCollection AddRecognizerSchema<T>(this IServiceCollection services)
            where T : class
        {
            var modelDescriptors = RecognizerSchemaHelper.GetAllModelTypes<T>();
            var modelTypes = modelDescriptors.Select(modelDescriptor => modelDescriptor.ModelType);

            foreach (var modelType in modelTypes)
            {
                services.AddTransient(modelType);
            }

            services.PostConfigure<RecognizerSchemaOptions>(options =>
            {
                options.AddModelTypes(modelTypes);
            });

            return services;
        }
    }
}
