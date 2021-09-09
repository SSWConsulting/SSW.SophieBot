using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSWSophieBot.Components;
using SSWSophieBot.HttpClientComponents.PersonQuery.Actions;
using SSWSophieBot.HttpClientComponents.PersonQuery.Clients;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Components
{
    public class HttpClientComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddBotApplicationService<GetProfileClient>()
                .AddBotApplicationService<IAvatarManager>();

            services.AddDeclarativeType<GetProfileAction>(GetProfileAction.Kind);
            services.AddDeclarativeType<GetEmployeesByDateAction>(GetEmployeesByDateAction.Kind);
            services.AddDeclarativeType<GetEmployeesByBillableAction>(GetEmployeesByBillableAction.Kind);
            services.AddDeclarativeType<GetGroupedEmployeesAction>(GetGroupedEmployeesAction.Kind);
        }
    }
}
