using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using SSW.SophieBot;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LuisCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddLuis(this IServiceCollection services, IConfiguration configuration)
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

            return services;
        }
    }
}
