using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Entities;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.Sync;
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

        private readonly ILuisService _luisService;
        private readonly PersonNames _sswPersonNamesClEntity;
        private readonly ILogger<ListEntitySync> _logger;

        public ListEntitySync(
            ILuisService luisService,
            PersonNames sswPersonNamesClEntity,
            ILogger<ListEntitySync> logger)
        {
            _luisService = luisService;
            _sswPersonNamesClEntity = sswPersonNamesClEntity;
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

            var batchContentEmployees = employees.Where(employee => employee.BatchMode == BatchMode.BatchContent);

            if (batchContentEmployees.Any())
            {
                await UpdateClosedListEntityAsync(batchContentEmployees, cancellationToken);
            }

            if (employees.Any(employee => employee.BatchMode == BatchMode.BatchEnd))
            {
                await _luisService.TrainAndPublishAppAsync(cancellationToken);
            }
        }

        private async Task UpdateClosedListEntityAsync(
            IEnumerable<MqMessage<Employee>> employees,
            CancellationToken cancellationToken = default)
        {
            var clEntityName = ModelAttribute.GetName(typeof(PersonNames));

            // TODO: We are currently retriving all sub lists from LUIS in a single call due to the limitation of the REST API. 
            // May be a bottleneck here but it's of low priority
            var clEntity = await _luisService.FindClosedListAsync(clEntityName, cancellationToken);
            if (clEntity == null)
            {
                // if this cl entity does not exist, it's the LUIS build/publish service's responsibility to do the creation, but not this sync service
                // (e.g. create this sswPeopleNames list entity and feed data by calling People API)
                // so we can just log a warning and exit execution here
                _logger.LogWarning("Failed to get target closed list entity: {EntityName}", clEntityName);
                return;
            }

            var sublist = clEntity.SubLists;

            _logger.LogDebug("Retrived closed entity sublists for {EntityName}: {Count}", clEntityName, sublist.Count);

            var newSubLists = sublist?.Select(item => new WordListObject(item.CanonicalForm, item.List))?.ToList() ?? new List<WordListObject>();

            foreach (var employee in employees)
            {
                var wordListObject = _sswPersonNamesClEntity.CreateWordList(employee.Message);

                newSubLists.RemoveAll(item => item.CanonicalForm == wordListObject.CanonicalForm);
                if (employee.SyncMode == SyncMode.Create || employee.SyncMode == SyncMode.Update)
                {
                    newSubLists.Add(wordListObject);
                }
            }

            _logger.LogDebug("Calculated new closed entity sublists for {EntityName}: {Count}", clEntityName, newSubLists.Count);

            var updateObject = new ClosedListModelUpdateObject(newSubLists, clEntityName);
            var updateResponse = await _luisService.UpdateClosedListAsync(clEntity.Id, updateObject, cancellationToken);
        }
    }
}
