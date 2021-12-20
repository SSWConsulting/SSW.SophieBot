using SSW.SophieBot.Employees;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SSW.SophieBot
{
    public class TestPeopleClient : IPeopleApiClient
    {
        public IAsyncEnumerable<IEnumerable<Employee>> GetPagedEmployeesAsync(CancellationToken cancellationToken = default)
        {
            return AsyncEnumerable.Empty<IEnumerable<Employee>>();
        }
    }
}
