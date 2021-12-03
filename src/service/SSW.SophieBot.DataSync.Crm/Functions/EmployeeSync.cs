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
        public const string UpsertSqlQueryText = "SELECT * FROM c WHERE c.organizationId=@organizationId AND ARRAY_CONTAINS(@ids, c.id)=true";
        public const string DeleteSqlQueryText = "SELECT * FROM c WHERE c.organizationId=@organizationId AND c.syncVersion != @syncVersion";

        private readonly IPagedSyncService<CrmEmployee> _employeeOdataService;
        private readonly ITransactionalBulkRepository<SyncSnapshot, PatchOperation> _syncSnapshotRepository;
        private readonly IBatchMessageService<MqMessage<Employee>, string> _serviceBusService;
        private readonly ISyncVersionGenerator _syncVersionGenerator;
        private readonly SyncOptions _syncOptions;
        private readonly ILogger<EmployeeSync> _logger;

        public EmployeeSync(
            IPagedSyncService<CrmEmployee> employeeOdataService,
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
            [TimerTrigger("%EmployeeSync:Timer%"
#if DEBUG
            , RunOnStartup = true
#endif
            )] TimerInfo timer,
            CancellationToken cancellationToken)
        {
            if (timer.IsPastDue)
            {
                _logger.LogWarning("Timer function is running late!");
                return;
            }

            var isVersionUpdated = true;
            var syncVersion = await _syncVersionGenerator.GenerateAsync(cancellationToken);

            await foreach (var crmEmployees in _employeeOdataService.GetAsyncPage(cancellationToken))
            {
                if (crmEmployees.IsNullOrEmpty())
                {
                    isVersionUpdated = false;
                    break;
                }

                // calculate and perform upsert on employees, and send out messages
                _logger.LogDebug("Retrieved employee odata: {Count}", crmEmployees.Count());
                var employeesSyncData = await PerformUpsertionAsync(crmEmployees, syncVersion, cancellationToken);
                isVersionUpdated = isVersionUpdated && employeesSyncData.IsVersionUpdated;
            }

            // if sync version was successfully updated, query and perform delete on out-dated employees, and send out messages
            if (isVersionUpdated)
            {
                await PerformDeletionAsync(syncVersion, cancellationToken);
            }
        }

        private async Task<SyncListData<MqMessage<Employee>>> PerformUpsertionAsync(
            IEnumerable<CrmEmployee> crmEmployeesOdata,
            string syncVersion,
            CancellationToken cancellationToken)
        {
            var employeesSyncData = await GetUpsertEmployeesAsync(crmEmployeesOdata, syncVersion, cancellationToken);
            var upsertedEmployees = await UpdateSnapshotAsync(employeesSyncData.Data, cancellationToken);
            await SendMessagesAsync(upsertedEmployees, cancellationToken);

            return employeesSyncData;
        }

        private async Task<SyncListData<MqMessage<Employee>>> GetUpsertEmployeesAsync(
            IEnumerable<CrmEmployee> crmEmployeesOdata,
            string syncVersion,
            CancellationToken cancellationToken)
        {
            if (!crmEmployeesOdata.Any())
            {
                return new SyncListData<MqMessage<Employee>>();
            }

            var crmEmployeeIds = crmEmployeesOdata.Select(crmEmployee => crmEmployee.Systemuserid).ToArray();
            var queryParameters = new List<(string, object)>
            {
                ("@organizationId", _syncOptions.OrganizationId),
                ("@ids", crmEmployeeIds)
            };
            var crmEmployeeSnapshots = await _syncSnapshotRepository.GetAllAsync(UpsertSqlQueryText, queryParameters, cancellationToken);
            var syncedSnapshotIds = new List<string>();

            var upsertEmployees = new List<MqMessage<Employee>>();
            foreach (var crmEmployee in crmEmployeesOdata)
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
            var transactionSaved = true;
            if (syncedSnapshotIds.Any())
            {
                var transactionBatch = await _syncSnapshotRepository.BeginTransactionAsync(cancellationToken);
                var syncVersionOperation = PatchOperation.Set("/syncVersion", syncVersion);

                foreach (var syncedSnapshotId in syncedSnapshotIds)
                {
                    transactionBatch.Patch(syncedSnapshotId, syncVersionOperation);
                }

                transactionSaved = await transactionBatch.SaveChangesAsync();
            }

            _logger.LogDebug("Employees to upsert: {Count}", upsertEmployees.Count);
            return new SyncListData<MqMessage<Employee>>(upsertEmployees, transactionSaved);
        }

        private async Task<List<MqMessage<Employee>>> UpdateSnapshotAsync(
            IEnumerable<MqMessage<Employee>> employees,
            CancellationToken cancellationToken)
        {
            var bulkOperations = await _syncSnapshotRepository.BeginBulkAsync();
            foreach (var employee in employees)
            {
                if (employee.SyncMode == SyncMode.Create || employee.SyncMode == SyncMode.Update)
                {
                    var snapshot = employee.Message.ToSnapshot(employee.ModifiedOn, employee.SyncVersion);
                    bulkOperations.BulkUpdate(snapshot, cancellationToken);
                }
                else if (employee.SyncMode == SyncMode.Delete)
                {
                    var snapshotId = employee.Message.UserId;
                    bulkOperations.BulkDelete(snapshotId, cancellationToken);
                }
            }

            var updatedSnapshots = await bulkOperations.ExecuteBulkAsync();
            var updatedEmployeeIds = updatedSnapshots.Select(snapshot => snapshot.Id);
            var updatedEmployees = employees.Where(employee => updatedEmployeeIds.Contains(employee.Message.UserId)).ToList();

            _logger.LogDebug("Snapshot updated: {Count}", updatedSnapshots.Count);
            return updatedEmployees;
        }

        private async Task PerformDeletionAsync(string syncVersion, CancellationToken cancellationToken)
        {
            var queryParameters = new List<(string, object)>
                {
                    ("@organizationId", _syncOptions.OrganizationId),
                    ("@syncVersion", syncVersion)
                };
            var snapshotPages = _syncSnapshotRepository.GetAsyncPages(DeleteSqlQueryText, queryParameters, cancellationToken);
            await foreach (var snapshots in snapshotPages)
            {
                if (snapshots.Any())
                {
                    var deleteEmployees = snapshots.Select(snapshot => new MqMessage<Employee>(
                        new Employee(snapshot.Id, snapshot.OrganizationId),
                        SyncMode.Delete,
                        snapshot.Modifiedon,
                        snapshot.SyncVersion)).ToList();

                    _logger.LogDebug("Snapshot to delete: {Count}", snapshots.Count());

                    await UpdateSnapshotAsync(deleteEmployees, cancellationToken); // ignore snapshot deletion failure
                    await SendMessagesAsync(deleteEmployees, cancellationToken);
                }
            }
        }

        private async Task SendMessagesAsync(IEnumerable<MqMessage<Employee>> messages, CancellationToken cancellationToken)
        {
            if (!messages.Any())
            {
                return;
            }

            _logger.LogDebug("Send out messages: {Count}", messages.Count());
            await _serviceBusService.SendMessageAsync(messages, _syncOptions.EmployeeSync.TopicName, cancellationToken);
        }
    }
}
