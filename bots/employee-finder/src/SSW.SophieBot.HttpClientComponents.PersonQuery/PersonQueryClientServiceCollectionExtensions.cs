using Microsoft.Extensions.DependencyInjection;
using SSW.SophieBot.HttpClientComponents.PersonQuery.Clients;
using System;
using System.Net.Http;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery
{
    public static class PersonQueryClientServiceCollectionExtensions
    {
        public static IServiceCollection AddPersonQueryClient(this IServiceCollection services, Action<HttpClient> action = null)
        {
            services.AddSingleton<IAvatarManager, GravatarManager>();
            return services
                .AddGetProfileClient(action)
                .AddGetOrganisationsClient(action);
        }

        public static IServiceCollection AddGetProfileClient(this IServiceCollection services, Action<HttpClient> action = null)
        {
            services.AddHttpClient<GetProfileClient>(c => action?.Invoke(c));
            return services;
        }

        public static IServiceCollection AddGetOrganisationsClient(this IServiceCollection services, Action<HttpClient> action = null)
        {
            services.AddHttpClient<GetOrganisationsClient>(c => action?.Invoke(c));
            return services;
        }
    }
}
