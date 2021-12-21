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

            var result = employees.Select(e => new EmployeeProfileWithStatusModel
            {
                UserId = e.UserId,
                AvatarUrl = e.AvatarUrl,
                DisplayName = $"{e.FirstName} {e.LastName}",
                Title = e.Title,
                Clients = EmployeesHelper.GetClientsByDate(date, e.Appointments),
                BookingStatus = EmployeesHelper.GetBookingStatus(e, date),
                LastSeenAt = e.LastSeenAt,
                LastSeenTime = EmployeesHelper.GetLastSeen(e),
                Skills = e.Skills,
                EmailAddress = e.EmailAddress,
                MobilePhone = e.MobilePhone,
                DefaultSite = e.DefaultSite,
                FirstName = e.FirstName,
                LastName = e.LastName,
                BillableRate = e.BillableRate,
                BookedDays = e.Appointments
                    .Where(appointment => appointment.End.UtcTicks >= date.Ticks)
                    .Count(appointment => EmployeesHelper.IsOnClientWorkFunc(appointment)),
                Appointments = EmployeesHelper.GetAppointments(e.Appointments, date, 10)
                    .Select(a => new EmployeeProfileAppointment
                    {
                        Start = a.Start.DateTime.ToUserLocalTime(dc).ToUserFriendlyDate(date),
                        Duration = (a.End - a.Start).ToUserFriendlyDuration(),
                        BookingStatus = EmployeesHelper.GetBookingStatus(a),
                        Subject = a.Subject.Trim(),
                        Regarding = a.Regarding.Trim()
                    })
                    .ToList()
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
