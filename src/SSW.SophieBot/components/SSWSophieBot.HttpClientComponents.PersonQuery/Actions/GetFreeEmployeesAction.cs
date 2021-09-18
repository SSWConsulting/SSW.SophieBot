using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.Components.Actions;
using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
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

        [JsonProperty("result")]
        public StringExpression Result { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var employees = dc.GetValue(Employees);
            if (employees == null || !employees.Any())
            {
                throw new ArgumentNullException(nameof(Employees));
            }

            var result = employees.Select(employee => new FreeEmployeeModel
            {
                FirstName = employee.FirstName,
                DefaultSite = employee.DefaultSite,
                AvatarUrl = employee.AvatarUrl,
                DisplayName = $"{employee.FirstName} {employee.LastName}",
                BilledDays = EmployeesHelper.GetBilledDays(employee, null),
                InOffice = employee.InOffice,
                LastSeen = EmployeesHelper.GetLastSeen(employee),
                NextClient = EmployeesHelper.GetNextClient(employee)
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
