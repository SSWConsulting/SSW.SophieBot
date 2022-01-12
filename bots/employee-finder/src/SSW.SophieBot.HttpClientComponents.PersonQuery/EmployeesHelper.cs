using Microsoft.Bot.Builder.Dialogs;
using SSW.SophieBot.Components;
using SSW.SophieBot.HttpClientAction.Models;
using SSW.SophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery
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
            DateTime date,
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

            if (IsOnLeaveFunc(appointment))
            {
                return BookingStatus.Leave;
            }
            if (IsOnClientWorkFunc(appointment))
            {
                return BookingStatus.ClientWork;
            }
            if (IsOnInternalWorkFunc(appointment))
            {
                return BookingStatus.InternalWork;
            }

            return BookingStatus.Unknown;
        }

        public static DateTime? GetReturningDate(IEnumerable<GetAppointmentModel> appointments, DateTime date, DialogContext dc)
        {
            if (appointments == null || !appointments.Any())
            {
                return null;
            }

            var currentLeaveAppointment = GetCurrentLeaveAppointment(appointments, date);
            if (currentLeaveAppointment != null)
            {
                var futureAppointments = appointments.Where(a => a.End > currentLeaveAppointment.End);

                if (!futureAppointments.Any())
                {
                    return null;
                }

                var daysToCheck = (int)Math.Ceiling((futureAppointments.Max(a => a.End.Date) - date.Date).TotalDays);

                for (int i = 0; i <= daysToCheck; i++)
                {
                    var checkDate = date.AddDays(i);

                    var appointmentsWithinDate = GetEnumerableAppointmentsByDate(futureAppointments, checkDate);
                    if (appointmentsWithinDate.Any()
                        && !IsOnLeaveFunc(appointmentsWithinDate.OrderByDescending(appointment => GetOverlapsTimeSpan(appointment, date)).First()))
                    {
                        return checkDate.Date.ToUserLocalTime(dc);
                    }
                }
            }

            return null;
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
            if (IsOnLeave(employee, date))
            {
                return BookingStatus.Leave;
            }
            if (IsOnClientWork(employee, date))
            {
                return BookingStatus.ClientWork;
            }
            if (IsOnInternalWork(employee, date))
            {
                return BookingStatus.InternalWork;
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

        public static int GetBookedDays(GetEmployeeModel employee, DateTime startDate)
        {
            var maxEndTime = employee.NormalizedAppointments.Max(appointment => appointment.End);
            var checkDays = (int)Math.Ceiling((maxEndTime.Date - startDate.Date).TotalDays);
            var bookedDays = 0;

            for (int i = 0; i <= checkDays; i++)
            {
                if (IsOnClientWork(employee.NormalizedAppointments, startDate.AddDays(i)))
                {
                    bookedDays++;
                }
            }

            return bookedDays;
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

            var now = DateTimeOffset.UtcNow;
            var timeOffset = now - lastSeenDateTime.Value;

            if (timeOffset.TotalMinutes < 1)
            {
                var seconds = GetInteger(timeOffset.TotalSeconds);
                return $"{seconds} {(seconds == 1 ? "second" : "seconds")} ago";
            }
            else if (timeOffset.TotalHours < 1)
            {
                var minutes = GetInteger(timeOffset.TotalMinutes);
                return $"{minutes} {(minutes == 1 ? "minute" : "minutes")} ago";
            }
            else if (timeOffset.TotalDays < 1)
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
            freeDays = 0;
            if (!employee.NormalizedAppointments.Any())
            {
                return null;
            }

            var endTime = employee.NormalizedAppointments.Max(appointment => appointment.End);
            var checkDays = Math.Min((int)Math.Ceiling((endTime - date).TotalDays), 4 * 7);
            GetAppointmentModel unavailableAppointment = null;

            for (int i = 1; i <= checkDays; i++)
            {
                var checkDate = date.AddDays(i);
                if (TryGetUnavailableAppointment(employee.NormalizedAppointments, date, out unavailableAppointment))
                {
                    break;
                }
                else if (checkDate.DayOfWeek != DayOfWeek.Saturday && checkDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    freeDays++;
                }
            }

            if (string.IsNullOrEmpty(unavailableAppointment?.Regarding))
            {
                return null;
            }

            return new NextClientModel
            {
                Name = unavailableAppointment.Regarding,
                Date = unavailableAppointment.Start.DateTime,
                DateText = unavailableAppointment.Start.ToString("ddd, dd/MM/yyyy"),
                Type = IsOnLeaveFunc(unavailableAppointment) ? BookingStatus.Leave : BookingStatus.ClientWork
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
            return GetEnumerableAppointmentsByDate(appointments, date)
                .Where(appointment => !_internalCompanyNames.Contains(appointment.Regarding?.ToLower()))
                .Select(appointment => appointment.Regarding)
                .Distinct()
                .ToList();
        }

        public static GetAppointmentModel GetCurrentLeaveAppointment(IEnumerable<GetAppointmentModel> appointments, DateTime date)
        {
            return appointments
                .Where(a => a.Start.Ticks <= date.Ticks && a.End.Ticks >= date.Ticks)
                .OrderByDescending(a => a.End)
                .FirstOrDefault(IsOnLeaveFunc);
        }

        public static bool IsOnClientWork(GetEmployeeModel employee, DateTime date)
        {
            return IsOnClientWork(employee.NormalizedAppointments, date);
        }

        public static bool IsOnClientWork(IEnumerable<GetAppointmentModel> appointments, DateTime date)
        {
            var appointmentsWithinDate = GetEnumerableAppointmentsByDate(appointments, date);
            return appointmentsWithinDate.Any()
                && IsOnClientWorkFunc(appointmentsWithinDate.OrderByDescending(appointment => GetOverlapsTimeSpan(appointment, date)).First());
        }

        public static bool TryGetUnavailableAppointment(
            IEnumerable<GetAppointmentModel> appointments,
            DateTime date,
            out GetAppointmentModel unavailableAppointment)
        {
            unavailableAppointment = null;
            var appointmentsWithinDate = GetEnumerableAppointmentsByDate(appointments, date);

            if (appointmentsWithinDate.Any())
            {
                var targetAppointment = appointmentsWithinDate
                    .OrderByDescending(appointment => GetOverlapsTimeSpan(appointment, date))
                    .First();

                if (IsOnLeaveFunc(targetAppointment) || IsOnClientWorkFunc(targetAppointment))
                {
                    unavailableAppointment = targetAppointment;
                    return true;
                }
            }

            return false;
        }

        public static bool IsOnInternalWork(GetEmployeeModel employee, DateTime date)
        {
            return IsOnInternalWork(employee.NormalizedAppointments, date);
        }

        public static bool IsOnInternalWork(IEnumerable<GetAppointmentModel> appointments, DateTime date)
        {
            var appointmentsWithinDate = GetEnumerableAppointmentsByDate(appointments, date);
            return appointmentsWithinDate.Any()
                && IsOnInternalWorkFunc(appointmentsWithinDate.OrderByDescending(appointment => GetOverlapsTimeSpan(appointment, date)).First());
        }

        public static bool IsOnLeave(GetEmployeeModel employee, DateTime date)
        {
            return IsOnLeave(employee.NormalizedAppointments, date);
        }

        public static bool IsOnLeave(IEnumerable<GetAppointmentModel> appointments, DateTime date)
        {
            var appointmentsWithinDate = GetEnumerableAppointmentsByDate(appointments, date);
            return appointmentsWithinDate.Any()
                && IsOnLeaveFunc(appointmentsWithinDate.OrderByDescending(appointment => GetOverlapsTimeSpan(appointment, date)).First());
        }

        public static bool IsFree(GetEmployeeModel employee, DateTime date)
        {
            return !GetEnumerableAppointmentsByDate(employee.NormalizedAppointments, date).Any();
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
                    && date.Date <= appointment.End.Date
                    && date.Date.AddDays(1) > appointment.Start.Date);
        }

        public static DateTime GetFreeDate(IEnumerable<GetAppointmentModel> appointments, DateTime startTime)
        {
            static DateTime GetNextWorkDay(DateTime startDate)
            {
                if (startDate.DayOfWeek == DayOfWeek.Friday)
                {
                    return startDate.AddDays(3);
                }

                if (startDate.DayOfWeek == DayOfWeek.Saturday)
                {
                    return startDate.AddDays(2);
                }

                return startDate.AddDays(1);
            }

            if (appointments == null || !appointments.Any())
            {
                return GetNextWorkDay(startTime);
            }

            var checkDate = startTime.Date;
            var appointmentsEndDate = appointments.Max(appointment => appointment.End);

            while (checkDate <= appointmentsEndDate.Date)
            {
                var checkAppointments = GetEnumerableAppointmentsByDate(appointments, checkDate);
                if (!checkAppointments.Any() || IsOnInternalWork(appointments, checkDate))
                {
                    return checkDate;
                }

                checkDate = GetNextWorkDay(checkDate);
            }

            return checkDate;
        }

        public static string GetFormatedTimeDuration(DateTime startDate, DateTime endDate)
        {
            static int GetInteger(double value)
            {
                return Math.Max(1, (int)Math.Floor(value));
            }

            static string GetDayString(double days)
            {
                return $"{days} {(days == 1 ? "day" : "days")}";
            }

            var timeOffset = endDate - startDate;

            if (timeOffset.TotalDays <= 2)
            {
                return String.Empty;
            }

            var totalDays = timeOffset.TotalDays;

            if (totalDays < 7)
            {
                return $"{GetDayString(totalDays)} from now";
            }
            else if (totalDays < 30)
            {
                var weeks = GetInteger(totalDays / 7);
                var days = totalDays % 7;

                var weekString = $"{weeks} {(weeks == 1 ? "week" : "weeks")}";

                if (days == 0)
                {
                    return $"{weekString} from now";
                }

                return $"{weekString} and {GetDayString(days)} from now";
            }
            else
            {
                var monthes = GetInteger(totalDays / 30);
                var days = totalDays % 30;

                var monthString = $"{monthes} {(monthes == 1 ? "month" : "months")}";

                if (days == 0)
                {
                    return $"{monthString} from now";
                }

                return $"{monthString} and {GetDayString(days)} from now";
            }
        }

        private static bool IsOnClientWorkFunc(GetAppointmentModel appointment)
        {
            return !_internalCompanyNames.Contains(appointment.Regarding?.ToLower());
        }

        private static bool IsOnInternalWorkFunc(GetAppointmentModel appointment)
        {
            return _internalCompanyNames.Contains(appointment.Regarding?.ToLower())
                && !IsOnLeaveFunc(appointment);
        }

        private static bool IsOnLeaveFunc(GetAppointmentModel appointment)
        {
            return appointment.RequiredAttendees.Any(attendee => attendee.ToLower().Contains("absence"));
        }

        private static TimeSpan GetOverlapsTimeSpan(GetAppointmentModel appointment, DateTime date)
        {
            if (appointment == null)
            {
                return TimeSpan.Zero;
            }

            var currentDate = date.Date;
            var nextDate = date.Date.AddDays(1);

            if (appointment.End < currentDate || appointment.Start > nextDate)
            {
                return TimeSpan.Zero;
            }

            if (appointment.Start.DateTime <= currentDate)
            {
                return appointment.End <= nextDate ? appointment.End - currentDate : nextDate - currentDate;
            }
            else
            {
                return appointment.End <= nextDate ? appointment.End - appointment.Start : nextDate - appointment.Start;
            }
        }
    }
}
