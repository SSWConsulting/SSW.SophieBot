using Microsoft.Extensions.Configuration;
using SSW.SophieBot;
using Microsoft.Extensions.Azure;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LuisCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddLuis(this IServiceCollection services)
        {
            services.ConfigureLuis();
            services.AddAzureClients(builder =>
            {
                builder.AddClient<ILUISAuthoringClient, LuisOptions>(options => 
                    new LUISAuthoringClient(new ApiKeyServiceClientCredentials(options.AuthoringKey))
                    {
                        Endpoint = options.AuthoringEndpoint
                    });
            });

            return services;
        }

        public static IServiceCollection ConfigureLuis(this IServiceCollection services)
        {
            services.AddOptions<LuisOptions>()
                .Configure<IConfiguration>((settings, configuration) =>
                {
                    configuration.GetSection("Luis").Bind(settings);
                });

            return services;
        }
    }
}
