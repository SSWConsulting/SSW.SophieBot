using SSW.SophieBot.Employees;
using SSW.SophieBot.Sync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Test
{
    public class TestEmployeeOdataService : IPagedSyncService<CrmEmployee>
    {
        private readonly EmployeeSyncMockHelper _employeeSyncMock;

        public TestEmployeeOdataService(EmployeeSyncMockHelper employeeSyncMock)
        {
            _employeeSyncMock = employeeSyncMock;
        }

        public Task<IEnumerable<CrmEmployee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_employeeSyncMock.CrmEmployees.AsEnumerable());
        }

        public async IAsyncEnumerable<IEnumerable<CrmEmployee>> GetAsyncPage(CancellationToken cancellationToken = default)
        {
            yield return _employeeSyncMock.CrmEmployees.Take(3);
            yield return _employeeSyncMock.CrmEmployees.Skip(3);
        }
    }
}
