using System;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Domain.Persistence
{
    public interface IPersistenceMigrator<TClient, TOptions>
    {
        Task<TClient> MigrateAsync(TOptions options, CancellationToken cancellationToken = default);
    }
}
