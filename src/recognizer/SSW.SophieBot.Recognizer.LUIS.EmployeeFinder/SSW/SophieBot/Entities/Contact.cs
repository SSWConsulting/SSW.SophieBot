using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SSW.SophieBot.Entities
{
    [Feature(typeof(SswPersonNames))]
    [Feature("personName")]
    public class Contact : IEntity
    {
        public Type Parent { get; }

        public IAsyncEnumerable<bool> SeedAsync(CancellationToken cancellationToken = default)
        {
            return new List<bool>() { true }.ToAsyncEnumerable();
        }
    }
}
