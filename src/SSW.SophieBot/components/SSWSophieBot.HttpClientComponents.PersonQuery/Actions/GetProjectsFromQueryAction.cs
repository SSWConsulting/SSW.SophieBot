using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.Components.Actions;
using SSWSophieBot.HttpClientAction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetProjectsFromQueryAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetProjectsFromQueryAction";

        public GetProjectsFromQueryAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("queriedProject")]
        public StringExpression QueriedProject { get; set; }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("projectsResultProperty")]
        public StringExpression ProjectsResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var queriedProject = dc.GetValue(QueriedProject);
            var employees = dc.GetValue(Employees);
            if (employees == null || !employees.Any())
            {
                throw new ArgumentNullException(nameof(Employees));
            }

            var resultProjects = new List<string>();

            if (string.IsNullOrWhiteSpace(queriedProject))
            {
                resultProjects = employees
                    .SelectMany(employee => employee.Projects.Select(project => project.ProjectName))
                    .Where(projectName => !string.IsNullOrWhiteSpace(projectName))
                    .Distinct()
                    .ToList();
            }
            else
            {
                resultProjects = employees
                    .SelectMany(employee => employee.Projects.Select(project =>
                        {
                            if (string.IsNullOrWhiteSpace(project.ProjectName))
                            {
                                return string.Empty;
                            }

                            if (EmployeesHelper.IsProjectNameMatch(queriedProject, project.ProjectName))
                            {
                                return project.ProjectName;
                            }

                            return string.Empty;
                        })
                    )
                    .Where(projectName => !string.IsNullOrWhiteSpace(projectName))
                    .Distinct()
                    .ToList();
            }

            dc.State.SetValue(dc.GetValue(ProjectsResultProperty), resultProjects);

            return await dc.EndDialogAsync(result: resultProjects, cancellationToken: cancellationToken);
        }
    }
}
