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

        [JsonProperty("isProject")]
        public BoolExpression IsProject { get; set; }

        [JsonProperty("queriedProject")]
        public StringExpression QueriedProject { get; set; }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("projectsResultProperty")]
        public StringExpression ProjectsResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var isProject = dc.GetValue(IsProject);
            var queriedProject = dc.GetValue(QueriedProject);
            var employees = dc.GetValue(Employees);
            if (employees == null || !employees.Any())
            {
                throw new ArgumentNullException(nameof(Employees));
            }

            var resultProjectsDic = new Dictionary<KeyValuePair<string, string>, int>();

            void AddResultProject(KeyValuePair<string, string> project)
            {
                if (!string.IsNullOrWhiteSpace(project.Value))
                {
                    var existedKey = resultProjectsDic.Keys.FirstOrDefault(key => EmployeesHelper.IsProjectNameMatch(key.Value, project.Value));
                    if (!string.IsNullOrWhiteSpace(existedKey.Value))
                    {
                        resultProjectsDic[existedKey]++;
                    }
                    else
                    {
                        resultProjectsDic[project] = 1;
                    }
                }
            }

            employees.ForEach(employee =>
            {
                var projects = new Dictionary<string, string>();

                if (string.IsNullOrWhiteSpace(queriedProject))
                {
                    employee.Projects.ForEach(project =>
                    {
                        if (!string.IsNullOrEmpty(project?.CrmProjectId) && !projects.ContainsKey(project.CrmProjectId))
                        {
                            projects[isProject ? project.CrmProjectId : project.CrmClientId] = isProject ? project.ProjectName : project.CustomerName;
                        }
                    });
                }
                else
                {
                    employee.Projects.ForEach(project =>
                    {
                        if (!string.IsNullOrEmpty(project?.CrmProjectId)
                            && !projects.ContainsKey(project.CrmProjectId)
                            && EmployeesHelper.IsProjectNameMatch(queriedProject, isProject ? project.ProjectName : project.CustomerName))
                        {
                            projects[isProject ? project.CrmProjectId : project.CrmClientId] = isProject ? project.ProjectName : project.CustomerName;
                        }
                    });
                }

                foreach (var project in projects)
                {
                    AddResultProject(project);
                }
            });

            var resultProjects = resultProjectsDic
                .Select(pair => new ProjectWithEmployeesCountModel(
                    pair.Key.Key, 
                    pair.Key.Value, 
                    pair.Value))
                .ToList();

            dc.State.SetValue(dc.GetValue(ProjectsResultProperty), resultProjects);

            return await dc.EndDialogAsync(result: resultProjects, cancellationToken: cancellationToken);
        }
    }
}
