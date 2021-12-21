using Microsoft.Extensions.Configuration;
using SSW.SophieBot;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DataApiClientServiceCollectionExtensions
    {
        public static IServiceCollection AddDataApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            services.AddTransient<IPeopleApiClient, PeopleApiClient>();
            services.Configure<PeopleApiOptions>(configuration.GetSection("PeopleApi"));
            return services;
        }
    }
}
