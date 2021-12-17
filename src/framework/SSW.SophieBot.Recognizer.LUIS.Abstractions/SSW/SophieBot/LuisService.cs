using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public class LuisService : ILuisService
    {
        public Guid AppId { get; protected set; }

        public string Version { get; protected set; }

        protected ILUISAuthoringClient AuthoringClient { get; }

        protected HttpClient HttpClient { get; }

        protected LuisOptions Options { get; }

        public LuisService(
            ILUISAuthoringClient authoringClient,
            HttpClient httpClient,
            IOptions<LuisOptions> options)
        {
            AuthoringClient = authoringClient;
            HttpClient = httpClient;
            Options = options.Value;

            AppId = Options.GetGuidAppId();
            if (AppId == default)
            {
                throw new LuisException("Failed to get LUIS app ID from configuration");
            }
        }

        public virtual async Task<string> GetActiveVersionAsync(CancellationToken cancellationToken = default)
        {
            var appInfo = await GetAppInfoAsync(cancellationToken);
            return appInfo?.ActiveVersion ?? string.Empty;
        }

        public virtual async Task<ApplicationInfoResponse> GetAppInfoAsync(CancellationToken cancellationToken = default)
        {
            return await AuthoringClient.Apps.GetAsync(AppId, cancellationToken);
        }

        public virtual async Task<Guid?> GetEntityIdAsync(string entityName, CancellationToken cancellationToken = default)
        {
            if (entityName.IsNullOrEmpty())
            {
                return null;
            }

            await SetVersionAsync();

            var entitiesPages = GetAsyncPagedEntities(cancellationToken: cancellationToken);

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

        public virtual async IAsyncEnumerable<IEnumerable<EntityExtractor>> GetAsyncPagedEntities(
            int pageSize = 100,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var pageIndex = 0;
            var hasMore = true;

            await SetVersionAsync();

            while (hasMore)
            {
                var skip = pageSize * pageIndex;
                var entities = await ListEntitiesAsync(skip, pageSize, cancellationToken);

                hasMore = entities != null && entities.Count == pageSize;
                pageIndex++;

                yield return entities ?? Enumerable.Empty<EntityExtractor>();
            }
        }

        public virtual async Task<IList<EntityExtractor>> ListEntitiesAsync(
            int? skip = 0,
            int? take = 100,
            CancellationToken cancellationToken = default)
        {
            await SetVersionAsync();
            return await AuthoringClient.Model.ListEntitiesAsync(AppId, Version, skip, take, cancellationToken);
        }

        public virtual async Task<Guid?> GetClEntityIdAsync(string clName, CancellationToken cancellationToken = default)
        {
            if (clName.IsNullOrEmpty())
            {
                return null;
            }

            await SetVersionAsync();

            var clEntitiesPages = GetAsyncPagedClEntities(cancellationToken: cancellationToken);

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

        public virtual async IAsyncEnumerable<IEnumerable<ClosedListEntityExtractor>> GetAsyncPagedClEntities(
            int pageSize = 100,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var pageIndex = 0;
            var hasMore = true;

            await SetVersionAsync();

            while (hasMore)
            {
                var skip = pageSize * pageIndex;
                var clEntities = await ListClosedListsAsync(skip, pageSize, cancellationToken);

                hasMore = clEntities != null && clEntities.Count == pageSize;
                pageIndex++;

                yield return clEntities ?? Enumerable.Empty<ClosedListEntityExtractor>();
            }
        }

        public virtual async Task<IList<ClosedListEntityExtractor>> ListClosedListsAsync(
            int? skip = 0,
            int? take = 100,
            CancellationToken cancellationToken = default)
        {
            await SetVersionAsync();
            return await AuthoringClient.Model.ListClosedListsAsync(AppId, Version, skip, take, cancellationToken);
        }

        public virtual async Task<Guid> EnsureEntityExistAsync(
            ModelCreateObject createObject,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(createObject, nameof(createObject));

            await SetVersionAsync();

            var entityId = await GetEntityIdAsync(createObject.Name, cancellationToken);
            if (entityId.HasValue)
            {
                var deleteResponse = await DeleteEntityAsync(entityId.Value, cancellationToken);
                deleteResponse.EnsureSuccessOperationStatus();
            }

            var newEntityId = await AddEntityAsync(createObject, cancellationToken);
            if (newEntityId == default)
            {
                LuisHelper.FailOperation();
            }

            return newEntityId;
        }

        public virtual async Task<OperationStatus> DeleteEntityAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            await SetVersionAsync();
            return await AuthoringClient.Model.DeleteEntityAsync(AppId, Version, entityId, cancellationToken); ;
        }

        public virtual async Task<Guid> AddEntityAsync(ModelCreateObject modelCreateObject, CancellationToken cancellationToken = default)
        {
            await SetVersionAsync();
            return await AuthoringClient.Model.AddEntityAsync(AppId, Version, modelCreateObject, cancellationToken);
        }

        public virtual async Task<Guid> EnsureClEntityExistAsync(
            ClosedListModelCreateObject createObject,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(createObject, nameof(createObject));

            await SetVersionAsync();

            var clEntityId = await GetClEntityIdAsync(createObject.Name, cancellationToken);
            if (clEntityId.HasValue)
            {
                var deleteOperation = await DeleteClosedListAsync(clEntityId.Value, cancellationToken);
                deleteOperation.EnsureSuccessOperationStatus();
            }

            return await AddClosedListAsync(createObject, cancellationToken);
        }

        public virtual async Task<OperationStatus> DeleteClosedListAsync(Guid clEntityId, CancellationToken cancellationToken = default)
        {
            await SetVersionAsync();
            return await AuthoringClient.Model.DeleteClosedListAsync(AppId, Version, clEntityId, cancellationToken);
        }

        public virtual async Task<Guid> AddClosedListAsync(
            ClosedListModelCreateObject closedListModelCreateObject,
            CancellationToken cancellationToken = default)
        {
            await SetVersionAsync();
            return await AuthoringClient.Model.AddClosedListAsync(AppId, Version, closedListModelCreateObject, cancellationToken);
        }

        public virtual async Task<PrebuiltEntityExtractor> EnsurePrebuiltEntityExistAsync(
            IList<string> prebuiltEntityNames,
            CancellationToken cancellationToken = default)
        {
            Check.NotNullOrEmpty(prebuiltEntityNames, nameof(prebuiltEntityNames));

            await SetVersionAsync();

            var prebuiltEntities = await ListPrebuiltsAsync(cancellationToken: cancellationToken);
            var targetPrebuiltEntity = prebuiltEntities.FirstOrDefault(entity => prebuiltEntityNames.Contains(entity.Name));
            if (targetPrebuiltEntity == null)
            {
                var newPrebuiltEntities = await AddPrebuiltAsync(prebuiltEntityNames, cancellationToken);

                if (!newPrebuiltEntities.Any())
                {
                    LuisHelper.FailOperation();
                }

                return newPrebuiltEntities.First();
            }

            return targetPrebuiltEntity;
        }

        public virtual async Task<IList<PrebuiltEntityExtractor>> ListPrebuiltsAsync(
            int? skip = 0,
            int? take = 100,
            CancellationToken cancellationToken = default)
        {
            await SetVersionAsync();
            return await AuthoringClient.Model.ListPrebuiltsAsync(AppId, Version, skip, take, cancellationToken);
        }

        public virtual async Task<IList<PrebuiltEntityExtractor>> AddPrebuiltAsync(
            IList<string> prebuiltExtractorNames,
            CancellationToken cancellationToken = default)
        {
            await SetVersionAsync();
            return await AuthoringClient.Model.AddPrebuiltAsync(
                AppId,
                Version,
                prebuiltExtractorNames,
                cancellationToken);
        }

        public virtual async Task<OperationStatus> UpdateEntityFeatureRelationAsync(
            Guid entityId,
            IList<EntityFeatureRelationCreateObject> createObjects,
            CancellationToken cancellationToken = default)
        {
            // TODO: consider using Microsoft.Rest
            Check.NotNull(createObjects, nameof(createObjects));

            await SetVersionAsync();

            var uri = new Uri($"{Options.AuthoringEndpoint.EnsureEndsWith("/")}luis/authoring/v3.0-preview/apps/{Options.AppId}" +
                $"/versions/{Version}/entities/{entityId}/features");

            var content = new StringContent(JsonSerializer.Serialize(createObjects), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Content = content
            };
            request.Headers.Add("Ocp-Apim-Subscription-Key", Options.AuthoringKey);

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            return LuisHelper.SuccessOperation();
        }

        public virtual async Task<Guid> CreateEntityChildAsync(
            Guid parentEntityId,
            string childEntityName,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(childEntityName, nameof(childEntityName));

            await SetVersionAsync();

            var uri = new Uri($"{Options.AuthoringEndpoint.EnsureEndsWith("/")}luis/authoring/v3.0-preview/apps/{Options.AppId}" +
                $"/versions/{Version}/entities/{parentEntityId}/children");

            var createObject = new { name = childEntityName };

            var content = new StringContent(JsonSerializer.Serialize(createObject), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = content
            };
            request.Headers.Add("Ocp-Apim-Subscription-Key", Options.AuthoringKey);

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var newEntityId = JsonSerializer.Deserialize<Guid>(await response.Content.ReadAsStringAsync());
            return newEntityId;
        }

        public virtual async Task SetVersionAsync(string version = null, CancellationToken cancellationToken = default)
        {
            if (!version.IsNullOrWhiteSpace())
            {
                Version = version;
            }
            else if (Version == null)
            {
                Version = await GetActiveVersionAsync(cancellationToken);
            }
        }

        public virtual async Task<bool> TrainAndPublishAppAsync(CancellationToken cancellationToken = default)
        {
            const int maxWaitSeconds = 30;
            var finishStates = new string[] { "Success", "UpToDate" };

            await AuthoringClient.Train.TrainVersionAsync(AppId, Version, cancellationToken);
            var modelResults = await AuthoringClient.Train.GetStatusAsync(AppId, Version, cancellationToken);

            var waitSeconds = 0;
            while (!modelResults.All(modelResult => finishStates.Contains(modelResult.Details.Status)))
            {
                if (waitSeconds > maxWaitSeconds)
                {
                    return false;
                }

                await Task.Delay(1000);
                waitSeconds++;

                modelResults = await AuthoringClient.Train.GetStatusAsync(AppId, Version, cancellationToken);
            }

            await AuthoringClient.Apps.PublishAsync(AppId, new ApplicationPublishObject(Version), cancellationToken);

            return true;
        }

        public virtual async Task<ClosedListEntityExtractor> FindClosedListAsync(string clEntityName, CancellationToken cancellationToken = default)
        {
            Check.NotNullOrWhiteSpace(clEntityName, nameof(clEntityName));

            await SetVersionAsync();

            var clEntityId = await GetClEntityIdAsync(clEntityName, cancellationToken);
            if (!clEntityId.HasValue)
            {
                return null;
            }

            return await AuthoringClient.Model.GetClosedListAsync(AppId, Version, clEntityId.Value, cancellationToken);
        }

        public virtual async Task<OperationStatus> UpdateClosedListAsync(
            Guid clEntityId,
            ClosedListModelUpdateObject closedListModelUpdateObject,
            CancellationToken cancellationToken = default)
        {
            Check.NotNull(closedListModelUpdateObject, nameof(closedListModelUpdateObject));

            await SetVersionAsync();
            await AuthoringClient.Model.UpdateClosedListAsync(AppId, Version, clEntityId, closedListModelUpdateObject, cancellationToken);

            // TODO: LUIS SDK v3-preview has a bug in UpdateClosedListAsync, which will always cause an empty response. Investigate solution
            return LuisHelper.SuccessOperation();
        }

        public virtual async Task<OperationStatus> PatchClosedListAsync(
            Guid clEntityId,
            ClosedListModelPatchObject closedListModelPatchObject,
            CancellationToken cancellationToken = default)
        {
            await SetVersionAsync();
            return await AuthoringClient.Model.PatchClosedListAsync(AppId, Version, clEntityId, closedListModelPatchObject, cancellationToken);
        }

        public void Dispose()
        {
            AuthoringClient?.Dispose();
        }
    }
}
