using SSW.SophieBot.Employees;
using SSW.SophieBot.Sync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public class TestEmployeeOdataService : IPagedSyncService<CrmEmployee>
    {
        private readonly TestData _testData;

        public TestEmployeeOdataService(TestData testData)
        {
            _testData = testData;
        }

        public virtual Task<IEnumerable<CrmEmployee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_testData.CrmEmployees.AsEnumerable());
        }

        public virtual async IAsyncEnumerable<IEnumerable<CrmEmployee>> GetAsyncPage(CancellationToken cancellationToken = default)
        {
            yield return _testData.CrmEmployees.Take(3);
            yield return _testData.CrmEmployees.Skip(3);
        }
    }
}
