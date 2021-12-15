using System;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class TestModelDependencyAttribute : Attribute, IModelDependency
    {
        public Type Dependency { get; }

        public TestModelDependencyAttribute(Type dependency)
        {
            Dependency = Check.NotNull(dependency, nameof(dependency));
        }
    }
}
