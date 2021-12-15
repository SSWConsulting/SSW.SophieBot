using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace SSW.SophieBot
{
    public class RecognizerSchemaOptions
    {
        private readonly List<Type> _modelTypes;
        public IReadOnlyList<Type> ModelTypes => _modelTypes.ToImmutableList();

        public List<Type> OfModelType<T>()
            where T : IRecognizerModel
        {
            return ModelTypes.Where(modelType => typeof(T).IsAssignableFrom(modelType)).ToList();
        }

        public void AddModelTypes(IEnumerable<Type> modelTypes)
        {
            _modelTypes.AddRange(modelTypes);
        }
    }
}
