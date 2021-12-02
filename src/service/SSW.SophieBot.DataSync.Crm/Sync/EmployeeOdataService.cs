using SSW.SophieBot.DataSync.Crm.HttpClients;
using SSW.SophieBot.DataSync.Domain.Employees;
using SSW.SophieBot.DataSync.Domain.Sync;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Sync
{
    public class EmployeeOdataService : CrmOdataService, IPagedOdataSyncService<CrmEmployee>
    {
        private const string InitialNextLink = "_next";

        private string _internalNextLink = InitialNextLink;

        public string NextLink => _internalNextLink == InitialNextLink ? null : _internalNextLink;

        public bool HasMoreResults => !_internalNextLink.IsNullOrWhiteSpace();

        public EmployeeOdataService(CrmClient crmClient) : base(crmClient)
        {

        }

        public async Task<OdataPagedResponse<CrmEmployee>> GetNextAsync(CancellationToken cancellationToken = default)
        {
            var odataResponse = await CrmClient.GetPagedEmployeesAsync(NextLink, cancellationToken);
            _internalNextLink = odataResponse.OdataNextLink;
            return odataResponse;
        }
    }
}
