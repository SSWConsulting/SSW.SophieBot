using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using SSW.SophieBot;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LuisCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddLuis<TSchema>(
            this IServiceCollection services, 
            IConfiguration configuration,
            Action<RecognizerSchemaServiceOptions> action = null)
            where TSchema : class
        {
            var section = configuration.GetSection("Luis");

            services.AddOptions<LuisOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("Luis").Bind(options);
                });

            services.AddAzureClients(builder =>
            {
                builder.AddClient<ILUISAuthoringClient, LuisOptions>(_ =>
                    new LUISAuthoringClient(new ApiKeyServiceClientCredentials(section["AuthoringKey"]))
                    {
                        Endpoint = section["AuthoringEndpoint"]
                    });
            });

            action ??= options => options.UseManager<LuisSchemaManager>();
            services.AddRecognizerSchema<TSchema>(action);

            return services;
        }
    }
}
