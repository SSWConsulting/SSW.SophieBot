using System.Text.Json.Serialization;

namespace SSW.SophieBot
{
    public class EntityFeatureRelationModel
    {
        [JsonPropertyName("featureName")]
        public string FeatureName { get; set; } = string.Empty;

        [JsonPropertyName("modelName")]
        public string ModelName { get; set; } = string.Empty;

        [JsonPropertyName("isRequired")]
        public bool IsRequired { get; set; }

        public EntityFeatureRelationModel()
        {

        }

        public EntityFeatureRelationModel(string modelName, string featureName, bool isRequired = false)
        {
            ModelName = modelName ?? string.Empty;
            FeatureName = featureName ?? string.Empty;
            IsRequired = isRequired;
        }
    }
}
