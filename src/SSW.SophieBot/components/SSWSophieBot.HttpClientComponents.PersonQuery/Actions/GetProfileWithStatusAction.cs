using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.Components.Actions;
using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetProfileWithStatusAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetProfileWithStatusAction";

        public GetProfileWithStatusAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("result")]
        public StringExpression Result { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var employees = dc.GetValue(Employees);

            var date = DateTime.Now.ToUniversalTime();

            var result = employees.Select(e => new EmployeeProfileModel
            {
                AvatarUrl = e.AvatarUrl,
                DisplayName = $"{e.FirstName} {e.LastName}",
                Title = e.Title,
                Clients = EmployeesHelper.GetClientsBy(date, e.Appointments),
                IsOnLeave = EmployeesHelper.GetIsOnLeaveBy(date, e.Appointments),
                LastSeenAt = e.LastSeenAt,
                LastSeenTime = EmployeesHelper.GetLastSeen(e),
                Skills = e.Skills,
                EmailAddress = e.EmailAddress,
                MobilePhone = e.MobilePhone,
                DefaultSite = e.DefaultSite,
                FirstName = e.FirstName,
                LastName = e.LastName
            })
            .ToList();

            if (Result != null)
            {
                dc.State.SetValue(Result.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
