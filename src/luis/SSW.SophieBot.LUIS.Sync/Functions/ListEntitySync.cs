using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.ClosedListEntity;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.LUIS.Sync.Functions
{
    // see https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/cognitiveservices/Language.LUIS.Authoring/tests/Luis/ModelClosedListsTests.cs for samples
    public class ListEntitySync
    {
        private const string SbConnectionStringName = "ServiceBus";

        private readonly ILUISAuthoringClient _luisAuthoringClient;
        private readonly SswPeopleNames _sswPeopleNamesClEntity;
        private readonly LuisOptions _options;
        private readonly ILogger<ListEntitySync> _logger;

        public ListEntitySync(
            ILUISAuthoringClient luisAuthoringClient,
            SswPeopleNames sswPeopleNamesClEntity,
            IOptions<LuisOptions> options,
            ILogger<ListEntitySync> logger)
        {
            _luisAuthoringClient = luisAuthoringClient;
            _sswPeopleNamesClEntity = sswPeopleNamesClEntity;
            _options = options.Value;
            _logger = logger;
        }

        [FunctionName(nameof(SyncSswPeopleNames))]
        public async Task SyncSswPeopleNames([ServiceBusTrigger(
            "%ServiceBus:SswPeopleNames:Topic%",
            "%ServiceBus:SswPeopleNames:Subscription%",
            Connection = SbConnectionStringName)] IEnumerable<MqMessage<Employee>> employees,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("Received employees from service bus: {Count}", employees.Count());

            var appId = _options.GetGuidAppId();
            if (appId == Guid.Empty)
            {
                _logger.LogWarning("Failed to get LUIS app ID from configuration, end execution");
                return;
            }

            var activeVersion = await _luisAuthoringClient.GetActiveVersionAsync(appId, cancellationToken);

            var batchContentEmployees = employees.Where(employee => employee.BatchMode == BatchMode.BatchContent);

            if (batchContentEmployees.Any())
            {
                await UpdateClosedListEntityAsync(batchContentEmployees, appId, activeVersion, cancellationToken);
            }

            if (employees.Any(employee => employee.BatchMode == BatchMode.BatchEnd))
            {
                await _luisAuthoringClient.TrainAndPublishAppAsync(appId, activeVersion, cancellationToken);
            }
        }

        private async Task UpdateClosedListEntityAsync(
            IEnumerable<MqMessage<Employee>> employees,
            Guid appId,
            string activeVersion,
            CancellationToken cancellationToken = default)
        {
            var clEntityId = await _luisAuthoringClient.GetClEntityIdAsync(appId, _sswPeopleNamesClEntity.EntityName, activeVersion, cancellationToken);

            if (!clEntityId.HasValue)
            {
                // if this cl entity does not exist, it's the LUIS build/publish service's responsibility to do the creation, but not this sync service
                // (e.g. create this sswPeopleNames list entity and feed data by calling People API)
                // so we can just log a warning and exit execution here
                _logger.LogWarning("Failed to get target closed list entity: {EntityName}", _sswPeopleNamesClEntity.EntityName);
                return;
            }

            _logger.LogDebug("Retrived LUIS app active version: {ActiveVersion}", activeVersion);
            _logger.LogDebug("Retrived closed entity ID for {EntityName}: {ClEntityId}", _sswPeopleNamesClEntity.EntityName, clEntityId);

            // TODO: We are currently retriving all sub lists from LUIS in a single call due to the limitation of the REST API. 
            // May be a bottleneck here but it's of low priority
            var clEntity = await _luisAuthoringClient.Model.GetClosedListAsync(appId, activeVersion, clEntityId.Value, cancellationToken);
            var sublist = clEntity.SubLists;

            _logger.LogDebug("Retrived closed entity sublists for {EntityName}: {Count}", _sswPeopleNamesClEntity.EntityName, sublist.Count);

            var newSubLists = sublist?.Select(item => new WordListObject(item.CanonicalForm, item.List))?.ToList() ?? new List<WordListObject>();

            foreach (var employee in employees)
            {
                var canonicalForm = _sswPeopleNamesClEntity.GetCanonicalForm(employee.Message);

                newSubLists.RemoveAll(item => item.CanonicalForm == canonicalForm);
                if (employee.SyncMode == SyncMode.Create || employee.SyncMode == SyncMode.Update)
                {
                    newSubLists.Add(_sswPeopleNamesClEntity.CreateWordList(employee.Message));
                }
            }

            _logger.LogDebug("Calculated new closed entity sublists for {EntityName}: {Count}", _sswPeopleNamesClEntity.EntityName, newSubLists.Count);

            var updateObject = new ClosedListModelUpdateObject(newSubLists, _sswPeopleNamesClEntity.EntityName);
            var updateResponse = await _luisAuthoringClient.Model.UpdateClosedListAsync(
                appId,
                activeVersion,
                clEntityId.Value,
                updateObject,
                cancellationToken);

            // LUIS SDK v3-preview has a bug in UpdateClosedListAsync, which will always cause an empty response 
            //if (updateResponse.Code != OperationStatusType.Success)
            //{
            //    _logger.LogError(
            //        "Failed to update closed list entity: {EntityName}, {Message}",
            //        _sswPeopleNamesClEntity.EntityName,
            //        updateResponse.Message);
            //}
        }
    }
}
