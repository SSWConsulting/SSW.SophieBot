using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class SchemaAttribute : Attribute
    {
        public Type[] SchemaTypes { get; }


        // TODO: bool Inclusive

        public SchemaAttribute(params Type[] schemaTypes)
        {
            SchemaTypes = schemaTypes ?? new Type[0];
        }

        public static ISet<Type> GetBaseSchemaTypes(Type type)
        {
            Check.NotNull(type, nameof(type));

            return type.GetCustomAttributes(true)
                .OfType<SchemaAttribute>()
                .SelectMany(attribute => attribute.SchemaTypes)
                .Distinct()
                .ToHashSet();
        }
    }
}
