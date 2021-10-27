using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.Components.Actions;
using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.PersonQuery.Models;
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

            var resultProjectsDic = new Dictionary<string, int>();

            void AddResultProject(string projectName)
            {
                if (!string.IsNullOrWhiteSpace(projectName))
                {
                    if (resultProjectsDic.TryGetValue(projectName, out var employeesCount))
                    {
                        resultProjectsDic[projectName]++;
                    }
                    else
                    {
                        resultProjectsDic[projectName] = 1;
                    }
                }
            }

            employees.ForEach(employee =>
            {
                var projectNames = Enumerable.Empty<string>();

                if (string.IsNullOrWhiteSpace(queriedProject))
                {
                    projectNames = employee.Projects.Select(project => project?.ProjectName).Distinct();
                }
                else
                {
                    projectNames = employee.Projects.Where(project =>
                        EmployeesHelper.IsProjectNameMatch(queriedProject, project.ProjectName))
                    .Select(project => project.ProjectName)
                    .Distinct();
                }

                foreach (var projectName in projectNames)
                {
                    AddResultProject(projectName);
                }
            });

            var resultProjects = resultProjectsDic
                .Select(pair => new ProjectWithEmployeesCountModel(pair.Key, pair.Value))
                .ToList();

            dc.State.SetValue(dc.GetValue(ProjectsResultProperty), resultProjects);

            return await dc.EndDialogAsync(result: resultProjects, cancellationToken: cancellationToken);
        }
    }
}
