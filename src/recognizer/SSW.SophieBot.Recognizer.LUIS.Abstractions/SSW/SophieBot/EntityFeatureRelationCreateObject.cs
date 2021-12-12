namespace SSW.SophieBot
{
    public class EntityFeatureRelationCreateObject : EntityFeatureRelationModel
    {
        public EntityFeatureRelationCreateObject() : base()
        {

        }

        public EntityFeatureRelationCreateObject(string modelName, string featureName, bool isRequired = false)
            : base(modelName, featureName, isRequired)
        {

        }

        public static EntityFeatureRelationCreateObject FromModel(EntityFeatureRelationModel model)
        {
            return new EntityFeatureRelationCreateObject(model.ModelName, model.FeatureName, model.IsRequired);
        }
    }
}
