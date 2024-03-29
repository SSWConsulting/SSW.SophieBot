﻿using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SSW.SophieBot.Components;
using SSW.SophieBot.HttpClientComponents.PersonQuery.Actions;
using SSW.SophieBot.HttpClientComponents.PersonQuery.Clients;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Components
{
    public class HttpClientComponent : BotComponent
    {
        public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddBotApplicationService<GetProfileClient>()
                .AddBotApplicationService<GetOrganisationsClient>()
                .AddBotApplicationService<IAvatarManager>();

            services.AddDeclarativeType<GetProfileAction>(GetProfileAction.Kind);
            services.AddDeclarativeType<GetProjectsFromQueryAction>(GetProjectsFromQueryAction.Kind);
            services.AddDeclarativeType<GetOrganisationsAction>(GetOrganisationsAction.Kind);
            services.AddDeclarativeType<GetEmployeesByDateAction>(GetEmployeesByDateAction.Kind);
            services.AddDeclarativeType<GetEmployeesByBillableAction>(GetEmployeesByBillableAction.Kind);
            services.AddDeclarativeType<GetGroupedEmployeesAction>(GetGroupedEmployeesAction.Kind);
            services.AddDeclarativeType<GetFreeEmployeesAction>(GetFreeEmployeesAction.Kind);
            services.AddDeclarativeType<GetInternalBookedEmployeesAction>(GetInternalBookedEmployeesAction.Kind);
            services.AddDeclarativeType<GetProfileWithStatusAction>(GetProfileWithStatusAction.Kind);
            services.AddDeclarativeType<GetProfileWithBookingInfoAction>(GetProfileWithBookingInfoAction.Kind);
            services.AddDeclarativeType<GetEmployeesWithFreeDateAction>(GetEmployeesWithFreeDateAction.Kind);
        }
    }
}
