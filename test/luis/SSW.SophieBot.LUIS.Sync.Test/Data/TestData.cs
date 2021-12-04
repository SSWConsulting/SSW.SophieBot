using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.Sync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestData
    {
        public ClosedListEntityExtractor SswPeopleNames { get; } = new(
            Guid.NewGuid(),
            string.Empty,
            name: "sswPeopleNames");

        public List<ClosedListEntityExtractor> ClosedListEntities => new()
        {
            SswPeopleNames
        };

        public List<MqMessage<Employee>> MqEmployees { get; set; } = new()
        {
            new MqMessage<Employee>(new Employee
            {
                UserId = Guid.NewGuid().ToString(),
                FirstName = "Jim",
                LastName = "Northwind",
                FullName = "Jim Northwind",
                NickName = "Jimmy"
            }, SyncMode.Create, DateTime.MinValue, string.Empty),
            new MqMessage<Employee>(new Employee
            {
                UserId = Guid.NewGuid().ToString(),
                FirstName = "Jack",
                LastName = "Northwind",
                FullName = "Jack Northwind"
            }, SyncMode.Update, DateTime.MinValue, string.Empty),
            new MqMessage<Employee>(new Employee
            {
                UserId = Guid.NewGuid().ToString(),
                FirstName = "John",
                LastName = "Northwind",
                FullName = "John Northwind"
            }, SyncMode.Delete, DateTime.MinValue, string.Empty)
        };

        public TestData()
        {
            var testSswPeopleNamesClEntity = new TestSswPeopleNamesClEntity();
            SswPeopleNames.SubLists = MqEmployees
                .Select(mqEmployee =>
                {
                    var response = testSswPeopleNamesClEntity.CreateSubClosedListResponse(mqEmployee.Message);
                    if (mqEmployee.SyncMode == SyncMode.Update)
                    {
                        response.List = response.List.Select(item => $"{item}_old").ToList();
                    }
                    return new SubClosedListResponse(response.CanonicalForm, response.List);
                })
                .ToList();
        }
    }
}
