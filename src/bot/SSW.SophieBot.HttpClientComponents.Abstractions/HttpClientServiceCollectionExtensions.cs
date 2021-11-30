using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSW.SophieBot.HttpClientComponents.PersonQuery;

namespace SSW.SophieBot.HttpClientComponents.Abstractions
{
    public static class HttpClientServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureSophieBotHttpClient(this IServiceCollection services)
        {
            var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            return services.Configure<HttpClientOptions>(configuration.GetSection(HttpClientOptions.ConfigName));
        }
    }
}
