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
                return originalProjectName.Contains(sourceProjectName, StringComparison.OrdinalIgnoreCase);
            }

            var queriedProject = dc.GetValue(Project);
            var employees = dc.GetValue(Employees);
            var project = employees.FirstOrDefault()?.Projects?.FirstOrDefault(p => IsProjectNameEqual(queriedProject, p.ProjectName))?.ProjectName;

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
                    BilledDays = (int)Math.Ceiling(
                        (decimal)e.Projects.FirstOrDefault(p => IsProjectNameEqual(project, p.ProjectName))?.BillableHours / 24
                    ),
                    LastSeen = "not implemented"
                }).ToList()
            };

            dc.State.SetValue(dc.GetValue(WorkingEmployeesProperty), workingEmployees);

            return await dc.EndDialogAsync(result: workingEmployees, cancellationToken: cancellationToken);
        }
    }
}
