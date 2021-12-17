using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public class LuisSchemaManager : RecognizerSchemaManagerBase
    {
        protected ILuisService LuisService { get; }

        protected ILogger<LuisSchemaManager> Logger { get; }

        public LuisSchemaManager(
            ILuisService luisService,
            IServiceProvider serviceProvider,
            IOptions<RecognizerSchemaOptions> schemaOptions,
            ILogger<LuisSchemaManager> logger)
            : base(serviceProvider, schemaOptions)
        {
            LuisService = luisService;
            Logger = logger;
        }

        public override async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await base.SeedAsync(cancellationToken);

            var publishResult = await LuisService.TrainAndPublishAppAsync(cancellationToken);
            if (!publishResult)
            {
                Logger.LogError("Failed to train and publish LUIS app: {AppId} - v{Version}", LuisService.AppId, LuisService.Version);
            }
            else
            {
                Logger.LogInformation("Successful to train and publish LUIS app: {AppId} - v{Version}", LuisService.AppId, LuisService.Version);
            }
        }

        public override async Task PublishSchemaAsync(CancellationToken cancellationToken = default)
        {
            var modelTypesToPublish = ChooseModelTypesToPublish();
            var prebuiltEntityNames = modelTypesToPublish
                .Where(type => typeof(IPrebuiltEntity).IsAssignableFrom(type))
                .Select(type => ModelAttribute.GetName(type))
                .ToList();

            if (prebuiltEntityNames.Any())
            {
                await PublishPrebuiltEntityAsync(prebuiltEntityNames, cancellationToken);
            }

            foreach (var modelType in ChooseModelTypesToPublish())
            {
                if (typeof(IEntity).IsAssignableFrom(modelType))
                {
                    await PublishEntityAsync(modelType, cancellationToken);
                }
                else if (typeof(IClosedList).IsAssignableFrom(modelType))
                {
                    await PublishClEntityAsync(modelType, cancellationToken);
                }

                // TODO: intent, example, pattern
            }
        }

        protected virtual async Task PublishPrebuiltEntityAsync(IList<string> prebuiltEntityNames, CancellationToken cancellationToken = default)
        {
            await LuisService.EnsurePrebuiltEntityExistAsync(prebuiltEntityNames, cancellationToken);
        }

        protected virtual async Task PublishEntityAsync(Type entityType, CancellationToken cancellationToken = default)
        {
            var entityId = Guid.Empty;
            var parentEntityType = ChildOfAttribute.GetParentOrDefault(entityType);

            if (parentEntityType == null)
            {
                var createObject = LuisHelper.GetEntityCreateObject(entityType);
                entityId = await LuisService.EnsureEntityExistAsync(createObject, cancellationToken);
            }
            else
            {
                var childEntityName = ModelAttribute.GetName(entityType);
                var parentEntityName = ModelAttribute.GetName(parentEntityType);
                var parentEntityId = await LuisService.GetEntityIdAsync(parentEntityName, cancellationToken);

                if (!parentEntityId.HasValue)
                {
                    throw new LuisException($"Failed to create child entity {childEntityName} because parent entity has not been created yet.");
                }

                entityId = await LuisService.CreateEntityChildAsync(parentEntityId.Value, childEntityName, cancellationToken);

                if (entityId == default)
                {
                    LuisHelper.FailOperation();
                }
            }

            var featureCreateObjects = LuisHelper.GetEntityFeatureRelationModels(entityType)
                .Select(featureModel => EntityFeatureRelationCreateObject.FromModel(featureModel));

            var updateFeatureResponse = await LuisService.UpdateEntityFeatureRelationAsync(entityId, featureCreateObjects.ToList(), cancellationToken);

            updateFeatureResponse.EnsureSuccessOperationStatus();
        }

        protected virtual async Task PublishClEntityAsync(Type entityType, CancellationToken cancellationToken = default)
        {
            var createObject = LuisHelper.GetClEntityCreateObject(entityType);
            await LuisService.EnsureClEntityExistAsync(createObject, cancellationToken);
        }
    }
}
