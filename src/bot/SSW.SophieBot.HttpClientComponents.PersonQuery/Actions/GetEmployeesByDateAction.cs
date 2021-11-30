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
    public class GetEmployeesByDateAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetEmployeesByDateAction";

        public GetEmployeesByDateAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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

            var date = dateString != null && dateString != ""
                ? DateTime.Parse(dateString).FromUserLocalTime(dc).AddHours(9)
                : DateTime.Now.ToUniversalTime();

            var result = EmployeesHelper.FilterDevelopers(employees).Select(e => new EmployeeByDateModel
            {
                FirstName = e.FirstName,
                LastName = e.LastName,
                DefaultSite = e.DefaultSite,
                AvatarUrl = e.AvatarUrl,
                DisplayName = $"{e.FirstName} {e.LastName}",
                Clients = EmployeesHelper.GetClientsByDate(date, e.Appointments),
                Title = e.Title,
            })
            .Where(employee => employee.Clients != null)
            .ToList();


            if (Result != null)
            {
                dc.State.SetValue(Result.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
