using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace SSW.SophieBot
{
    public class RecognizerSchemaOptions
    {
        private readonly List<Type> _modelTypes = new List<Type>();
        public IReadOnlyList<Type> ModelTypes => _modelTypes.ToImmutableList();

        public void AddModelTypes(IEnumerable<Type> modelTypes)
        {
            _modelTypes.AddRange(modelTypes);
        }
    }
}
