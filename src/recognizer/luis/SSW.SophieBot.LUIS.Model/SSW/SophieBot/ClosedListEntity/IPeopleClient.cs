using SSW.SophieBot.Employees;
using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot.ClosedListEntity
{
    public interface IPeopleClient
    {
        IAsyncEnumerable<IEnumerable<Employee>> GetAsyncPagedEmployees(CancellationToken cancellationToken = default);
    }
}
