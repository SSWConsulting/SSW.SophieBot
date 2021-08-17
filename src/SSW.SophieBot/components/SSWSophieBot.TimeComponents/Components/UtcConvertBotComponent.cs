using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSWSophieBot.TimeComponents.Actions;

namespace SSWSophieBot.TimeComponents.Components
{
    public class UtcConvertBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UtcConvertAction>(UtcConvertAction.Kind));
        }
    }
}
