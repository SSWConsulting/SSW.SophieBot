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

            var date = DateTime.Now.ToUserLocalTime(dc);

            var result = employees.Select(e => ConvertToProfile(e, dc, date)).ToList();

            if (Result != null)
            {
                dc.State.SetValue(Result.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }

        private EmployeeProfileWithStatusModel ConvertToProfile(GetEmployeeModel employee, DialogContext dc, DateTime date)
        {
            employee.NormalizeAppointments(dc);

            var profile = new EmployeeProfileWithStatusModel
            {
                UserId = employee.UserId,
                AvatarUrl = employee.AvatarUrl,
                DisplayName = $"{employee.FirstName} {employee.LastName}",
                Title = employee.Title,
                Clients = EmployeesHelper.GetClientsByDate(date, employee.NormalizedAppointments),
                BookingStatus = EmployeesHelper.GetBookingStatus(employee, date),
                LastSeenAt = employee.LastSeenAt,
                LastSeenTime = EmployeesHelper.GetLastSeen(employee),
                Skills = employee.Skills,
                EmailAddress = employee.EmailAddress,
                MobilePhone = employee.MobilePhone,
                DefaultSite = employee.DefaultSite,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                BillableRate = employee.BillableRate,
                BookedDays = EmployeesHelper.GetBookedDays(employee, date),
                Appointments = EmployeesHelper.GetAppointments(employee.NormalizedAppointments, date, 10)
                    .Select(a => new EmployeeProfileAppointment
                    {
                        Start = a.Start.DateTime.ToUserFriendlyDate(date),
                        End = a.End.DateTime.ToUserFriendlyDate(date),
                        Duration = (a.End - a.Start).ToUserFriendlyDuration(),
                        BookingStatus = EmployeesHelper.GetBookingStatus(a),
                        Subject = a.Subject.Trim(),
                        Regarding = a.Regarding.Trim()
                    })
                    .ToList()
            };

            if (profile.BookingStatus == BookingStatus.Leave)
            {
                var returningDate = EmployeesHelper.GetReturningDate(employee.NormalizedAppointments, date, dc);

                if (returningDate.HasValue)
                {
                    var additionalStatus = $"back on {returningDate.Value.ToUserFriendlyDate(date, dayOfWeek: false)}";

                    var timeInterval = date.GetUserFriendlyTimeInterval(returningDate.Value);
                    if (!string.IsNullOrEmpty(timeInterval))
                    {
                        additionalStatus += $" ({timeInterval})";
                    }
                    profile.AdditionalStatus = additionalStatus;
                }
            }

            return profile;
        }
    }
}
