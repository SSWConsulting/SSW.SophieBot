using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot
{
    public class RecognizerSchemaOptions
    {
        public List<Type> ModelTypes { get; } = new List<Type>();

        public List<Type> OfModelType<T>()
            where T : IRecognizerModel
        {
            return ModelTypes.Where(modelType => typeof(T).IsAssignableFrom(modelType)).ToList();
        }
    }
}
