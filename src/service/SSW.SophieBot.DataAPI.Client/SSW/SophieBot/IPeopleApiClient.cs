using SSW.SophieBot.Employees;
using System.Collections.Generic;

namespace SSW.SophieBot
{
    public interface IPeopleApiClient
    {
        IAsyncEnumerable<IEnumerable<Employee>> GetPagedEmployeesAsync();
    }
}
