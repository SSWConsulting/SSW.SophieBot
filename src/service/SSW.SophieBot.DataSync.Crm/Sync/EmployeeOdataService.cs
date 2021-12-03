using SSW.SophieBot.DataSync.Crm.HttpClients;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Sync;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Sync
{
    public class EmployeeOdataService : CrmOdataService, IPagedSyncService<CrmEmployee>
    {
        public EmployeeOdataService(CrmClient crmClient) : base(crmClient)
        {

        }

        public async IAsyncEnumerable<IEnumerable<CrmEmployee>> GetAsyncPage([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var odataResponse = await CrmClient.GetPagedEmployeesAsync(cancellationToken: cancellationToken);
            yield return odataResponse.Value;

            while (odataResponse.HasMoreResults)
            {
                odataResponse = await CrmClient.GetPagedEmployeesAsync(odataResponse.OdataNextLink, cancellationToken);
                yield return odataResponse.Value;
            }
        }

        public async Task<IEnumerable<CrmEmployee>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var crmEmployees = new List<CrmEmployee>();

            await foreach (var page in GetAsyncPage(cancellationToken))
            {
                crmEmployees.AddRange(page);
            }

            return crmEmployees;
        }
    }
}
