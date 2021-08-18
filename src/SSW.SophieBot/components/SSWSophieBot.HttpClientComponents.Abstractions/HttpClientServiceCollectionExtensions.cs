using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
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

        // TODO: extract to a base lib
        public static IServiceCollection AddBotApplicationService<T>(this IServiceCollection services)
            where T : class
        {
            return services.AddSingleton<IMiddleware, RegisterClassMiddleware<T>>(
                sp => new RegisterClassMiddleware<T>(sp.GetRequiredService<T>()));
        }

        // TODO: extract to a base lib
        public static IServiceCollection AddDeclarativeType<T>(this IServiceCollection services, string kind)
        {
            return services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<T>(kind));
        }
    }
}
