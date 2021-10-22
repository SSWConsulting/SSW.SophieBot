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
            "annual leave",
            "non working",
            "non-working",
            "non-work",
            "leave",
            "holiday",
            "time in lieu",
            "hour leave",
            "hours leave",
            "day off",
            "days off",
            "study days"
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

        public static List<EmployeeBillableItemModel> GetBillableEmployees(IEnumerable<GetEmployeeModel> employees, string queriedProjectName, out string projectName)
        {
            GetEmployeeProjectModel project;
            if (string.IsNullOrWhiteSpace(queriedProjectName))
            {
                project = null;
            }
            else
            {
                project = employees?.FirstOrDefault()?.Projects?.FirstOrDefault(p => IsProjectNameEqual(queriedProjectName, p.ProjectName));
            }
            projectName = project?.ProjectName;

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

        public static NextClientModel GetNextClient(GetEmployeeModel employee, DateTime date)
        {
            var appointments = employee.Appointments
                .Where(appointment => appointment.Start.UtcDateTime.Ticks > date.Ticks)
                .Where(appointment => appointment.Regarding?.ToLower() != "ssw" || (appointment.Regarding?.ToLower() == "ssw" && _leavePhrases.Any(appointment.Subject.ToLower().Contains)))
                .ToList();
            var appointment = appointments.LastOrDefault();

            if (appointments.Count == 0 || appointment.Regarding == null)
            {
                return null;
            }

            return new NextClientModel
            {
                FreeDays = GetBusinessDays(date, appointment.Start.UtcDateTime),
                Name = appointment.Regarding,
                Date = appointment.Start.ToString("ddd, dd/MM/yyyy")
            };

            static int GetBusinessDays(DateTime start, DateTime end)
            {
                double result =
                    1 + ((end - start).TotalDays * 5 -
                    (start.DayOfWeek - end.DayOfWeek) * 2) / 7;

                if (end.DayOfWeek == DayOfWeek.Saturday) result--;
                if (start.DayOfWeek == DayOfWeek.Sunday) result--;

                return (int)Math.Floor(result);
            }
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

        public static bool IsOnInternalWork(GetEmployeeModel employee, DateTime date)
        {
            return GetEnumerableAppointmentsByDate(employee.Appointments, date)
                .Any(appointment => _internalCompanyNames.Contains(appointment.Regarding.ToLower())
                    && !_leavePhrases.Any(appointment.Subject.ToLower().Contains));
        }

        public static bool IsOnLeave(GetEmployeeModel employee, DateTime date)
        {
            return GetEnumerableAppointmentsByDate(employee.Appointments, date)
                .All(appointment => _internalCompanyNames.Contains(appointment.Regarding.ToLower())
                    && _leavePhrases.Any(appointment.Subject.ToLower().Contains));
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
