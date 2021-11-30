using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSW.SophieBot.Components;
using SSW.SophieBot.Components.Actions;
using SSW.SophieBot.HttpClientAction.Models;
using SSW.SophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetFreeEmployeesAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetFreeEmployeesAction";

        public GetFreeEmployeesAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("date")]
        public StringExpression Date { get; set; }

        [JsonProperty("result")]
        public StringExpression Result { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var employees = dc.GetValue(Employees);
            var dateString = dc.GetValue(Date);

            if (employees == null || !employees.Any())
            {
                throw new ArgumentNullException(nameof(Employees));
            }

            var date = !string.IsNullOrEmpty(dateString)
                ? DateTime.Parse(dateString).FromUserLocalTime(dc).AddHours(9)
                : DateTime.Now.ToUniversalTime();

            var result = EmployeesHelper.FilterDevelopers(employees).Select(employee =>
            {
                var nextUnavailability = EmployeesHelper.GetNextUnavailability(employee, date, out var freeDays);
                return new FreeEmployeeModel
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    DefaultSite = employee.DefaultSite,
                    AvatarUrl = employee.AvatarUrl,
                    DisplayName = $"{employee.FirstName} {employee.LastName}",
                    BilledDays = EmployeesHelper.GetBilledDays(employee, null),
                    InOffice = employee.InOffice,
                    LastSeen = EmployeesHelper.GetLastSeen(employee),
                    NextClient = nextUnavailability,
                    FreeDays = freeDays
                };
            })
            .OrderByDescending(employee => employee.BilledDays)
            .ToList();

            if (Result != null)
            {
                dc.State.SetValue(Result.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
