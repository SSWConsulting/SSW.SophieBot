using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using SSW.SophieBot.Employees;
using System;
using System.Collections.Generic;

namespace SSW.SophieBot.Entities
{
    public class SswPeopleNames : ClosedListEntityBase
    {
        private const string _entityName = "sswPeopleNames";

        public override string EntityName => _entityName;

        public virtual WordListObject CreateWordList(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            return new WordListObject(GetCanonicalForm(employee), GetSubList(employee));
        }

        public virtual SubClosedList CreateSubClosedListResponse(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            return new SubClosedList(GetCanonicalForm(employee), GetSubList(employee));
        }

        public virtual string GetCanonicalForm(Employee employee)
        {
            return employee.UserId;
        }

        public virtual IList<string> GetSubList(Employee employee)
        {
            var nameList = new List<string>
            {
                employee.FirstName,
                employee.LastName,
                employee.FullName,
                $"{employee.FullName}s",
                $"{employee.FirstName}s"
            };

            if (!employee.NickName.IsNullOrWhiteSpace())
            {
                nameList.Add(employee.NickName);
                nameList.Add($"{employee.NickName}s");
            }

            return nameList;
        }
    }
}
