using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSWSophieBot.Components.Actions;

namespace SSWSophieBot.Components.Components
{
    public class Component : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDeclarativeType<StringJoinAction>(StringJoinAction.Kind);
        }
    }
}
