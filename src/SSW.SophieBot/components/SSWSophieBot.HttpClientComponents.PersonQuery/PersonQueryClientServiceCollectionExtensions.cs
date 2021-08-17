using Microsoft.Extensions.DependencyInjection;
using SSWSophieBot.HttpClientComponents.PersonQuery.Clients;
using System;
using System.Net.Http;

namespace SSWSophieBot.HttpClientComponents.PersonQuery
{
    public static class PersonQueryClientServiceCollectionExtensions
    {
        public static IServiceCollection AddPersonQueryClient(this IServiceCollection services, Action<HttpClient> action = null)
        {
            return services.AddGetProfileClient(action);
        }

        public static IServiceCollection AddGetProfileClient(this IServiceCollection services, Action<HttpClient> action = null)
        {
            services.AddHttpClient<GetProfileClient>(c => action?.Invoke(c));
            return services;
        }
    }
}
