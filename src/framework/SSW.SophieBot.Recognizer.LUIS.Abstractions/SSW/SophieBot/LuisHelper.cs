using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public static class LuisHelper
    {
        public static ModelCreateObject GetEntityCreateObject<T>()
            where T : class, IEntity
        {
            return GetEntityCreateObject(typeof(T));
        }

        public static ModelCreateObject GetEntityCreateObject(Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));
            if (!typeof(IEntity).IsAssignableFrom(entityType))
            {
                throw new ArgumentException($"Given type should implement {typeof(IEntity).FullName}, but is {entityType.FullName}");
            }

            return new ModelCreateObject(ModelAttribute.GetName(entityType));
        }

        public static ClosedListModelCreateObject GetClEntityCreateObject<T>()
            where T : class, IClosedList
        {
            return GetClEntityCreateObject(typeof(T));
        }

        public static ClosedListModelCreateObject GetClEntityCreateObject(Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));
            if (!typeof(IClosedList).IsAssignableFrom(entityType))
            {
                throw new ArgumentException($"Given type should implement {typeof(IClosedList).FullName}, but is {entityType.FullName}");
            }

            return new ClosedListModelCreateObject(new List<WordListObject>(), ModelAttribute.GetName(entityType));
        }

        public static async Task<(Guid, string)> GetLuisAppIdAndActiveVersionAsync(
            ILUISAuthoringClient authoringClient,
            LuisOptions options,
            CancellationToken cancellationToken = default)
        {
            var appId = options.GetGuidAppId();
            if (appId == default)
            {
                throw new LuisException("Failed to get LUIS app ID from configuration, end execution");
            }

            var activeVersion = await authoringClient.GetActiveVersionAsync(appId, cancellationToken);

            return (appId, activeVersion);
        }

        public static List<EntityFeatureRelationModel> GetEntityFeatureRelationModels(Type entityType)
        {
            return FeatureAttribute.GetFeatures(Check.NotNull(entityType, nameof(entityType)))
                .Select(feature =>
                {
                    if (feature.IsModelFeature)
                    {
                        return new EntityFeatureRelationModel(feature.FeatureName, null, feature.IsRequired);
                    }
                    else
                    {
                        return new EntityFeatureRelationModel(null, feature.FeatureName, feature.IsRequired);
                    }
                })
                .ToList();
        }

        public static OperationStatus SuccessOperation(string message = null)
        {
            return new OperationStatus(OperationStatusType.Success, message);
        }

        public static void FailOperation(string message = null)
        {
            new OperationStatus(OperationStatusType.Failed, message).EnsureSuccessOperationStatus();
        }
    }
}
