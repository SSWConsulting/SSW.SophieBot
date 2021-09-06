using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.Components.Actions;
using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetEmployeesByProjectAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetEmployeesByProjectAction";

        public GetEmployeesByProjectAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("project")]
        public StringExpression Project { get; set; }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("workingEmployeesProperty")]
        public StringExpression WorkingEmployeesProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            static bool IsProjectNameEqual(string sourceProjectName, string originalProjectName)
            {
                if (string.IsNullOrWhiteSpace(originalProjectName) || string.IsNullOrWhiteSpace(sourceProjectName))
                {
                    return false;
                }

                var _rgx = new Regex("[^a-zA-Z0-9]", RegexOptions.Compiled);
                var prjName = _rgx.Replace(sourceProjectName, string.Empty);
                return _rgx.Replace(originalProjectName, string.Empty).IndexOf(prjName, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            var queriedProject = dc.GetValue(Project);

            if (string.IsNullOrWhiteSpace(queriedProject))
            {
                throw new ArgumentNullException(nameof(Project));
            }

            var employees = dc.GetValue(Employees);
            var project = employees?.FirstOrDefault()?.Projects?.FirstOrDefault(p => IsProjectNameEqual(queriedProject, p.ProjectName))?.ProjectName;

            int BilledDays(GetEmployeeModel model)
            {
                return (int)Math.Ceiling(
                    (decimal)model.Projects.FirstOrDefault(p => IsProjectNameEqual(project, p.ProjectName))?.BillableHours / 8
                );
            }

            static string LastSeen(GetEmployeeModel model)
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

            if (string.IsNullOrWhiteSpace(project))
            {
                throw new ArgumentNullException(nameof(Project));
            }

            if (employees == null || !employees.Any())
            {
                throw new ArgumentNullException(nameof(Employees));
            }

            var workingEmployees = new WorkingEmployeesModel
            {
                Project = project,
                Employees = employees.Select(e => new WorkingEmployeeItem
                {
                    AvatarUrl = e.AvatarUrl,
                    DisplayName = $"{e.FirstName} {e.LastName}",
                    BilledDays = BilledDays(e),
                    LastSeen = LastSeen(e)
                }).ToList()
            };

            dc.State.SetValue(dc.GetValue(WorkingEmployeesProperty), workingEmployees);

            return await dc.EndDialogAsync(result: workingEmployees, cancellationToken: cancellationToken);
        }
    }
}
