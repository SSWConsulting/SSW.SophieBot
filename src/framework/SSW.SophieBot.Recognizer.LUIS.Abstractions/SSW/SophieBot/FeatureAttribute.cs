using System;
using System.Collections.Generic;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class FeatureAttribute : Attribute, IModelDependency
    {
        // TODO: allow multiple
        public Type FeatureType { get; }

        public bool IsModel { get; }

        public bool IsRequired { get; }

        public FeatureAttribute(Type type, bool isModel = true, bool isRequired = false)
        {
            Check.NotNull(type, nameof(type));
            if (!typeof(IRecognizerModel).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Feature type should implement {typeof(IRecognizerModel).FullName}, but is {type.FullName}");
            }

            FeatureType = type;
            IsModel = isModel;
            IsRequired = isRequired;
        }

        public string GetFeatureName()
        {
            return ModelAttribute.GetName(FeatureType);
        }

        public List<Type> GetDependencies()
        {
            var dependencies = new List<Type>();
            if (FeatureType != null)
            {
                dependencies.Add(FeatureType);
            }

            return dependencies;
        }
    }
}
