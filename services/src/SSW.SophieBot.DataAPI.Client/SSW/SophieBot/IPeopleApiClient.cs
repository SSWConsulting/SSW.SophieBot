using SSW.SophieBot.Employees;
using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot
{
    public interface IPeopleApiClient
    {
        IAsyncEnumerable<IEnumerable<Employee>> GetPagedEmployeesAsync(CancellationToken cancellationToken = default);
    }
}
