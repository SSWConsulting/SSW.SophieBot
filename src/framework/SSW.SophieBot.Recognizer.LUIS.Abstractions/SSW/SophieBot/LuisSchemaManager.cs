using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
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
            var clEntities = SchemaOptions.OfModelType<IClosedList>();
            var entities = SchemaOptions.OfModelType<IEntity>();

            await PublishClEntitiesAsync(clEntities, cancellationToken);
            await PublishEntityAsync(entities, cancellationToken);
        }

        protected virtual async Task PublishClEntitiesAsync(IEnumerable<Type> entities, CancellationToken cancellationToken = default)
        {
            if (entities.IsNullOrEmpty())
            {
                return;
            }

            if (!AppId.HasValue || Version.IsNullOrEmpty())
            {
                (AppId, Version) = await LuisHelper.GetLuisAppIdAndActiveVersionAsync(LuisAuthoringClient, LuisOptions, cancellationToken);
            }

            foreach (var entity in entities)
            {
                var createObject = LuisHelper.GetClEntityCreateObject(entity);
                await LuisAuthoringClient.EnsureClEntityExistAsync(
                    AppId.Value,
                    Version,
                    createObject,
                    cancellationToken);
            }
        }

        protected virtual async Task PublishEntityAsync(IEnumerable<Type> entities, CancellationToken cancellationToken = default)
        {
            if (entities.IsNullOrEmpty())
            {
                return;
            }

            if (!AppId.HasValue || Version.IsNullOrEmpty())
            {
                (AppId, Version) = await LuisHelper.GetLuisAppIdAndActiveVersionAsync(LuisAuthoringClient, LuisOptions, cancellationToken);
            }

            foreach (var entity in entities)
            {
                // only process root entity as their children will be handled in the same iteration
                if (ChildOfAttribute.GetParentOrDefault(entity) != null)
                {
                    continue;
                }

                var createObject = LuisHelper.GetEntityCreateObject(entity, entities);
                var entityId = await LuisAuthoringClient.EnsureEntityExistAsync(AppId.Value, Version, createObject, cancellationToken);

                var featureCreateObjects = LuisHelper.GetEntityFeatureRelationModels(entity)
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
        }
    }
}
