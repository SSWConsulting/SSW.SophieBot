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
    public class GetGroupedEmployeesAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetGroupedEmployeesAction";

        public GetGroupedEmployeesAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("groupOptions")]
        public EmployeesGroupOptionsExpression GroupOptions { get; set; }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("groupedEmployeesProperty")]
        public StringExpression GroupedEmployeesProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            GroupOptions ??= new EmployeesGroupOptionsExpression();
            var groupedEmployees = new GroupedEmployeesModel(dc.GetValue(GroupOptions.CountPerSet));
            var employees = dc.GetValue(Employees);

            if (employees == null || !employees.Any())
            {
                dc.State.SetValue(dc.GetValue(GroupedEmployeesProperty), groupedEmployees);
                return await dc.EndDialogAsync(result: groupedEmployees, cancellationToken: cancellationToken);
            }

            var showAll = dc.GetValue(GroupOptions.ShowAll);
            if (showAll)
            {
                groupedEmployees.AddItem(new GroupedEmployeesItem
                {
                    Title = $"Show All [{employees.Count}]",
                    Key = string.Empty,
                    Count = employees.Count
                });
            }

            var groupKey = dc.GetValue(GroupOptions.GroupKey);
            if (!Enum.TryParse<EmployeesGroupKey>(groupKey, true, out var groupKeyEnum))
            {
                groupKeyEnum = EmployeesGroupKey.None;
            }

            var maxGroupCount = dc.GetValue(GroupOptions.MaxGroupCount) - (showAll ? 1 : 0);

            switch (groupKeyEnum)
            {
                case EmployeesGroupKey.Skill:
                    var skills = employees
                        .SelectMany(e => e.Skills.Select(s => s.Technology))
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()
                        .ToList();

                    skills.ToDictionary(s => s, s => employees.Count(e => e.HasSkill(s)))
                        .OrderByDescending(p => p.Value)
                        .Take(maxGroupCount)
                        .ToList()
                        .ForEach(s => groupedEmployees.AddItem(new GroupedEmployeesItem
                        {
                            Title = $"{s.Key} [{s.Value}]",
                            Key = s.Key,
                            Count = s.Value
                        }));
                    break;
                case EmployeesGroupKey.Location:
                    var locations = employees
                        .Select(e => e.DefaultSite?.Name)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()
                        .ToList();

                    locations.ToDictionary(l => l, l => employees.Count(e => e.DefaultSite?.Name == l))
                        .OrderByDescending(p => p.Value)
                        .Take(maxGroupCount)
                        .ToList()
                        .ForEach(l => groupedEmployees.AddItem(new GroupedEmployeesItem
                        {
                            Title = $"{l.Key} [{l.Value}]",
                            Key = l.Key,
                            Count = l.Value
                        }));
                    break;
                case EmployeesGroupKey.None:
                default:
                    break;
            }

            dc.State.SetValue(dc.GetValue(GroupedEmployeesProperty), groupedEmployees);
            return await dc.EndDialogAsync(result: groupedEmployees, cancellationToken: cancellationToken);
        }
    }
}
