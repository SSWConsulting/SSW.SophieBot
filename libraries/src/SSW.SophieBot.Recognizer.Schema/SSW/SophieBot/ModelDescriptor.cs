using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SSW.SophieBot
{
    public class ModelDescriptor
    {
        public Type ModelType { get; }

        private readonly List<ModelDescriptor> _dependencies;
        public IReadOnlyList<ModelDescriptor> Dependencies => _dependencies.ToImmutableList();

        public ModelDescriptor(Type modelType)
        {
            Check.NotNull(modelType, nameof(modelType));
            if (!typeof(IRecognizerModel).IsAssignableFrom(modelType))
            {
                throw new ArgumentException($"Model type should implement {typeof(IRecognizerModel).FullName}, but is {modelType.FullName}");
            }

            ModelType = modelType;
            _dependencies = new List<ModelDescriptor>();
        }

        public void AddDependency(ModelDescriptor descriptor)
        {
            _dependencies.AddIfNotContains(descriptor);
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

            if (!(obj is ModelDescriptor))
            {
                return false;
            }

            return ModelType == ((ModelDescriptor)obj).ModelType;
        }

        public override int GetHashCode()
        {
            return ModelType.GetHashCode();
        }
    }
}
