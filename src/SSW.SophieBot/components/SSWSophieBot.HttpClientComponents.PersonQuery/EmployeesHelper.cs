using Microsoft.Bot.Builder.Dialogs;
using SSWSophieBot.Components;
using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SSWSophieBot.HttpClientComponents.PersonQuery
{
    public static class EmployeesHelper
    {
        private static readonly string[] _internalCompanyNames = new[]
        {
            "ssw",
            "ssw test"
        };

        public static List<EmployeeBillableItemModel> GetBillableEmployees(
            IEnumerable<GetEmployeeModel> employees,
            string queriedProjectName,
            bool isProject,
            out string projectName)
        {
            GetEmployeeProjectModel project;
            if (string.IsNullOrWhiteSpace(queriedProjectName))
            {
                project = null;
            }
            else
            {
                project = employees?.FirstOrDefault()?.Projects?.FirstOrDefault(p => IsProjectNameMatch(queriedProjectName, isProject ? p.ProjectName : p.CustomerName));
            }
            projectName = isProject ? project?.ProjectName : project?.CustomerName;

            var date = DateTime.Now.ToUniversalTime();
            return employees
                .Select(e => new EmployeeBillableItemModel
                {
                    UserId = e.UserId,
                    AvatarUrl = e.AvatarUrl,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    DisplayName = $"{e.FirstName} {e.LastName}",
                    BilledDays = GetBilledDays(e, project),
                    BookingStatus = GetBookingStatus(e, date),
                    LastSeen = GetLastSeen(e)
                })
                .OrderByDescending(i => i.BilledDays)
                .ToList();
        }

        public static BookingStatus GetBookingStatus(GetAppointmentModel appointment)
        {
            if (appointment == null)
            {
                return BookingStatus.Unknown;
            }

            if (IsOnClientWorkFunc(appointment))
            {
                return BookingStatus.ClientWork;
            }
            if (IsOnInternalWorkFunc(appointment))
            {
                return BookingStatus.InternalWork;
            }
            if (IsOnLeaveFunc(appointment))
            {
                return BookingStatus.Leave;
            }

            return BookingStatus.Unknown;
        }

        public static BookingStatus GetBookingStatus(GetEmployeeModel employee, DateTime date)
        {
            if (employee == null)
            {
                return BookingStatus.Unknown;
            }

            if (IsFree(employee, date))
            {
                return BookingStatus.Free;
            }
            if (IsOnClientWork(employee, date))
            {
                return BookingStatus.ClientWork;
            }
            if (IsOnInternalWork(employee, date))
            {
                return BookingStatus.InternalWork;
            }
            if (IsOnLeave(employee, date))
            {
                return BookingStatus.Leave;
            }

            return BookingStatus.Unknown;
        }

        public static int GetBilledDays(GetEmployeeModel employee, GetEmployeeProjectModel project)
        {
            double billableHours = 0;
            if (project != null)
            {
                billableHours = project?.BillableHours ?? 0;
            }
            else
            {
                billableHours = employee.Projects.Where(project => !project.CustomerName.Equals("ssw", StringComparison.OrdinalIgnoreCase)).Sum(p => p.BillableHours);
            }

            return billableHours == 0 ? 0 : (int)Math.Ceiling(billableHours / 8);
        }

        public static string GetLastSeen(GetEmployeeModel model)
        {
            var lastSeenDateTime = model.LastSeenAt?.LastSeen.ToUniversalTime();

            if (!lastSeenDateTime.HasValue)
            {
                return string.Empty;
            }

            static int GetInteger(double value)
            {
                return Math.Max(1, (int)Math.Floor(value));
            }

            var lastSeenDateTimeValue = lastSeenDateTime.Value;
            var now = DateTimeOffset.UtcNow;

            var timeOffset = now - lastSeenDateTimeValue;
            if (timeOffset.TotalDays < 1)
            {
                var hours = GetInteger(timeOffset.TotalHours);
                return $"{hours} {(hours == 1 ? "hour" : "hours")} ago";
            }
            else if (timeOffset.TotalDays < 30)
            {
                var days = GetInteger(timeOffset.TotalDays);
                return $"{days} {(days == 1 ? "day" : "days")} ago";
            }
            else
            {
                var months = GetInteger(timeOffset.TotalDays / 30);
                return $"{months} {(months == 1 ? "month" : "months")} ago";
            }
        }

        public static NextClientModel GetNextUnavailability(GetEmployeeModel employee, DateTime date, out int freeDays)
        {
            const int daysForNext4Weeks = 4 * 7;
            const int daysFor4Weekends = 4 * 2;

            freeDays = daysForNext4Weeks - daysFor4Weekends;
            var unfreeAppointments = new List<GetAppointmentModel>();
            var lastDate = DateTimeOffset.MinValue;

            var startDate = date;
            var endDate = date.AddDays(daysForNext4Weeks);

            foreach (var a in employee.Appointments
                .Where(a => a.Start.UtcTicks >= startDate.Ticks
                    && a.End.UtcTicks < endDate.Ticks
                    && !string.IsNullOrWhiteSpace(a.Regarding))
                .OrderBy(a => a.Start.UtcTicks))
            {
                if (!IsOnInternalWorkFunc(a) && !IsOnLeaveFunc(a))
                {
                    unfreeAppointments.Add(a);

                    if (a.End.UtcDateTime >= lastDate)
                    {
                        var unfreeDays = (int)Math.Ceiling((a.End.UtcDateTime - (a.Start.UtcDateTime < lastDate ? lastDate : a.Start.UtcDateTime)).TotalHours / 24);

                        freeDays -= unfreeDays;
                        lastDate = a.End.UtcDateTime;
                    }
                }
            }

            var appointment = unfreeAppointments.FirstOrDefault();

            if (appointment?.Regarding == null)
            {
                return null;
            }

            return new NextClientModel
            {
                Name = appointment.Regarding,
                Date = appointment.Start.ToString("ddd, dd/MM/yyyy"),
                Type = IsOnLeaveFunc(appointment) ? BookingStatus.Leave : BookingStatus.ClientWork
            };
        }

        public static IEnumerable<GetAppointmentModel> GetAppointments(IEnumerable<GetAppointmentModel> appointments, DateTime startTime, int maxCount = int.MaxValue)
        {
            return appointments
                .Where(a => !string.IsNullOrWhiteSpace(a.Regarding) && a.End.UtcTicks >= startTime.Ticks)
                .OrderBy(a => a.Start)
                .Take(maxCount);
        }

        public static List<string> GetClientsByDate(DateTime date, List<GetAppointmentModel> appointments)
        {
            var clientAppointments = GetEnumerableAppointmentsByDate(appointments, date)
                .Where(appointment => !_internalCompanyNames.Contains(appointment.Regarding?.ToLower()))
                .Select(appointment => appointment.Regarding)
                .Distinct();

            return clientAppointments.Any()
                ? clientAppointments.ToList()
                : null;
        }

        public static bool IsOnClientWork(GetEmployeeModel employee, DateTime date)
        {
            return GetEnumerableAppointmentsByDate(employee.Appointments, date)
                .Any(IsOnClientWorkFunc);
        }

        public static bool IsOnClientWorkFunc(GetAppointmentModel appointment)
        {
            return !_internalCompanyNames.Contains(appointment.Regarding?.ToLower());
        }

        public static bool IsOnInternalWorkFunc(GetAppointmentModel appointment)
        {
            return _internalCompanyNames.Contains(appointment.Regarding?.ToLower())
                && !IsOnLeaveFunc(appointment);
        }

        public static bool IsOnLeaveFunc(GetAppointmentModel appointment)
        {
            return appointment.RequiredAttendees.Any(attendee => attendee.ToLower().Contains("absence"));
        }

        public static bool IsOnInternalWork(GetEmployeeModel employee, DateTime date)
        {
            return GetEnumerableAppointmentsByDate(employee.Appointments, date)
                .Any(IsOnInternalWorkFunc);
        }

        public static bool IsOnLeave(GetEmployeeModel employee, DateTime date)
        {
            var appointments = GetEnumerableAppointmentsByDate(employee.Appointments, date);
            return appointments.Any() && appointments
                .All(IsOnLeaveFunc);
        }

        public static bool IsFree(GetEmployeeModel employee, DateTime date)
        {
            return !GetEnumerableAppointmentsByDate(employee.Appointments, date).Any();
        }

        public static DateTime ToUserLocalTime(DialogContext dc, DateTime dateTime)
        {
            var serverLocalTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            var utcOffset = dc.Context.Activity.LocalTimestamp.GetValueOrDefault().Offset;
            return serverLocalTime.Subtract(utcOffset);
        }

        private static long GetTicksFrom(DateTimeOffset date)
        {
            return date.UtcDateTime.Date.Ticks;
        }

        public static bool IsProjectNameMatch(string sourceProjectName, string originalProjectName)
        {
            if (string.IsNullOrWhiteSpace(originalProjectName) && string.IsNullOrWhiteSpace(sourceProjectName))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(originalProjectName) || string.IsNullOrWhiteSpace(sourceProjectName))
            {
                return false;
            }

            var _rgx = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
            var prjName = _rgx.Replace(sourceProjectName, string.Empty);
            return _rgx.Replace(originalProjectName, string.Empty).IndexOf(prjName, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static List<GetEmployeeModel> FilterDevelopers(List<GetEmployeeModel> employees)
        {
            var devCategories = new[] { ProfileCategory.Developers, ProfileCategory.Designers };
            return employees.Where(employee => devCategories.Any(category => category == employee.ProfileCategory)).ToList();
        }

        public static List<GetEmployeeModel> GetInternalBookedEmployees(List<GetEmployeeModel> employees, DateTime date)
        {
            return employees
                .Where(employee => IsOnInternalWork(employee, date))
                .ToList();
        }

        public static IEnumerable<GetAppointmentModel> GetEnumerableAppointmentsByDate(IEnumerable<GetAppointmentModel> appointments, DateTime date)
        {
            if (appointments == null || !appointments.Any())
            {
                return Enumerable.Empty<GetAppointmentModel>();
            }

            return appointments
                .Where(appointment =>
                    !string.IsNullOrWhiteSpace(appointment.Regarding)
                    && date.Date.Ticks <= GetTicksFrom(appointment.End)
                    && date.Date.Ticks >= GetTicksFrom(appointment.Start));
        }

        public static string GetFreeDate(IEnumerable<GetAppointmentModel> appointments, DateTime startTime, bool isFree = true, DateTime? now = null)
        {
            var checkDate = startTime.Date;

            var appointmentsEndDate = appointments.Max(appointment => appointment.End);

            while (checkDate <= appointmentsEndDate)
            {
                var checkAppointments = GetEnumerableAppointmentsByDate(appointments, checkDate);
                if (isFree
                    ? !checkAppointments.Any(appointment => !IsOnInternalWorkFunc(appointment))
                    : checkAppointments.Any(appointment => !IsOnInternalWorkFunc(appointment)))
                {
                    return checkDate.ToUserFriendlyDate(now);
                }

                do
                {
                    checkDate = checkDate.AddDays(1);
                } while (checkDate.DayOfWeek == DayOfWeek.Saturday || checkDate.DayOfWeek == DayOfWeek.Sunday);
            }

            return isFree ? appointmentsEndDate.DateTime.AddDays(1).ToUserFriendlyDate() : string.Empty;
        }
    }
}
