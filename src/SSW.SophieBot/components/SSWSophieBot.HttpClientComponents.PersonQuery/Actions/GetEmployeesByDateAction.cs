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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
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
            static GetAppointmentModel GetAppointmentBy(DateTime date, List<GetAppointmentModel> appointments)
            {
               var results = appointments.Where(appointment => date.Ticks >= GetTicksFrom(appointment.Start) && date.Ticks <= GetTicksFrom(appointment.End)).ToList();
               return results.Count != 0 ? results[0] : null;
            }

            static long GetTicksFrom(DateTimeOffset date)
            {
                return date.UtcDateTime.Ticks;
            }

            var employees = dc.GetValue(Employees);
            var dateString = dc.GetValue(Date);

        

            var date = dateString != null ? DateTime.Parse(dateString).ToUniversalTime() : DateTime.Now.ToUniversalTime();

            var result = employees.Select(e => new EmployeeByDateModel{
                FirstName = e.FirstName,
                DisplayName = $"{e.FirstName} {e.LastName}",
                DefaultSite = e.DefaultSite,
                AvatarUrl = e.AvatarUrl,
                CurrentAppointment = GetAppointmentBy(date, e.Appointments),
                Title = e.Title,
            }).ToList();


            if (Result != null)
            {
                dc.State.SetValue(Result.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
