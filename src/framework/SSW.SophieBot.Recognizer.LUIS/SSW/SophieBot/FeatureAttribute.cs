using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class FeatureAttribute : Attribute, IModelDependency
    {
        private readonly List<FeatureDescriptor> _features = new List<FeatureDescriptor>();
        public IReadOnlyCollection<FeatureDescriptor> Features => _features.ToImmutableHashSet();

        public FeatureAttribute(Type type, bool isRequired = false)
        {
            _features.Add(new FeatureDescriptor(type, isRequired));
        }

        public FeatureAttribute(params Type[] types)
        {
            Check.NotNull(types, nameof(types));

            _features.AddRange(types
                .Where(type => type != null)
                .Select(type => new FeatureDescriptor(type))
            );
        }

        public List<Type> GetDependencies()
        {
            return _features.Select(feature => feature.FeatureType).ToList();
        }

        public static List<FeatureDescriptor> GetFeatures<T>()
        {
            return GetFeatures(typeof(T));
        }

        public static List<FeatureDescriptor> GetFeatures(Type modelType)
        {
            Check.NotNull(modelType, nameof(modelType));

            return modelType.GetCustomAttributes(true)
                .OfType<FeatureAttribute>()
                .SelectMany(attribute => attribute.Features)
                .ToList();
        }
    }
}
