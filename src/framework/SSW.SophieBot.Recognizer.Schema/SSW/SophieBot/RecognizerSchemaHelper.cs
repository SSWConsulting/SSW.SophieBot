using SSW.SophieBot.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SSW.SophieBot
{
    public static class RecognizerSchemaHelper
    {
        public static ISet<Type> GetAllModelTypes<T>()
        {
            return GetAllModelTypes(typeof(T));
        }

        public static ISet<Type> GetAllModelTypes(Type schemaType)
        {
            Check.NotNull(schemaType, nameof(schemaType));

            var schemaTypes = GetInheritedSchemaTypes(schemaType).Append(schemaType);
            var schemaAssemblies = schemaTypes.Select(x => x.Assembly).Distinct();
            var modelTypes = schemaAssemblies
                .SelectMany(assembly => EnumerateModelTypesFromAssembly(assembly))
                .Distinct();

            return modelTypes.ToHashSet();
        }

        public static ISet<Type> GetInheritedSchemaTypes<T>()
        {
            return GetInheritedSchemaTypes(typeof(T));
        }

        public static ISet<Type> GetInheritedSchemaTypes(Type type)
        {
            var typeList = new List<Type>();

            var baseSchemaTypes = SchemaAttribute.GetBaseSchemaTypes(type);
            if (!baseSchemaTypes.Any())
            {
                return typeList.ToHashSet();
            }

            typeList.AddRange(baseSchemaTypes);

            foreach (var baseSchemaType in baseSchemaTypes)
            {
                var recursiveBaseSchemaTypes = GetInheritedSchemaTypes(baseSchemaType);
                typeList.AddRange(recursiveBaseSchemaTypes);
            }

            return typeList.ToHashSet();
        }

        private static IEnumerable<Type> EnumerateModelTypesFromAssembly(Assembly assembly)
        {
            return AssemblyHelper.GetAllTypes(assembly)
                .Where(
                    type => type != null &&
                        type.IsClass &&
                        !type.IsAbstract &&
                        !type.IsGenericType &&
                        typeof(IRecognizerModel).IsAssignableFrom(type)
                );
        }
    }
}
