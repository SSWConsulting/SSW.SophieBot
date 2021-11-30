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
    public class GetEmployeesWithFreeDateAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetEmployeesWithFreeDateAction";

        public GetEmployeesWithFreeDateAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("isFree")]
        public BoolExpression IsFree { get; set; }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("result")]
        public StringExpression Result { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var isFree = dc.GetValue(IsFree);
            var employees = dc.GetValue(Employees);

            var date = DateTime.Now.ToUniversalTime();

            var result = employees.Select(e => new EmployeeWithFreeDateModel
            {
                DisplayName = $"{e.FirstName} {e.LastName}",
                FirstName = e.FirstName,
                LastName = e.LastName,
                FreeDate = EmployeesHelper.GetFreeDate(e.Appointments, date, isFree, date.ToUserLocalTime(dc)),
                BookedDays = e.Appointments
                    .Where(appointment => appointment.End.UtcTicks >= date.Ticks)
                    .Count(appointment => EmployeesHelper.IsOnClientWorkFunc(appointment)),
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
