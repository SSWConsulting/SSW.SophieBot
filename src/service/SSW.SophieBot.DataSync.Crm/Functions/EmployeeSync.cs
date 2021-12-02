using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Functions
{
    public class EmployeeSync
    {
        public const string UpsertSqlQueryText = "SELECT * FROM c WHERE c.organizationId=@organizationId AND c.id IN @ids";
        public const string DeleteSqlQueryText = "SELECT * FROM c WHERE c.organizationId=@organizationId AND c.syncVersion != @syncVersion";

        private readonly IPagedOdataSyncService<CrmEmployee> _employeeOdataService;
        private readonly ITransactionalBulkRepository<SyncSnapshot, PatchOperation> _syncSnapshotRepository;
        private readonly IBatchMessageService<MqMessage<Employee>, string> _serviceBusService;
        private readonly ISyncVersionGenerator _syncVersionGenerator;
        private readonly SyncOptions _syncOptions;
        private readonly ILogger<EmployeeSync> _logger;

        public EmployeeSync(
            IPagedOdataSyncService<CrmEmployee> employeeOdataService,
            ITransactionalBulkRepository<SyncSnapshot, PatchOperation> syncSnapshotRepository,
            IBatchMessageService<MqMessage<Employee>, string> serviceBusService,
            ISyncVersionGenerator syncVersionGenerator,
            IOptions<SyncOptions> syncOptions,
            ILogger<EmployeeSync> logger)
        {
            _employeeOdataService = employeeOdataService;
            _syncSnapshotRepository = syncSnapshotRepository;
            _serviceBusService = serviceBusService;
            _syncVersionGenerator = syncVersionGenerator;
            _syncOptions = syncOptions.Value;
            _logger = logger;
        }

        [FunctionName(nameof(SyncEmployeeProfileAsync))]
        public async Task SyncEmployeeProfileAsync(
            [TimerTrigger("%EmployeeSync:Timer%")] TimerInfo timer,
            CancellationToken cancellationToken)
        {
            if (timer.IsPastDue)
            {
                _logger.LogWarning("Timer function is running late!");
                return;
            }

            var isVersionUpdated = true;
            var syncVersion = await _syncVersionGenerator.GenerateAsync(cancellationToken);

            do
            {
                var crmEmployeesOdata = await _employeeOdataService.GetNextAsync(cancellationToken);
                if (crmEmployeesOdata?.Value?.IsNullOrEmpty() ?? true)
                {
                    isVersionUpdated = false;
                    break;
                }

                // calculate and perform upsert on employees, and send out messages
                var employeesSyncData = await PerformUpsertionAsync(crmEmployeesOdata, syncVersion, cancellationToken);
                isVersionUpdated = isVersionUpdated && employeesSyncData.IsVersionUpdated;
            }
            while (_employeeOdataService.HasMoreResults);

            // if sync version was successfully updated, query and perform delete on out-dated employees, and send out messages
            if (isVersionUpdated)
            {
                await PerformDeletionAsync(syncVersion, cancellationToken);
            }
        }

        private async Task<SyncListData<MqMessage<Employee>>> PerformUpsertionAsync(
            OdataPagedResponse<CrmEmployee> crmEmployeesOdata,
            string syncVersion,
            CancellationToken cancellationToken)
        {
            var employeesSyncData = await GetUpsertEmployeesAsync(crmEmployeesOdata, syncVersion, cancellationToken);
            var upsertedEmployees = await UpdateSnapshotAsync(employeesSyncData.Data, cancellationToken);
            await SendMessagesAsync(upsertedEmployees, cancellationToken);

            return employeesSyncData;
        }

        private async Task PerformDeletionAsync(string syncVersion, CancellationToken cancellationToken)
        {
            do
            {
                var queryParameters = new List<(string, object)>
                {
                    ("@organizationId", _syncOptions.OrganizationId),
                    ("@syncVersion", syncVersion)
                };
                var deleteSnapshots = await _syncSnapshotRepository.GetNextAsync(DeleteSqlQueryText, queryParameters, cancellationToken);
                var deleteEmployees = deleteSnapshots.Select(snapshot => new MqMessage<Employee>(
                    new Employee(snapshot.Id, snapshot.OrganizationId),
                    SyncMode.Delete,
                    snapshot.Modifiedon,
                    snapshot.SyncVersion)).ToList();

                await UpdateSnapshotAsync(deleteEmployees, cancellationToken); // ignore snapshot deletion failure
                await SendMessagesAsync(deleteEmployees, cancellationToken);
            }
            while (_syncSnapshotRepository.HasMoreResults);
        }

        private async Task<SyncListData<MqMessage<Employee>>> GetUpsertEmployeesAsync(
            OdataPagedResponse<CrmEmployee> crmEmployeesOdata,
            string syncVersion,
            CancellationToken cancellationToken)
        {
            if (!crmEmployeesOdata.Value.Any())
            {
                return new SyncListData<MqMessage<Employee>>();
            }

            var crmEmployeeIds = crmEmployeesOdata.Value.Select(crmEmployee => crmEmployee.Systemuserid).ToArray();
            var queryParameters = new List<(string, object)>
            {
                ("@organizationId", _syncOptions.OrganizationId),
                ("@ids", crmEmployeeIds)
            };
            var crmEmployeeSnapshots = await _syncSnapshotRepository.GetAllAsync(UpsertSqlQueryText, queryParameters, cancellationToken);
            var syncedSnapshotIds = new List<string>();

            var upsertEmployees = new List<MqMessage<Employee>>();
            foreach (var crmEmployee in crmEmployeesOdata.Value)
            {
                var previousSnapshot = crmEmployeeSnapshots.FirstOrDefault(snapshot => snapshot.Id == crmEmployee.Systemuserid);

                if (previousSnapshot == null || previousSnapshot.Modifiedon < crmEmployee.Modifiedon)
                {
                    upsertEmployees.Add(new MqMessage<Employee>(
                        Employee.FromCrmEmployee(crmEmployee),
                        previousSnapshot == null ? SyncMode.Create : SyncMode.Update,
                        crmEmployee.Modifiedon,
                        syncVersion));
                }
                else
                {
                    syncedSnapshotIds.Add(previousSnapshot.Id);
                }
            }

            // update sync version
            var transactionBatch = await _syncSnapshotRepository.BeginTransactionAsync(cancellationToken);
            var syncVersionOperation = PatchOperation.Set("/syncVersion", syncVersion);

            foreach (var syncedSnapshotId in syncedSnapshotIds)
            {
                transactionBatch.Patch(syncedSnapshotId, syncVersionOperation);
            }

            var transactionSaved = await transactionBatch.SaveChangesAsync();
            return new SyncListData<MqMessage<Employee>>(upsertEmployees, transactionSaved);
        }

        private async Task<List<MqMessage<Employee>>> UpdateSnapshotAsync(
            IEnumerable<MqMessage<Employee>> employees,
            CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            var successfulEmployees = new List<MqMessage<Employee>>();

            Action<Task> GetContinueAction(MqMessage<Employee> employee)
            {
                return itemResponse =>
                {
                    if (!itemResponse.IsCompletedSuccessfully)
                    {
                        var innerExceptions = itemResponse.Exception.Flatten();
                        if (innerExceptions.InnerExceptions.FirstOrDefault(innerEx => innerEx is CosmosException) is CosmosException cosmosException)
                        {
                            _logger.LogError("Error occurred during Cosmos DB bulk operation: [{StatusCode}] {Message}",
                                cosmosException.StatusCode,
                                cosmosException.Message);
                        }
                        else
                        {
                            _logger.LogError("Error occurred during Cosmos DB bulk operation: {Exception}",
                                innerExceptions.InnerExceptions.FirstOrDefault());
                        }
                    }
                    else
                    {
                        successfulEmployees.Add(employee);
                    }
                };
            }

            _syncSnapshotRepository.BeginBulk();
            foreach (var employee in employees)
            {
                if (employee.SyncMode == SyncMode.Create || employee.SyncMode == SyncMode.Update)
                {
                    var snapshot = employee.Message.ToSnapshot(employee.ModifiedOn, employee.SyncVersion);
                    await _syncSnapshotRepository.BulkUpdateAsync(snapshot, GetContinueAction(employee), cancellationToken);
                }
                else if (employee.SyncMode == SyncMode.Delete)
                {
                    var snapshotId = employee.Message.UserId;
                    await _syncSnapshotRepository.BulkDeleteAsync(snapshotId, GetContinueAction(employee), cancellationToken);
                }
            }
            await _syncSnapshotRepository.ExecuteBulkAsync();

            return successfulEmployees;
        }

        private async Task SendMessagesAsync(IEnumerable<MqMessage<Employee>> messages, CancellationToken cancellationToken)
        {
            if (!messages.Any())
            {
                return;
            }

            await _serviceBusService.SendMessageAsync(messages, _syncOptions.EmployeeSync.TopicName, cancellationToken);
        }
    }
}
