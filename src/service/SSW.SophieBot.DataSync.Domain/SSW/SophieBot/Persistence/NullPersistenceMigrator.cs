using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public sealed class NullPersistenceMigrator<TClient, TOptions> : IPersistenceMigrator<TClient, TOptions>
    {
        public Task<TClient> MigrateAsync(TOptions options, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TClient));
        }
    }
}
