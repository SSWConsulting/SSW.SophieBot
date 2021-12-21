using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot
{
    public interface IRecognizerModel
    {
        IAsyncEnumerable<bool> SeedAsync(CancellationToken cancellationToken = default);
    }
}
