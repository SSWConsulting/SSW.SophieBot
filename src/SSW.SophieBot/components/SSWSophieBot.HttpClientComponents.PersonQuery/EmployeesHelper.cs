using Microsoft.Bot.Builder.Dialogs;
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
        private static readonly string[] _leavePhrases = new string[]
        {
            "non working",
            "non-working",
            "non-work",
            "holiday",
            "time in lieu",
            "day off",
            "days off",
            "leave",
            "study days",
            "uni days"
        };

        private static readonly string[] _nonDevTitles = new string[]
        {
            "SSW Admin",
            "Senior Marketing Specialist",
            "SSW Senior Accountant",
            "SSW China CEO",
            "SSW Electrician",
            "SSW QLD State",
            "International Manager",
            "SSW VIC State Manager",
            "SSW Administrative Assistant",
            "SSW General Manager",
            "SSW Multimedia Assistant",
            "SSW Multimedia Specialist",
            "SSW Multimedia Videographer",
            "SSW Chief Architect",
            "Microsoft Regional Director",
            "SSW Lawyer",
            "SSW Chief Financial Controller",
            "SSW Videographer",
            "SSW Producer, Director and Editor",
            "Administrative Assistant",
            "SSW Digital Marketing",
            "SSW Admin Manager"
        };

        private static readonly string[] _internalCompanyNames = new string[]
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
                project = employees?.FirstOrDefault()?.Projects?.FirstOrDefault(p => IsProjectNameEqual(queriedProjectName, isProject ? p.ProjectName : p.CustomerName));
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
                billableHours = employee.Projects.Sum(p => p.BillableHours);
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
            var daysTillNextWeek = (int)DayOfWeek.Saturday - (int)date.DayOfWeek + 1;

            freeDays = daysForNext4Weeks;
            var unfreeAppointments = new List<GetAppointmentModel>();
            var lastDate = DateTimeOffset.MinValue;

            var startDate = date.AddDays(daysTillNextWeek);
            var endDate = date.AddDays(daysTillNextWeek + daysForNext4Weeks);

            foreach (var a in employee.Appointments
                .Where(a => a.Start.UtcTicks >= startDate.Ticks
                    && a.End.UtcTicks < endDate.Ticks
                    && !string.IsNullOrWhiteSpace(a.Regarding))
                .OrderBy(a => a.Start.UtcTicks))
            {
                if (!IsOnInternalWorkFunc(a))
                {
                    unfreeAppointments.Add(a);

                    if (a.End.Date >= lastDate)
                    {
                        var unfreeDays = (int)Math.Ceiling((a.End.Date.AddDays(1) - (a.Start.Date < lastDate ? lastDate : a.Start.Date)).TotalDays);

                        freeDays -= unfreeDays;
                        lastDate = a.End.Date.AddDays(1);
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

        public static List<string> GetClientsByDate(DateTime date, List<GetAppointmentModel> appointments)
        {
            var clientAppointments = GetEnumerableAppointmentsByDate(appointments, date)
                .Where(appointment => !_internalCompanyNames.Contains(appointment.Regarding.ToLower()))
                .Select(appointment => appointment.Regarding)
                .Distinct();

            return clientAppointments.Any()
                ? clientAppointments.ToList()
                : null;
        }

        public static bool IsOnClientWork(GetEmployeeModel employee, DateTime date)
        {
            return GetEnumerableAppointmentsByDate(employee.Appointments, date)
                .Any(appointment => !_internalCompanyNames.Contains(appointment.Regarding.ToLower()));
        }

        private static bool IsOnInternalWorkFunc(GetAppointmentModel appointment)
        {
            return _internalCompanyNames.Contains(appointment.Regarding.ToLower())
                && !_leavePhrases.Any(appointment.Subject.ToLower().Contains);
        }

        private static bool IsOnLeaveFunc(GetAppointmentModel appointment)
        {
            return _internalCompanyNames.Contains(appointment.Regarding.ToLower())
                && _leavePhrases.Any(appointment.Subject.ToLower().Contains);
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

        private static bool IsProjectNameEqual(string sourceProjectName, string originalProjectName)
        {
            return sourceProjectName?.Trim() == originalProjectName?.Trim();
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
            return employees.Where(employee => !_nonDevTitles.Any(employee.Title.Contains)).ToList();
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
                    && date.Date.Ticks >= GetTicksFrom(appointment.Start)
                    && date.Date.Ticks <= GetTicksFrom(appointment.End));
        }
    }
}
