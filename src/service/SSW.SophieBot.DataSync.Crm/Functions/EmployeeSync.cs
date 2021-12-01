using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.AzureFunction.System;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.DataSync.Crm.HttpClients;
using SSW.SophieBot.DataSync.Domain;
using SSW.SophieBot.DataSync.Domain.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Functions
{
    public class EmployeeSync
    {
        private readonly CrmClient _crmClient;
        private readonly CosmosClient _cosmosClient;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly SyncOptions _syncOptions;
        private readonly ILogger<EmployeeSync> _logger;

        public EmployeeSync(
            CrmClient crmClient,
            CosmosClient cosmosClient,
            ServiceBusClient serviceBusClient,
            IOptions<SyncOptions> syncOptions,
            ILogger<EmployeeSync> logger)
        {
            _crmClient = crmClient;
            _cosmosClient = cosmosClient;
            _serviceBusClient = serviceBusClient;
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

            var container = _cosmosClient.GetContainer(_syncOptions.EmployeeSync.DatabaseId, _syncOptions.EmployeeSync.ContainerId);
            var employeesSyncData = new SyncListData<MqMessage<Employee>>();
            await using var serviceBusSender = _serviceBusClient.CreateSender(_syncOptions.EmployeeSync.TopicName);

            var loop = true;
            var nextLink = string.Empty;

            while (loop)
            {
                var crmEmployeesOdata = await _crmClient.GetPagedEmployeesAsync(nextLink, cancellationToken);
                if (crmEmployeesOdata?.Value.IsNullOrEmpty() ?? true)
                {
                    break;
                }

                // calculate and perform upsert on employees, and send out messages
                employeesSyncData = await PerformUpsertAsync(crmEmployeesOdata, container, serviceBusSender, cancellationToken);

                nextLink = crmEmployeesOdata.OdataNextLink;
                loop = crmEmployeesOdata.HasNext();
            }

            // if sync version was successfully updated, query and perform delete on out-dated employees, and send out messages
            if (employeesSyncData.IsVersionUpdated)
            {
                await PerformDeletionAsync(employeesSyncData.SyncVersion, container, serviceBusSender, cancellationToken);
            }
        }

        private async Task<SyncListData<MqMessage<Employee>>> PerformUpsertAsync(
            OdataPagedResponse<CrmEmployee> crmEmployeesOdata,
            Container container,
            ServiceBusSender sender,
            CancellationToken cancellationToken)
        {
            var employeesSyncData = await GetUpsertEmployeesAsync(crmEmployeesOdata, container, cancellationToken);
            var upsertedEmployees = await UpdateSnapshotAsync(employeesSyncData.Data, container, cancellationToken);
            await SendMessagesAsync(upsertedEmployees, sender, cancellationToken);

            return employeesSyncData;
        }

        private async Task PerformDeletionAsync(
            string syncVersion,
            Container container,
            ServiceBusSender sender,
            CancellationToken cancellationToken)
        {
            string GetSqlQueryText()
            {
                return $"SELECT * FROM c WHERE c.organizationId={_syncOptions.OrganizationId} AND c.syncVersion != '{syncVersion}'";
            }

            var queryDefinition = new QueryDefinition(GetSqlQueryText());

            using var snapshotsIterator = container.GetItemQueryIterator<SyncSnapshot>(queryDefinition);

            while (snapshotsIterator.HasMoreResults)
            {
                var deleteEmployees = (await snapshotsIterator.ReadNextAsync(cancellationToken))
                    .Select(snapshot => new MqMessage<Employee>(
                        new Employee(snapshot.Id, snapshot.OrganizationId),
                        SyncMode.Delete,
                        snapshot.Modifiedon,
                        snapshot.SyncVersion));

                await UpdateSnapshotAsync(deleteEmployees, container, cancellationToken); // ignore snapshot deletion failure

                await SendMessagesAsync(deleteEmployees, sender, cancellationToken);
            }
        }

        private async Task<SyncListData<MqMessage<Employee>>> GetUpsertEmployeesAsync(
            OdataPagedResponse<CrmEmployee> crmEmployeesOdata,
            Container container,
            CancellationToken cancellationToken)
        {
            if (!crmEmployeesOdata.Value.Any())
            {
                return new SyncListData<MqMessage<Employee>>();
            }

            string GetSqlQueryText(IEnumerable<string> ids)
            {
                var formattedIds = ids.Select(id => id.EnsureSurroundsWith("'"));
                return $"SELECT * FROM c WHERE c.organizationId={_syncOptions.OrganizationId} AND c.id IN ({string.Join(",", formattedIds)})";
            }

            var syncVersion = Guid.NewGuid().ToString();
            var crmEmployeeIds = crmEmployeesOdata.Value.Select(crmEmployee => crmEmployee.Systemuserid);
            var queryDefinition = new QueryDefinition(GetSqlQueryText(crmEmployeeIds));

            using var crmEmployeeSnapshotsIterator = container.GetItemQueryIterator<SyncSnapshot>(queryDefinition);
            var crmEmployeeSnapshots = new List<SyncSnapshot>();
            var syncedSnapshotIds = new List<string>();

            while (crmEmployeeSnapshotsIterator.HasMoreResults)
            {
                var currentSnapshotSet = await crmEmployeeSnapshotsIterator.ReadNextAsync(cancellationToken);
                foreach (var snapshot in currentSnapshotSet)
                {
                    crmEmployeeSnapshots.Add(snapshot);
                }
            }

            var upsertEmployees = new List<MqMessage<Employee>>();
            foreach (var crmEmployee in crmEmployeesOdata.Value)
            {
                var previousSnapshot = crmEmployeeSnapshots.FirstOrDefault(snapshot => snapshot.Id == crmEmployee.Systemuserid);

                if (previousSnapshot != null)
                {
                    syncedSnapshotIds.Add(previousSnapshot.Id);
                }

                if (previousSnapshot == null || previousSnapshot.Modifiedon < crmEmployee.Modifiedon)
                {
                    upsertEmployees.Add(new MqMessage<Employee>(
                        Employee.FromCrmEmployee(crmEmployee),
                        previousSnapshot == null ? SyncMode.Create : SyncMode.Update,
                        crmEmployee.Modifiedon,
                        syncVersion));
                }
            }

            // update sync version
            var syncVersionOperations = new List<PatchOperation>
            {
                PatchOperation.Set("/syncVersion", syncVersion)
            };

            var patitionKey = new PartitionKey(crmEmployeesOdata.Value.First().Organizationid);
            var batch = container.CreateTransactionalBatch(patitionKey);
            foreach (var syncedSnapshotId in syncedSnapshotIds)
            {
                batch.PatchItem(syncedSnapshotId, syncVersionOperations);
            }

            using var batchResponse = await batch.ExecuteAsync(cancellationToken);
            if (!batchResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to batch update sync version: [{StatusCode}] {Message}", batchResponse.StatusCode, batchResponse.ErrorMessage);
            }

            return new SyncListData<MqMessage<Employee>>(upsertEmployees, syncVersion, batchResponse.IsSuccessStatusCode);
        }

        private async Task<List<MqMessage<Employee>>> UpdateSnapshotAsync(
            IEnumerable<MqMessage<Employee>> employees,
            Container container,
            CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();
            var successfulEmployees = new List<MqMessage<Employee>>();

            Action<Task<ItemResponse<SyncSnapshot>>> GetContinueAction(MqMessage<Employee> employee)
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

            foreach (var employee in employees)
            {
                var patitionKey = new PartitionKey(employee.Message.OrganisationId);

                if (employee.SyncMode == SyncMode.Create || employee.SyncMode == SyncMode.Update)
                {
                    var snapshot = employee.Message.ToSnapshot(employee.ModifiedOn, employee.SyncVersion);
                    tasks.Add(container.UpsertItemAsync(
                        snapshot,
                        patitionKey,
                        cancellationToken: cancellationToken)
                        .ContinueWith(GetContinueAction(employee), cancellationToken));
                }
                else if (employee.SyncMode == SyncMode.Delete)
                {
                    tasks.Add(container.DeleteItemAsync<SyncSnapshot>(
                        employee.Message.UserId,
                        patitionKey,
                        cancellationToken: cancellationToken)
                        .ContinueWith(GetContinueAction(employee), cancellationToken));
                }
            }

            await Task.WhenAll(tasks);

            return successfulEmployees;
        }

        private async Task SendMessagesAsync(IEnumerable<MqMessage<Employee>> messages, ServiceBusSender sender, CancellationToken cancellationToken)
        {
            if (!messages.Any())
            {
                return;
            }

            using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync(cancellationToken);
            var jsonSerializeOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var totalMessageCount = messages.Count();
            var addedMessageCount = totalMessageCount;
            foreach (var message in messages)
            {
                if (!messageBatch.TryAddMessage(new ServiceBusMessage(BinaryData.FromObjectAsJson(message, jsonSerializeOptions))))
                {
                    _logger.LogError("Failed to add message to batch: {Message}", message);
                    addedMessageCount--;
                }
            }

            await sender.SendMessagesAsync(messageBatch, cancellationToken);
            _logger.LogInformation($"A batch of {addedMessageCount}/{totalMessageCount} messages " +
                $"has been published to the topic {_syncOptions.EmployeeSync.TopicName}.");
        }
    }
}
