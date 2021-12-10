using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using SSW.SophieBot.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.ClosedListEntity
{
    public class SswPeopleNames : ClosedListEntityBase
    {
        private const string _entityName = "sswPeopleNames";
        public override string EntityName => _entityName;

        private readonly IPeopleClient _peopleClient;

        public SswPeopleNames(IPeopleClient peopleClient)
        {
            _peopleClient = peopleClient;
        }

        public override async Task FillSubListsAsync(CancellationToken cancellationToken = default)
        {
            SubLists.Clear();

            var employeePages = _peopleClient.GetAsyncPagedEmployees(cancellationToken);

            await foreach (var employees in employeePages)
            {
                if (!employees.IsNullOrEmpty())
                {
                    var subLists = employees.Select(employee => CreateWordList(employee));
                    foreach (var subList in subLists)
                    {
                        SubLists.Add(subList);
                    }
                }
            }
        }

        public virtual WordListObject CreateWordList(Employee employee)
        {
            Check.NotNull(employee, nameof(employee));
            return new WordListObject(GetCanonicalForm(employee), GetSubList(employee));
        }

        public virtual SubClosedList CreateSubClosedListResponse(Employee employee)
        {
            Check.NotNull(employee, nameof(employee));
            return new SubClosedList(GetCanonicalForm(employee), GetSubList(employee));
        }

        public virtual string GetCanonicalForm(Employee employee)
        {
            return employee.UserId;
        }

        public virtual IList<string> GetSubList(Employee employee)
        {
            var fullName = employee.FullName.IsNullOrWhiteSpace() ? $"{employee.FirstName} {employee.LastName}" : employee.FullName;
            var nameList = new List<string>
            {
                employee.FirstName,
                employee.LastName,
                $"{employee.FirstName}s",
                fullName,
                $"{fullName}s"
            };

            if (!employee.NickName.IsNullOrWhiteSpace())
            {
                nameList.Add(employee.NickName);
                nameList.Add($"{employee.NickName}s");
            }

            return nameList.Distinct().ToList();
        }
    }
}
