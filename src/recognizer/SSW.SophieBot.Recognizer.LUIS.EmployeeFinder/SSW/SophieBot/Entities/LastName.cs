using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SSW.SophieBot.Entities
{
    [ChildOf(typeof(Contact))]
    [Feature(typeof(SswPersonNames))]
    [Feature("personName")]
    public class LastName : IEntity
    {
        public ICollection<Type> Children { get; } = new List<Type>();

        public async IAsyncEnumerable<bool> SeedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return true;
        }
    }
}
