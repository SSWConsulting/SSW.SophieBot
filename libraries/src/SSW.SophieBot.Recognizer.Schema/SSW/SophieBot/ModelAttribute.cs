using System;
using System.Linq;

namespace SSW.SophieBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ModelAttribute : Attribute, IModelMetaDataProvider
    {
        public string Name { get; }

        public ModelAttribute(string name)
        {
            Name = name;
        }

        public static string GetName(Type modelType)
        {
            Check.NotNull(modelType, nameof(modelType));

            return modelType
                .GetCustomAttributes(true)
                .OfType<IModelMetaDataProvider>()
                .FirstOrDefault()
                ?.Name
            ?? modelType.Name;
        }
    }
}
