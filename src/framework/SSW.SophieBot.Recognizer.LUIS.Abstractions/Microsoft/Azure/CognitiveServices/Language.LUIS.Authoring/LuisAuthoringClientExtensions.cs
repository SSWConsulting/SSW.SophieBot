using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using SSW.SophieBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring
{
    public static class LuisAuthoringClientExtensions
    {
        public static async Task<string> GetActiveVersionAsync(
            this ILUISAuthoringClient client,
            Guid appId,
            CancellationToken cancellationToken = default)
        {
            var appInfo = await client.Apps.GetAsync(appId, cancellationToken);
            return appInfo?.ActiveVersion ?? string.Empty;
        }

        public static async Task<Guid?> GetEntityIdAsync(
            this ILUISAuthoringClient client,
            Guid appId,
            string entityName,
            string version,
            CancellationToken cancellationToken = default)
        {
            if (entityName.IsNullOrEmpty())
            {
                return null;
            }

            var entitiesPages = client.GetAsyncPagedEntities(appId, version, cancellationToken: cancellationToken);

            await foreach (var entities in entitiesPages)
            {
                if (!entities.IsNullOrEmpty())
                {
                    var entity = entities.FirstOrDefault(entity => entity.Name == entityName);
                    if (entity != null)
                    {
                        return entity.Id;
                    }
                }
            }

            return null;
        }

        public static async Task<Guid?> GetClEntityIdAsync(
            this ILUISAuthoringClient client,
            Guid appId,
            string clName,
            string version,
            CancellationToken cancellationToken = default)
        {
            if (clName.IsNullOrEmpty())
            {
                return null;
            }

            var clEntitiesPages = client.GetAsyncPagedClEntities(appId, version, cancellationToken: cancellationToken);

            await foreach (var clEntities in clEntitiesPages)
            {
                if (!clEntities.IsNullOrEmpty())
                {
                    var clEntity = clEntities.FirstOrDefault(entity => entity.Name == clName);
                    if (clEntity != null)
                    {
                        return clEntity.Id;
                    }
                }
            }

            return null;
        }

        public static async IAsyncEnumerable<IEnumerable<ClosedListEntityExtractor>> GetAsyncPagedClEntities(
            this ILUISAuthoringClient client,
            Guid appId,
            string version,
            int pageSize = 100,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var pageIndex = 0;
            var hasMore = true;

            while (hasMore)
            {
                var skip = pageSize * pageIndex;
                var clEntities = await client.Model.ListClosedListsAsync(appId, version, skip, pageSize, cancellationToken);

                hasMore = clEntities != null && clEntities.Count == pageSize;
                pageIndex++;

                yield return clEntities ?? Enumerable.Empty<ClosedListEntityExtractor>();
            }
        }

        public static async IAsyncEnumerable<IEnumerable<EntityExtractor>> GetAsyncPagedEntities(
            this ILUISAuthoringClient client,
            Guid appId,
            string version,
            int pageSize = 100,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var pageIndex = 0;
            var hasMore = true;

            while (hasMore)
            {
                var skip = pageSize * pageIndex;
                var entities = await client.Model.ListEntitiesAsync(appId, version, skip, pageSize, cancellationToken);

                hasMore = entities != null && entities.Count == pageSize;
                pageIndex++;

                yield return entities ?? Enumerable.Empty<EntityExtractor>();
            }
        }

        public static async Task<bool> TrainAndPublishAppAsync(
            this ILUISAuthoringClient client,
            Guid appId,
            string version,
            CancellationToken cancellationToken = default)
        {
            const int maxWaitSeconds = 30;
            var finishStates = new string[] { "Success", "UpToDate" };

            await client.Train.TrainVersionAsync(appId, version, cancellationToken);
            var modelResults = await client.Train.GetStatusAsync(appId, version, cancellationToken);

            var waitSeconds = 0;
            while (!modelResults.All(modelResult => finishStates.Contains(modelResult.Details.Status)))
            {
                if (waitSeconds > maxWaitSeconds)
                {
                    return false;
                }

                await Task.Delay(1000);
                waitSeconds++;

                modelResults = await client.Train.GetStatusAsync(appId, version, cancellationToken);
            }

            await client.Apps.PublishAsync(appId, new ApplicationPublishObject(version), cancellationToken);

            return true;
        }

        public static async Task<Guid> EnsureEntityExistAsync(
            this ILUISAuthoringClient client,
            Guid appId,
            string version,
            HierarchicalEntityModel createObject,
            CancellationToken cancellationToken = default)
        {
            var entityId = await client.GetEntityIdAsync(appId, createObject.Name, version, cancellationToken);
            if (entityId.HasValue)
            {
                await client.Model.DeleteEntityAsync(appId, version, entityId.Value, cancellationToken);
            }

            return await client.Model.AddHierarchicalEntityAsync(appId, version, createObject, cancellationToken);
        }

        public static async Task<Guid> EnsureClEntityExistAsync(
            this ILUISAuthoringClient client,
            Guid appId,
            string version,
            ClosedListModelCreateObject createObject,
            CancellationToken cancellationToken = default)
        {
            var clEntityId = await client.GetClEntityIdAsync(appId, createObject.Name, version, cancellationToken);
            if (clEntityId.HasValue)
            {
                await client.Model.DeleteClosedListAsync(appId, version, clEntityId.Value, cancellationToken);
            }

            return await client.Model.AddClosedListAsync(appId, version, createObject, cancellationToken);
        }

        public static async Task<OperationStatus> UpdateEntityFeatureRelationAsync(
            this IModel model,
            string version,
            Guid entityId,
            IList<EntityFeatureRelationCreateObject> createObjects,
            HttpClient httpClient,
            LuisOptions luisOptions,
            CancellationToken cancellationToken = default)
        {
            // TODO: consider using Microsoft.Rest
            var uri = new Uri($"https://{luisOptions.AuthoringEndpoint}/luis/authoring/v3.0-preview/apps/{luisOptions.AppId}" +
                $"/versions/{version}/entities/{entityId}/features");

            var content = new StringContent(JsonSerializer.Serialize(createObjects), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Content = content
            };
            request.Headers.Add("Ocp-Apim-Subscription-Key", luisOptions.AuthoringKey);

            using var response = await httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return JsonSerializer.Deserialize<OperationStatus>(await response.Content.ReadAsStringAsync());
        }
    }
}
