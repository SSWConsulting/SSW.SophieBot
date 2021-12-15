using System;
using System.Linq;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class FeatureAttribute : Attribute, IModelDependency
    {
        public string ModelName { get; }

        public string FeatureName { get; }

        public bool IsRequired { get; }

        public Type Dependency { get; }

        public FeatureAttribute(Type type, bool isModel = false, bool isRequired = false)
        {
            var name = ModelAttribute.GetName(Check.NotNull(type, nameof(type)));

            if (isModel)
            {
                ModelName = name;
            }
            else
            {
                FeatureName = name;
            }

            Dependency = type;
            IsRequired = isRequired;
        }

        public FeatureAttribute(string name, bool isModel = false, bool isRequired = false)
        {
            name = Check.NotNullOrWhiteSpace(name, nameof(name));

            if (isModel)
            {
                ModelName = name;
            }
            else
            {
                FeatureName = name;
            }

            IsRequired = isRequired;
        }

        public static string[] GetModelNames(Type modelType)
        {
            Check.NotNull(modelType, nameof(modelType));

            return modelType.GetCustomAttributes(true)
                .OfType<FeatureAttribute>()
                .Select(attribute => attribute.ModelName)
                .ToArray();
        }

        public static string[] GetFeatureNames(Type modelType)
        {
            Check.NotNull(modelType, nameof(modelType));

            return modelType.GetCustomAttributes(true)
                .OfType<FeatureAttribute>()
                .Select(attribute => attribute.FeatureName)
                .ToArray();
        }
    }
}
