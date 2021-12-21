using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TestModelDependencyAttribute : Attribute, IModelDependency
    {
        private readonly List<Type> _dependencies = new List<Type>();
        public IReadOnlyCollection<Type> Dependencies => _dependencies.ToImmutableHashSet();

        public TestModelDependencyAttribute(params Type[] dependencies)
        {
            _dependencies.AddRange(Check.NotNullOrEmpty(dependencies, nameof(dependencies)));
        }

        public List<Type> GetDependencies()
        {
            return _dependencies;
        }
    }
}
