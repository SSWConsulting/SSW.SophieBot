using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SSW.SophieBot.LUIS.Core.DependencyInjection
{
    public static class LuisCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddLuis(this IServiceCollection services)
        {
            return services.ConfigureLuis();
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
