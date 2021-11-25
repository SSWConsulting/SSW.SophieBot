using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSWSophieBot.HttpClientComponents.PersonQuery;

namespace SSWSophieBot.HttpClientComponents.Abstractions
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
