using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Entities;
using System.Collections.Generic;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestSswPeopleNamesClEntity : SswPeopleNames
    {
        public TestSswPeopleNamesClEntity()
        {

        }

        public override WordListObject CreateWordList(Employee employee)
        {
            return base.CreateWordList(employee);
        }

        public override string GetCanonicalForm(Employee employee)
        {
            return base.GetCanonicalForm(employee);
        }

        public override SubClosedList CreateSubClosedListResponse(Employee employee)
        {
            return base.CreateSubClosedListResponse(employee);
        }

        public override IList<string> GetSubList(Employee employee)
        {
            return base.GetSubList(employee);
        }
    }
}
