using SSW.SophieBot.ClosedListEntity;
using SSW.SophieBot.Employees;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestPeopleClient : IPeopleClient
    {
        public IAsyncEnumerable<IEnumerable<Employee>> GetAsyncPagedEmployees(CancellationToken cancellationToken = default)
        {
            return AsyncEnumerable.Empty<IEnumerable<Employee>>();
        }
    }
}
