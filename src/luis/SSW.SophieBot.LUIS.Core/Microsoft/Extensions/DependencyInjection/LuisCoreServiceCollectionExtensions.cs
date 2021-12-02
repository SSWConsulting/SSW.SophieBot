using Microsoft.Extensions.Configuration;
using SSW.SophieBot;

namespace Microsoft.Extensions.DependencyInjection
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
