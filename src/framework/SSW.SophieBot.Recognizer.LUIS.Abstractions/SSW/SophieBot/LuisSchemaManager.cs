using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public class LuisSchemaManager : RecognizerSchemaManagerBase
    {
        protected HttpClient HttpClient { get; }

        protected ILUISAuthoringClient LuisAuthoringClient { get; }

        protected LuisOptions LuisOptions { get; }

        protected Guid AppId { get; set; }

        protected string Version { get; set; }

        protected ILogger<LuisSchemaManager> Logger { get; }

        public LuisSchemaManager(
            HttpClient httpClient,
            ILUISAuthoringClient luisAuthoringClient,
            IServiceProvider serviceProvider,
            IOptions<RecognizerSchemaOptions> schemaOptions,
            IOptions<LuisOptions> luisOptions,
            ILogger<LuisSchemaManager> logger)
            : base(serviceProvider, schemaOptions)
        {
            HttpClient = httpClient;
            LuisAuthoringClient = luisAuthoringClient;
            LuisOptions = luisOptions.Value;
            Logger = logger;
        }

        public override async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            await base.SeedAsync(cancellationToken);

            var publishResult = await LuisAuthoringClient.TrainAndPublishAppAsync(AppId, Version, cancellationToken);
            if (!publishResult)
            {
                Logger.LogError("Failed to train and publish LUIS app: {AppId} - v{Version}", AppId, Version);
            }
            else
            {
                Logger.LogInformation("Successful to train and publish LUIS app: {AppId} - v{Version}", AppId, Version);
            }
        }

        public override async Task PublishSchemaAsync(CancellationToken cancellationToken = default)
        {
            await SetAppIdAndVersionAsync(cancellationToken);

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

        protected virtual async Task SetAppIdAndVersionAsync(CancellationToken cancellationToken = default)
        {
            (AppId, Version) = await LuisHelper.GetLuisAppIdAndActiveVersionAsync(LuisAuthoringClient, LuisOptions, cancellationToken);
        }

        protected virtual async Task PublishPrebuiltEntityAsync(IList<string> prebuiltEntityNames, CancellationToken cancellationToken = default)
        {
            await LuisAuthoringClient.EnsurePrebuiltEntityExistAsync(AppId, Version, prebuiltEntityNames, cancellationToken);
        }

        protected virtual async Task PublishEntityAsync(Type entityType, CancellationToken cancellationToken = default)
        {
            var entityId = Guid.Empty;
            var parentEntityType = ChildOfAttribute.GetParentOrDefault(entityType);

            if (parentEntityType == null)
            {
                var createObject = LuisHelper.GetEntityCreateObject(entityType);
                entityId = await LuisAuthoringClient.EnsureEntityExistAsync(AppId, Version, createObject, cancellationToken);
            }
            else
            {
                var childEntityName = ModelAttribute.GetName(entityType);
                var parentEntityName = ModelAttribute.GetName(parentEntityType);
                var parentEntityId = await LuisAuthoringClient.GetEntityIdAsync(AppId, parentEntityName, Version, cancellationToken);

                if (!parentEntityId.HasValue)
                {
                    throw new LuisException($"Failed to create child entity {childEntityName} because parent entity has not been created yet.");
                }

                entityId = await LuisAuthoringClient.CreateEntityChildAsync(
                    Version,
                    parentEntityId.Value,
                    childEntityName,
                    HttpClient,
                    LuisOptions,
                    cancellationToken);

                if (entityId == default)
                {
                    LuisHelper.FailOperation();
                }
            }

            var featureCreateObjects = LuisHelper.GetEntityFeatureRelationModels(entityType)
                .Select(featureModel => EntityFeatureRelationCreateObject.FromModel(featureModel));

            var updateFeatureResponse = await LuisAuthoringClient.UpdateEntityFeatureRelationAsync(
                Version,
                entityId,
                featureCreateObjects.ToList(),
                HttpClient,
                LuisOptions,
                cancellationToken);

            updateFeatureResponse.EnsureSuccessOperationStatus();
        }

        protected virtual async Task PublishClEntityAsync(Type entityType, CancellationToken cancellationToken = default)
        {
            var createObject = LuisHelper.GetClEntityCreateObject(entityType);
            await LuisAuthoringClient.EnsureClEntityExistAsync(
                AppId,
                Version,
                createObject,
                cancellationToken);
        }
    }
}
