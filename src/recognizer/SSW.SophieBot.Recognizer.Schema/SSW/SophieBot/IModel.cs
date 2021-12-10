using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot
{
    public interface IModel
    {
        IAsyncEnumerable<bool> SeedAsync(CancellationToken cancellationToken = default);
    }
}
