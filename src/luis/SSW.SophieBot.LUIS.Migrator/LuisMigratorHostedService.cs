using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.ClosedListEntity;

namespace SSW.SophieBot.LUIS.Migrator
{
    public class LuisMigratorHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IPeopleClient _peopleClient;
        private readonly ILUISAuthoringClient _luisAuthoringClient;
        private readonly SswPeopleNames _sswPeopleNamesClEntity;
        private readonly LuisOptions _luisOptions;
        private readonly ILogger<LuisMigratorHostedService> _logger;

        public LuisMigratorHostedService(
            IHostApplicationLifetime hostApplicationLifetime,
            IPeopleClient peopleClient,
            ILUISAuthoringClient luisAuthoringClient,
            SswPeopleNames sswPeopleNamesClEntity,
            IOptions<LuisOptions> luisOptions,
            ILogger<LuisMigratorHostedService> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _peopleClient = peopleClient;
            _luisAuthoringClient = luisAuthoringClient;
            _sswPeopleNamesClEntity = sswPeopleNamesClEntity;
            _luisOptions = luisOptions.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var appId = _luisOptions.GetGuidAppId();
            if (appId == Guid.Empty)
            {
                _logger.LogWarning("Failed to get LUIS app ID from configuration, end execution");
                return;
            }

            var activeVersion = await _luisAuthoringClient.GetActiveVersionAsync(appId, cancellationToken);
            var clEntityId = await _luisAuthoringClient.GetClEntityIdAsync(appId, _sswPeopleNamesClEntity.EntityName, activeVersion, cancellationToken);

            if (!clEntityId.HasValue)
            {
                clEntityId = await _luisAuthoringClient.Model.AddClosedListAsync(
                    appId,
                    activeVersion,
                    new ClosedListModelCreateObject(new List<WordListObject>(), _sswPeopleNamesClEntity.EntityName),
                    cancellationToken);
            }

            var employeePages = _peopleClient.GetAsyncPagedEmployees(cancellationToken);
            var subLists = new List<WordListObject>();

            await foreach (var employees in employeePages)
            {
                subLists.AddRange(employees.Select(employee => _sswPeopleNamesClEntity.CreateWordList(employee)));
            }

            var updateObject = new ClosedListModelUpdateObject(subLists, _sswPeopleNamesClEntity.EntityName);
            var updateResponse = await _luisAuthoringClient.Model.UpdateClosedListAsync(
                appId,
                activeVersion,
                clEntityId.Value,
                updateObject,
                cancellationToken);

            await _luisAuthoringClient.TrainAndPublishAppAsync(appId, activeVersion, cancellationToken);

            _hostApplicationLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
