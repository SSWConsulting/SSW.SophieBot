using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot
{
    public class TestModelDependency : IRecognizerModel
    {
        public async IAsyncEnumerable<bool> SeedAsync(CancellationToken cancellationToken = default)
        {
            yield return true;
        }
    }
}
