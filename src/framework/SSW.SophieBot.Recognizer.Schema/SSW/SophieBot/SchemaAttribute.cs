using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class SchemaAttribute : Attribute
    {
        public Type[] BaseSchemaTypes { get; }

        public SchemaAttribute(params Type[] baseSchemaTypes)
        {
            BaseSchemaTypes = baseSchemaTypes ?? new Type[0];
        }

        public static ISet<Type> GetBaseSchemaTypes(Type type)
        {
            Check.NotNull(type, nameof(type));

            return type.GetCustomAttributes(true)
                .OfType<SchemaAttribute>()
                .SelectMany(attribute => attribute.BaseSchemaTypes)
                .Distinct()
                .ToHashSet();
        }
    }
}
