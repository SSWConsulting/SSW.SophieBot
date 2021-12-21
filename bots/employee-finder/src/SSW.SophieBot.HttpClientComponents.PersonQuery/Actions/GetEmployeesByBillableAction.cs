using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSW.SophieBot.Components;
using SSW.SophieBot.Components.Actions;
using SSW.SophieBot.HttpClientAction.Models;
using SSW.SophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetEmployeesByBillableAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetEmployeesByBillableAction";

        public GetEmployeesByBillableAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("isProject")]
        public BoolExpression IsProject { get; set; }

        [JsonProperty("project")]
        public StringExpression Project { get; set; }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("workingEmployeesProperty")]
        public StringExpression WorkingEmployeesProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var isProject = dc.GetValue(IsProject);
            var queriedProject = dc.GetValue(Project);

            var employees = dc.GetValue(Employees);
            if (employees == null || !employees.Any())
            {
                throw new ArgumentNullException(nameof(Employees));
            }

            var billableEmployees = EmployeesHelper.GetBillableEmployees(employees, queriedProject, isProject, out var projectName);

            var workingEmployees = new EmployeesByProjectListModel
            {
                Project = projectName,
                Employees = billableEmployees
            };

            dc.State.SetValue(dc.GetValue(WorkingEmployeesProperty), workingEmployees);

            return await dc.EndDialogAsync(result: workingEmployees, cancellationToken: cancellationToken);
        }
    }
}
