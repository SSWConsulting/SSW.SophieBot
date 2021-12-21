using SSW.SophieBot.Employees;
using SSW.SophieBot.Persistence;
using System;
using System.Collections.Generic;

namespace SSW.SophieBot
{
    public class TestData
    {
        public List<CrmEmployee> CrmEmployees { get; set; } = new()
        {
            new CrmEmployee
            {
                Systemuserid = "a",
                Organizationid = "ssw",
                Firstname = "Jim",
                Lastname = "Northwind",
                Fullname = "Jim Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "b",
                Organizationid = "ssw",
                Firstname = "Jack",
                Lastname = "Northwind",
                Fullname = "Jack Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "d",
                Organizationid = "ssw",
                Firstname = "John",
                Lastname = "Northwind",
                Fullname = "John Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "e",
                Organizationid = "ssw",
                Firstname = "Hans",
                Lastname = "Northwind",
                Fullname = "Hans Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "f",
                Organizationid = "ssw",
                Firstname = "Alex",
                Lastname = "Northwind",
                Fullname = "Alex Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "g",
                Organizationid = "ssw",
                Firstname = "Taya",
                Lastname = "Northwind",
                Fullname = "Taya Northwind",
                Modifiedon = DateTime.MinValue
            },
            new CrmEmployee
            {
                Systemuserid = "h",
                Organizationid = "ssw",
                Firstname = "Nick",
                Lastname = "Northwind",
                Fullname = "Nick Northwind",
                Modifiedon = DateTime.MinValue
            }
        };

        public List<SyncSnapshot> Snapshots { get; set; } = new();
    }
}
