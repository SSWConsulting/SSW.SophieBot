using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.DependencyInjection;

namespace SSWSophieBot.Components
{
    public static class ComponentsServiceCollectionExtensions
    {
        public static IServiceCollection AddBotApplicationService<T>(this IServiceCollection services)
            where T : class
        {
            return services.AddSingleton<IMiddleware, RegisterClassMiddleware<T>>(
                sp => new RegisterClassMiddleware<T>(sp.GetRequiredService<T>()));
        }

        public static IServiceCollection AddDeclarativeType<T>(this IServiceCollection services, string kind)
        {
            return services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<T>(kind));
        }
    }
}
