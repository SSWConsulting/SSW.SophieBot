using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SSWSophieBot.UtcConvertAction
{
    public class UtcConvertActionBotComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<UtcConvertAction>(UtcConvertAction.Kind));
        }
    }
}
