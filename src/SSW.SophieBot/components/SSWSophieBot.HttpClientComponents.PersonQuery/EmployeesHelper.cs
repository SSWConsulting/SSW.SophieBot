﻿using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SSWSophieBot.HttpClientComponents.PersonQuery
{
    public static class EmployeesHelper
    {
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

            return employees.Select(e => new EmployeeBillableItemModel
            {
                AvatarUrl = e.AvatarUrl,
                DisplayName = $"{e.FirstName} {e.LastName}",
                BilledDays = GetBilledDays(e, project),
                LastSeen = GetLastSeen(e)
            })
            .OrderByDescending(i => i.BilledDays)
            .ToList();
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
                return $"{hours} {(hours == 1 ? "hr" : "hrs")} ago";
            }
            else if (timeOffset.TotalDays < 30)
            {
                var days = GetInteger(timeOffset.TotalDays);
                return $"{days} {(days == 1 ? "d" : "ds")} ago";
            }
            else
            {
                var months = GetInteger(timeOffset.TotalDays / 30);
                return $"{months} {(months == 1 ? "month" : "months")} ago";
            }
        }

        public static NextClientModel GetNextClient(GetEmployeeModel employee)
        {
            var now = DateTime.Now.ToUniversalTime();
            var appointments = employee.Appointments
                .Where(appointment => appointment.Start.UtcDateTime.Ticks > now.Ticks && appointment.Regarding?.ToLower() != "ssw")
                .ToList();
            var appointment = appointments.LastOrDefault();

            if (appointments.Count == 0 || appointment.Regarding == null)
            {
                return null;
            }

            return new NextClientModel
            {
                FreeDays = GetBusinessDays(DateTime.Now.ToUniversalTime(), appointment.Start.UtcDateTime),
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

                return (int)Math.Round(result);
            }
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
    }
}
