using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Extensions.Logging.Abstractions;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Entities;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.Sync;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot
{
    public class TestData
    {
        public ClosedListEntityExtractor PersonNames { get; } = new(
            Guid.NewGuid(),
            string.Empty,
            name: "personNames");

        public List<ClosedListEntityExtractor> ClosedListEntities => new()
        {
            PersonNames
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
            }, SyncMode.Create, BatchMode.BatchContent, DateTime.MinValue, string.Empty),
            new MqMessage<Employee>(new Employee
            {
                UserId = Guid.NewGuid().ToString(),
                FirstName = "Jack",
                LastName = "Northwind",
                FullName = "Jack Northwind"
            }, SyncMode.Update, BatchMode.BatchContent, DateTime.MinValue, string.Empty),
            new MqMessage<Employee>(new Employee
            {
                UserId = Guid.NewGuid().ToString(),
                FirstName = "John",
                LastName = "Northwind",
                FullName = "John Northwind"
            }, SyncMode.Delete, BatchMode.BatchContent, DateTime.MinValue, string.Empty)
        };

        public TestData()
        {
            var testPersonNamesClEntity = new TestPersonNamesClEntity(
                null,
                new TestPeopleClient(),
                NullLogger<PersonNames>.Instance);

            PersonNames.SubLists = MqEmployees
                .Select(mqEmployee =>
                {
                    var response = testPersonNamesClEntity.CreateWordList(mqEmployee.Message);
                    if (mqEmployee.SyncMode == SyncMode.Update)
                    {
                        response.List = response.List.Select(item => $"{item}_old").ToList();
                    }
                    return new SubClosedListResponse(response.CanonicalForm, response.List);
                })
                .Append(new SubClosedListResponse("Alex", new List<string>
                {
                    "Alexs",
                    "Alex Northwind",
                    "Alex Northwinds"
                }))
                .ToList();
        }
    }
}
