using System;

namespace SSW.SophieBot
{
    public class FeatureDescriptor
    {
        public Type FeatureType { get; }

        public bool IsRequired { get; }

        public string FeatureName { get; }

        public bool IsModelFeature { get; }

        public FeatureDescriptor(Type featureType, bool isRequired = false)
        {
            Check.NotNull(featureType, nameof(featureType));
            if (!typeof(IRecognizerModel).IsAssignableFrom(featureType))
            {
                throw new ArgumentException($"Feature type should implement {typeof(IRecognizerModel).FullName}, but is {featureType.FullName}");
            }

            FeatureType = featureType;
            IsRequired = isRequired;
            FeatureName = ModelAttribute.GetName(featureType);
            IsModelFeature = !typeof(IPhraseList).IsAssignableFrom(FeatureType);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (!(obj is FeatureDescriptor))
            {
                return false;
            }

            return FeatureType == ((FeatureDescriptor)obj).FeatureType;
        }

        public override int GetHashCode()
        {
            return FeatureType.GetHashCode();
        }
    }
}
