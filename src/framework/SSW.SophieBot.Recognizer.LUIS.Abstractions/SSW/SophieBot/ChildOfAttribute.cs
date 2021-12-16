using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ChildOfAttribute : Attribute, IModelDependency
    {
        public Type Parent { get; }

        public ChildOfAttribute(Type parentEntity)
        {
            Check.NotNull(parentEntity, nameof(parentEntity));

            if (!typeof(IEntity).IsAssignableFrom(parentEntity))
            {
                throw new ArgumentException($"Parent type should implement {typeof(IEntity).FullName}, but is {parentEntity.FullName}");
            }

            Parent = parentEntity;
        }

        public static Type GetParentOrDefault(Type entityType)
        {
            return entityType.GetCustomAttributes(true)
                .OfType<ChildOfAttribute>()
                .FirstOrDefault()
                ?.Parent;
        }

        public List<Type> GetDependencies()
        {
            return new List<Type> { Parent };
        }
    }
}
