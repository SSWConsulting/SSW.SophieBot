using Microsoft.Bot.Builder.Dialogs;
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
                .Select(e =>
                {
                    var employeeProjects = new List<GetEmployeeProjectModel>();
                    var billedProjects = new List<BilledProject>();
                    if (project != null)
                    {
                        var employeeProject = e.Projects?.FirstOrDefault(p => IsProjectNameMatch(queriedProjectName, isProject ? p.ProjectName : p.CustomerName));
                        employeeProjects.Add(employeeProject);
                    }
                    else
                    {
                        employeeProjects = e.Projects ?? new List<GetEmployeeProjectModel>();
                    }


                    foreach (var employeeProject in employeeProjects)
                    {
                        double billableHours = employeeProject.BillableHours;
                        var billedDays = GetBilledDays(e, employeeProject, out billableHours);
                        billedProjects.Add(new BilledProject
                        {
                            BilledDays = billedDays,
                            BilledHours = (int)billableHours,
                            ProjectName = employeeProject.ProjectName,
                            ProjectId = employeeProject.ProjectId,
                            CustomerName = employeeProject.CustomerName,
                            CrmProjectId = employeeProject.CrmProjectId
                        });
                    }

                    //TODO: evaluate is we still need this line of code, I think we can get rid of it
                    billedProjects = billedProjects.OrderByDescending(project => project.BilledHours).ToList();
                    double totalHours = billedProjects.Sum(project => project.BilledHours);
                    

                    return new EmployeeBillableItemModel
                    {
                        UserId = e.UserId,
                        AvatarUrl = e.AvatarUrl,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        DisplayName = $"{e.FirstName} {e.LastName}",
                        BilledDays = (int)Math.Ceiling(totalHours/ 8),
                        BilledHours = (int)totalHours,
                        BilledProjects = billedProjects,
                        BookingStatus = GetBookingStatus(e, date),
                        LastSeen = GetLastSeen(e)
                    };
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

            if (TryGetUnavailableAppointment(appointments, date, out var currentLeaveAppointment))
            {
                var futureAppointments = appointments.Where(a => a.End > currentLeaveAppointment.End);

                if (!futureAppointments.Any())
                {
                    return null;
                }

                var daysToCheck = (int)Math.Ceiling((futureAppointments.Max(a => a.End.Date) - date.Date).TotalDays);

                for (int i = 1; i <= daysToCheck; i++)
                {
                    var checkDate = date.Date.AddDays(i);

                    var appointmentsWithinDate = GetEnumerableAppointmentsByDate(futureAppointments, checkDate, false);
                    var mainAppointment = GetMainAppointmentWithinDate(appointmentsWithinDate, checkDate);

                    if (mainAppointment != null
                        && !IsOnLeaveFunc(mainAppointment))
                    {
                        return checkDate.Date;
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

        //This function is to
        // 1. decide if the the working hours are billable hours or internal hours(Client work or internal project)
        // 2. calculate the billable days according to the billable hours
        public static int GetBilledDays(GetEmployeeModel employee, GetEmployeeProjectModel project, out double billableHours)
        {


            if (project != null && project.CustomerName != "SSW")
            {
                billableHours = project?.BillableHours ?? 0;
            }
            else {
                billableHours = 0;
            }
         

            return billableHours == 0 ? 0 : (int)Math.Ceiling(billableHours / 8);
        }

        public static int GetBookedDays(GetEmployeeModel employee, DateTime startDate)
        {
            if (!employee.NormalizedAppointments.Any())
            {
                return 0;
            }

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
                return $"{seconds}{(seconds == 1 ? "s" : "s")} ago";
            }
            else if (timeOffset.TotalHours < 1)
            {
                var minutes = GetInteger(timeOffset.TotalMinutes);
                return $"{minutes}{(minutes == 1 ? "m" : "m")} ago";
            }
            else if (timeOffset.TotalDays < 1)
            {
                var hours = GetInteger(timeOffset.TotalHours);
                return $"{hours}{(hours == 1 ? "h" : "h")} ago";
            }
            else if (timeOffset.TotalDays < 30)
            {
                var days = GetInteger(timeOffset.TotalDays);
                return $"{days}{(days == 1 ? "d" : "d")} ago";
            }
            else
            {
                var months = GetInteger(timeOffset.TotalDays / 30);
                return $"{months} {(months == 1 ? "month" : "months")} ago";
            }
        }

        public static NextClientModel GetNextUnavailability(GetEmployeeModel employee, DateTime date, out int freeDays, bool startFromNextWeek = true)
        {
            freeDays = 0;
            if (!employee.NormalizedAppointments.Any())
            {
                return null;
            }

            var checkDays = 4 * 7;
            GetAppointmentModel unavailableAppointment = null;

            if (startFromNextWeek)
            {
                var offsetDaysTillNextWeek = 14 % ((int)date.DayOfWeek + 7);
                date = date.Date.AddDays(offsetDaysTillNextWeek);
            }

            for (int i = 0; i <= checkDays; i++)
            {
                var checkDate = date.AddDays(i);
                if (TryGetUnavailableAppointment(employee.NormalizedAppointments, checkDate, out var currentUnavailability))
                {
                    if (unavailableAppointment == null)
                    {
                        unavailableAppointment = currentUnavailability;
                    }
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
                .Where(a => a.End.UtcTicks >= startTime.Ticks)
                .OrderBy(a => a.Start)
                .Take(maxCount);
        }


        public static List<ClientsInfoModel> GetClientInfo(DateTime date, List<GetAppointmentModel> appointments)
        {
            var appointment = GetEnumerableAppointmentsByDate(appointments, date);

            var clientsInfo = appointment
			.Where(a => !_internalCompanyNames.Contains(a.Regarding?.ToLower()) && !string.IsNullOrWhiteSpace(a.Regarding) && a.End.UtcTicks >= date.Ticks)
            .Select(a => { 
				if(a.Client != null && a.Client.Any())
				{
					return new ClientsInfoModel
					{
						AccountId = a.Client[0].AccountId,
						ClientName = a.Client[0].ClientName,
						ClientNumber = a.Client[0].ClientNumber,
						PrimaryContactId = a.Client[0].PrimaryContactId,
						PrimaryContactName = a.Client[0].PrimaryContactName,
						PrimaryContactEmail = a.Client[0].PrimaryContactEmail,
						PrimaryContactPhone = a.Client[0].PrimaryContactPhone,
						AccountManagerId = a.Client[0].AccountManagerId,
						AccountManagerName = a.Client[0].AccountManagerName,
						Address = a.Client[0].Address,
						Industry = a.Client[0].Industry,
						IndustryCode = a.Client[0].IndustryCode
					};
				}
				else
				{
					return null;
				}
			})
			.Where(a => a != null)
            .Distinct()
            .ToList();

            return clientsInfo;
        }

        public static List<EmployeeProfileClient> GetClientsByDate(DateTime date, List<GetAppointmentModel> appointments)
        {
            return GetEnumerableAppointmentsByDate(appointments, date)
                .Where(appointment => !_internalCompanyNames.Contains(appointment.Regarding?.ToLower()))
                .Select(appointment => new EmployeeProfileClient
                {
                    Name = appointment.Regarding,
                    Number = appointment.ClientNumber,
                    AccountManager = appointment.AccountManagerName
                })
                .Distinct()
                .ToList();
        }

        public static bool IsOnClientWork(GetEmployeeModel employee, DateTime date)
        {
            return IsOnClientWork(employee.NormalizedAppointments, date);
        }

        public static bool IsOnClientWork(IEnumerable<GetAppointmentModel> appointments, DateTime date)
        {
            var appointmentsWithinDate = GetEnumerableAppointmentsByDate(appointments, date);
            var mainAppointment = GetMainAppointmentWithinDate(appointmentsWithinDate, date);
            return mainAppointment != null
                && IsOnClientWorkFunc(mainAppointment);
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
                var targetAppointment = GetMainAppointmentWithinDate(appointmentsWithinDate, date);
                if (targetAppointment != null
                    && (IsOnLeaveFunc(targetAppointment)
                    || IsOnClientWorkFunc(targetAppointment)))
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
            var mainAppointment = GetMainAppointmentWithinDate(appointmentsWithinDate, date);
            return mainAppointment != null
                && IsOnInternalWorkFunc(mainAppointment);
        }

        public static bool IsOnLeave(GetEmployeeModel employee, DateTime date)
        {
            return IsOnLeave(employee.NormalizedAppointments, date);
        }

        public static bool IsOnLeave(IEnumerable<GetAppointmentModel> appointments, DateTime date)
        {
            var appointmentsWithinDate = GetEnumerableAppointmentsByDate(appointments, date);
            var mainAppointment = GetMainAppointmentWithinDate(appointmentsWithinDate, date);
            return mainAppointment != null
                && IsOnLeaveFunc(mainAppointment);
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

        public static IEnumerable<GetAppointmentModel> GetEnumerableAppointmentsByDate(IEnumerable<GetAppointmentModel> appointments, DateTime date, bool requieDynamicsTracked = true)
        {
            if (appointments == null || !appointments.Any())
            {
                return Enumerable.Empty<GetAppointmentModel>();
            }

            return appointments
                .Where(appointment =>
                    (requieDynamicsTracked
                    && !string.IsNullOrWhiteSpace(appointment.Regarding)
                    || !requieDynamicsTracked)
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

            static string GetDayString(int days)
            {
                return $"{days} {(days == 1 ? "day" : "days")}";
            }

            var timeOffset = endDate - startDate;

            if (timeOffset.TotalDays <= 2)
            {
                return String.Empty;
            }

            var totalDays = (int)timeOffset.TotalDays;

            if (totalDays < 7)
            {
                return $"{GetDayString(totalDays)} from now";
            }
            else if (totalDays < 30)
            {
                var weeks = totalDays / 7;
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
                var monthes = totalDays / 30;
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

        private static GetAppointmentModel GetMainAppointmentWithinDate(IEnumerable<GetAppointmentModel> appointments, DateTime date)
        {
            GetAppointmentModel mainAppointment = null;
            var mainOverlaps = TimeSpan.Zero;
            if (appointments == null || !appointments.Any())
            {
                return mainAppointment;
            }

            foreach (var appointment in appointments)
            {
                if (TryGetOverlapsTimeSpan(appointment, date, out var overlaps) && overlaps > mainOverlaps)
                {
                    mainAppointment = appointment;
                    mainOverlaps = overlaps;
                }
            }

            return mainAppointment;
        }

        private static bool TryGetOverlapsTimeSpan(GetAppointmentModel appointment, DateTime date, out TimeSpan overlaps)
        {
            overlaps = TimeSpan.Zero;
            if (appointment == null)
            {
                return false;
            }

            var currentDate = date.Date;
            var nextDate = date.Date.AddDays(1);

            if (appointment.End <= currentDate || appointment.Start >= nextDate)
            {
                return false;
            }

            if (appointment.Start.DateTime <= currentDate)
            {
                overlaps = appointment.End <= nextDate ? appointment.End - currentDate : nextDate - currentDate;
            }
            else
            {
                overlaps = appointment.End <= nextDate ? appointment.End - appointment.Start : nextDate - appointment.Start;
            }

            return true;
        }
    }
}
