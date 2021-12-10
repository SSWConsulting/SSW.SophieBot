using SSW.SophieBot;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RecognizerSchemaServiceCollectionExtensions
    {
        public static IServiceCollection AddRecognizerSchema<T>(this IServiceCollection services)
            where T : class
        {
            var modelTypes = RecognizerSchemaHelper.GetAllModelTypes<T>();

            foreach (var modelType in modelTypes)
            {
                services.AddTransient(modelType);
            }

            services.PostConfigure<RecognizerSchemaOptions>(options =>
            {
                options.ModelTypes.AddRange(modelTypes);
            });

            return services;
        }
    }
}
