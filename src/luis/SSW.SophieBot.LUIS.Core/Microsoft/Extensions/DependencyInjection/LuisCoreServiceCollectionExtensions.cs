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
            //var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            //services.Configure<LuisOptions>(configuration.GetSection("Luis"));

            services.AddOptions<LuisOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("Luis").Bind(options);
                });

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
    }
}
