using System;

namespace SSW.SophieBot
{
    public class RecognizerSchemaServiceOptions
    {
        public Type SchemaManagerType { get; private set; }

        public RecognizerSchemaServiceOptions UseManager<T>()
            where T : class, IRecognizerSchemaManager
        {
            return UseManager(typeof(T));
        }

        public RecognizerSchemaServiceOptions UseManager(Type schemaManagerType)
        {
            Check.NotNull(schemaManagerType, nameof(schemaManagerType));
            if (!typeof(IRecognizerSchemaManager).IsAssignableFrom(schemaManagerType))
            {
                throw new ArgumentException($"Schema manager should implement {typeof(IRecognizerSchemaManager).FullName}, but is {schemaManagerType.FullName}");
            }

            SchemaManagerType = schemaManagerType;
            return this;
        }
    }
}
