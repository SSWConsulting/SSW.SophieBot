using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSW.SophieBot.Components.Actions;

namespace SSW.SophieBot.Components.Components
{
    public class CommonComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDeclarativeType<PersonNameAlterateAction>(PersonNameAlterateAction.Kind);
            services.AddDeclarativeType<StringJoinAction>(StringJoinAction.Kind);
            services.AddDeclarativeType<UtcConvertAction>(UtcConvertAction.Kind);
            services.AddDeclarativeType<ClientNowAction>(ClientNowAction.Kind);
            services.AddDeclarativeType<TimeFormatAction>(TimeFormatAction.Kind);
            services.AddDeclarativeType<TimeDifferenceAction>(TimeDifferenceAction.Kind);
            services.AddDeclarativeType<EnrichDatatimeAction>(EnrichDatatimeAction.Kind);
        }
    }
}
