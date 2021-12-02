using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public interface IPersistenceMigrator<TClient, TOptions>
    {
        Task<TClient> MigrateAsync(TOptions options, CancellationToken cancellationToken = default);
    }
}
