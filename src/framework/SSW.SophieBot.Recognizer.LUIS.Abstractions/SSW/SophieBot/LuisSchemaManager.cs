using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Extensions.Options;
using System;
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

        protected Guid? AppId { get; set; }

        protected string Version { get; set; }

        public LuisSchemaManager(
            HttpClient httpClient,
            ILUISAuthoringClient luisAuthoringClient,
            IServiceProvider serviceProvider,
            IOptions<RecognizerSchemaOptions> schemaOptions,
            IOptions<LuisOptions> luisOptions)
            : base(serviceProvider, schemaOptions)
        {
            HttpClient = httpClient;
            LuisAuthoringClient = luisAuthoringClient;
            LuisOptions = luisOptions.Value;
        }

        public override async Task PublishSchemaAsync(CancellationToken cancellationToken = default)
        {
            await SetAppIdAndVersionAsync(cancellationToken);

            foreach (var modelType in SchemaOptions.ModelTypes)
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

        protected virtual async Task PublishEntityAsync(Type entityType, CancellationToken cancellationToken = default)
        {
            var entityId = Guid.Empty;
            var parentEntityType = ChildOfAttribute.GetParentOrDefault(entityType);

            if (parentEntityType == null)
            {
                var createObject = LuisHelper.GetEntityCreateObject(entityType);
                entityId = await LuisAuthoringClient.EnsureEntityExistAsync(AppId.Value, Version, createObject, cancellationToken);
            }
            else
            {
                var createObject = LuisHelper.GetChildEntityCreateObject(entityType);
                var parentEntityName = ModelAttribute.GetName(parentEntityType);
                var parentEntityId = await LuisAuthoringClient.GetEntityIdAsync(AppId.Value, parentEntityName, Version, cancellationToken);

                if (!parentEntityId.HasValue)
                {
                    throw new LuisException($"Failed to create child entity {createObject.Name} because parent entity has not been created yet.");
                }

                var childEntityId = await LuisAuthoringClient.Model.AddHierarchicalEntityChildAsync(
                    AppId.Value,
                    Version,
                    parentEntityId.Value,
                    createObject,
                    cancellationToken);

                if (childEntityId == default)
                {
                    LuisHelper.FailOperation();
                }
            }

            var featureCreateObjects = LuisHelper.GetEntityFeatureRelationModels(entityType)
                .Select(featureModel => EntityFeatureRelationCreateObject.FromModel(featureModel));

            var updateFeatureResponse = await LuisAuthoringClient.Model.UpdateEntityFeatureRelationAsync(
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
                AppId.Value,
                Version,
                createObject,
                cancellationToken);
        }
    }
}
