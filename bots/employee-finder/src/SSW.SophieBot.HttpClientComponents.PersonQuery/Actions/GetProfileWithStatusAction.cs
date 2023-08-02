using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSW.SophieBot.Components;
using SSW.SophieBot.Components.Actions;
using SSW.SophieBot.HttpClientAction.Models;
using SSW.SophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Linq;
using System.Collections.Generic;
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
                Appointments = GetDisplayAppointments(EmployeesHelper.GetAppointments(employee.NormalizedAppointments, date, 10), date)
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

        private List<EmployeeProfileAppointment> GetDisplayAppointments(IEnumerable<GetAppointmentModel> sourceAppointments, DateTime date)
        {
            var result = new List<EmployeeProfileAppointment>();
            var appointment = new EmployeeProfileAppointment();
            if (!sourceAppointments.Any())
            {
                return result;
            }

            foreach (var sourceAppointment in sourceAppointments.OrderBy(sa => sa.Start.DateTime))
            {
                if (appointment.Subject != sourceAppointment.Subject)
                {
                    appointment = new EmployeeProfileAppointment
                    {
                        Start = sourceAppointment.Start.DateTime.ToUserFriendlyDate(date),
                        End = sourceAppointment.End.DateTime.ToUserFriendlyDate(date),
                        Duration = (sourceAppointment.End - sourceAppointment.Start).ToUserFriendlyDuration(),
                        BookingStatus = EmployeesHelper.GetBookingStatus(sourceAppointment),
                        Subject = sourceAppointment.Subject?.Trim() ?? string.Empty,
                        Regarding = sourceAppointment.Regarding?.Trim() ?? string.Empty
                    };

                    result.Add(appointment);
                }
                else
                {
                    appointment.End = sourceAppointment.End.DateTime.ToUserFriendlyDate(date);
                    appointment.Duration = (sourceAppointment.End - sourceAppointment.Start).ToUserFriendlyDuration();
                }
            }

            return result;
        }
    }
}
