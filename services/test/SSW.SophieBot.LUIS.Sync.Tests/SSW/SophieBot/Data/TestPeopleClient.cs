using SSW.SophieBot.Employees;
using SSW.SophieBot.Projects;
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

        public IAsyncEnumerable<IEnumerable<Project>> GetPagedCrmProjectsAsync(CancellationToken cancellationToken = default)
        {
            return AsyncEnumerable.Empty<IEnumerable<Project>>();
        }
    }
}
