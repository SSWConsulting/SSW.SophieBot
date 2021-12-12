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

        public override Task PublishSchemaAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected virtual async Task PublishEntityAsync(IEnumerable<Type> entities, CancellationToken cancellationToken = default)
        {
            if (entities.IsNullOrEmpty())
            {
                return;
            }

            var (appId, activeVersion) = await LuisHelper.GetLuisAppIdAndActiveVersionAsync(LuisAuthoringClient, LuisOptions, cancellationToken);

            foreach (var entity in entities)
            {
                if (ChildOfAttribute.GetParentOrDefault(entity) != null)
                {
                    continue;
                }

                var createObject = LuisHelper.GetEntityCreateObject(entity, entities);
                var entityId = await LuisAuthoringClient.EnsureEntityExistAsync(appId, activeVersion, createObject, cancellationToken);

                var featureCreateObjects = LuisHelper.GetEntityFeatureRelationModels(entity)
                    .Select(featureModel => EntityFeatureRelationCreateObject.FromModel(featureModel));

                var updateFeatureResponse = await LuisAuthoringClient.Model.UpdateEntityFeatureRelationAsync(
                    activeVersion,
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
