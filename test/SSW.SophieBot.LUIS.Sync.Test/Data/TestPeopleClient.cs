using SSW.SophieBot.Employees;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestPeopleClient : IPeopleApiClient
    {
        public IAsyncEnumerable<IEnumerable<Employee>> GetPagedEmployeesAsync()
        {
            return AsyncEnumerable.Empty<IEnumerable<Employee>>();
        }
    }
}
