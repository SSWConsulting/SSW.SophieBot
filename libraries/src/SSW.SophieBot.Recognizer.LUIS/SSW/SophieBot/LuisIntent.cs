using System;
using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot
{
    public class LuisIntent : IIntent<LuisExample>
    {
        public ICollection<LuisExample> Examples => throw new NotImplementedException();

        public IAsyncEnumerable<bool> SeedAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
