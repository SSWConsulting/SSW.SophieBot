using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public interface ILuisService : IDisposable
    {
        Guid AppId { get; }

        string Version { get; }

        Task<string> GetActiveVersionAsync(CancellationToken cancellationToken = default);
        Task<ApplicationInfoResponse> GetAppInfoAsync(CancellationToken cancellationToken = default);
        Task<Guid?> GetEntityIdAsync(string entityName, CancellationToken cancellationToken = default);
        IAsyncEnumerable<IEnumerable<EntityExtractor>> GetAsyncPagedEntities(int pageSize = 100, CancellationToken cancellationToken = default);
        Task<IList<EntityExtractor>> ListEntitiesAsync(int? skip = 0, int? take = 100, CancellationToken cancellationToken = default);
        Task<Guid?> GetClEntityIdAsync(string clName, CancellationToken cancellationToken = default);
        IAsyncEnumerable<IEnumerable<ClosedListEntityExtractor>> GetAsyncPagedClEntities(int pageSize = 100, CancellationToken cancellationToken = default);
        Task<IList<ClosedListEntityExtractor>> ListClosedListsAsync(int? skip = 0, int? take = 100, CancellationToken cancellationToken = default);
        Task<Guid> EnsureEntityExistAsync(ModelCreateObject createObject, CancellationToken cancellationToken = default);
        Task<OperationStatus> DeleteEntityAsync(Guid entityId, CancellationToken cancellationToken = default);
        Task<Guid> AddEntityAsync(ModelCreateObject modelCreateObject, CancellationToken cancellationToken = default);
        Task<Guid> EnsureClEntityExistAsync(ClosedListModelCreateObject createObject, CancellationToken cancellationToken = default);
        Task<OperationStatus> DeleteClosedListAsync(Guid clEntityId, CancellationToken cancellationToken = default);
        Task<Guid> AddClosedListAsync(ClosedListModelCreateObject closedListModelCreateObject, CancellationToken cancellationToken = default);
        Task<PrebuiltEntityExtractor> EnsurePrebuiltEntityExistAsync(IList<string> prebuiltEntityNames, CancellationToken cancellationToken = default);
        Task<IList<PrebuiltEntityExtractor>> ListPrebuiltsAsync(int? skip = 0, int? take = 100, CancellationToken cancellationToken = default);
        Task<IList<PrebuiltEntityExtractor>> AddPrebuiltAsync(IList<string> prebuiltExtractorNames, CancellationToken cancellationToken = default);
        Task<OperationStatus> UpdateEntityFeatureRelationAsync(Guid entityId, IList<EntityFeatureRelationCreateObject> createObjects, CancellationToken cancellationToken = default);
        Task<Guid> EnsureChildEntityExistAsync(Guid parentEntityId, string childEntityName, CancellationToken cancellationToken = default);
        Task<Guid> CreateChildEntityAsync(Guid parentEntityId, string childEntityName, CancellationToken cancellationToken = default);
        Task SetVersionAsync(string version = null, CancellationToken cancellationToken = default);
        Task<bool> TrainAndPublishAppAsync(CancellationToken cancellationToken = default);
        Task<ClosedListEntityExtractor> FindClosedListAsync(string clEntityName, CancellationToken cancellationToken = default);
        Task<OperationStatus> UpdateClosedListAsync(Guid clEntityId, ClosedListModelUpdateObject closedListModelUpdateObject, CancellationToken cancellationToken = default);
        Task<OperationStatus> PatchClosedListAsync(Guid clEntityId, ClosedListModelPatchObject closedListModelPatchObject, CancellationToken cancellationToken = default);
    }
}
