using SSW.SophieBot.Employees;
using SSW.SophieBot.Projects;
using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot
{
    public interface IPeopleApiClient
    {
        IAsyncEnumerable<IEnumerable<Employee>> GetPagedEmployeesAsync(CancellationToken cancellationToken = default);

        IAsyncEnumerable<IEnumerable<Project>> GetPagedCrmProjectsAsync(CancellationToken cancellationToken = default);
    }
}
