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
        public async IAsyncEnumerable<bool> SeedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return true;
        }
    }
}
