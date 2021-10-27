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
    public class GetProfileWithBookingInfoAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetProfileWithBookingInfoAction";

        public GetProfileWithBookingInfoAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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

            var result = employees.Select(employee => new EmployeeWithBookingInfoModel
            {
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                AvatarUrl = employee.AvatarUrl,
                DisplayName = $"{employee.FirstName} {employee.LastName}",
                BookingStatus = EmployeesHelper.GetBookingStatus(employee, date),
                Clients = EmployeesHelper.GetClientsByDate(date, employee.Appointments),
                LastSeenAt = employee.LastSeenAt,
                LastSeenTime = EmployeesHelper.GetLastSeen(employee),
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
