using System;
using System.Linq;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ChildOfAttribute : Attribute
    {
        public Type ParentEntity { get; }

        public ChildOfAttribute(Type parentEntity)
        {
            ParentEntity = Check.NotNull(parentEntity, nameof(parentEntity));
        }

        public static Type GetParentOrDefault(Type entityType)
        {
            return entityType.GetCustomAttributes(true)
                .OfType<ChildOfAttribute>()
                .FirstOrDefault()
                ?.ParentEntity;
        }
    }
}
