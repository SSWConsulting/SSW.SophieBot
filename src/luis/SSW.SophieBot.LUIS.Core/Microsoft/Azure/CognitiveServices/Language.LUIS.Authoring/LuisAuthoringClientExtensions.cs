using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    }
}
