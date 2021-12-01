using Microsoft.Azure.Cosmos;
using Moq;
using SSW.SophieBot.DataSync.Crm.HttpClients;
using SSW.SophieBot.DataSync.Domain;
using SSW.SophieBot.DataSync.Domain.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Test
{
    public static class Mock
    {
        private static List<CrmEmployee> _crmEmployees = new()
        {
            new CrmEmployee
            {
                Systemuserid = "a",
                Organizationid = "ssw",
                Firstname = "Jim",
                Lastname = "Northwind",
                Fullname = "Jim Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "b",
                Organizationid = "ssw",
                Firstname = "Jack",
                Lastname = "Northwind",
                Fullname = "Jack Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "d",
                Organizationid = "ssw",
                Firstname = "John",
                Lastname = "Northwind",
                Fullname = "John Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "e",
                Organizationid = "ssw",
                Firstname = "Hans",
                Lastname = "Northwind",
                Fullname = "Hans Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "f",
                Organizationid = "ssw",
                Firstname = "Alex",
                Lastname = "Northwind",
                Fullname = "Alex Northwind"
            },
            new CrmEmployee
            {
                Systemuserid = "c",
                Organizationid = "ssw",
                Firstname = "Taya",
                Lastname = "Northwind",
                Fullname = "Taya Northwind"
            }
        };

        public static CrmClient MockCrmClient()
        {
            var mock = new Mock<CrmClient>();
            mock.Setup(client => client.GetPagedEmployeesAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()).Result)
                .Returns((string nextLink, CancellationToken _) =>
                {
                    if(string.IsNullOrEmpty(nextLink))
                    {
                        return new OdataPagedResponse<CrmEmployee>
                        {
                            Value = _crmEmployees.Take(3).ToList(),
                            OdataNextLink = "next page"
                        };
                    }
                    else
                    {
                        return new OdataPagedResponse<CrmEmployee>
                        {
                            Value = _crmEmployees.Skip(3).ToList()
                        };
                    }
                });

            return mock.Object;
        }
    }
}
