using System;
using System.Linq;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ChildOfAttribute : Attribute, IModelDependency
    {
        public Type Dependency { get; }

        public ChildOfAttribute(Type parentEntity)
        {
            Check.NotNull(parentEntity, nameof(parentEntity));

            if (!typeof(IEntity).IsAssignableFrom(parentEntity))
            {
                throw new ArgumentException($"Parent type should implement {typeof(IEntity).FullName}, but is {parentEntity.FullName}");
            }

            Dependency = parentEntity;
        }

        public static Type GetParentOrDefault(Type entityType)
        {
            return entityType.GetCustomAttributes(true)
                .OfType<ChildOfAttribute>()
                .FirstOrDefault()
                ?.Dependency;
        }
    }
}
