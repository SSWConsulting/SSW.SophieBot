using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.Persistence;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Persistence
{
    public class SyncSnapshotRepository : CosmosRepository<SyncSnapshot>
    {
        public SyncSnapshotRepository(
            IPersistenceMigrator<Container, SyncFunctionOptions> migrator,
            IOptions<SyncOptions> options,
            ILogger<CosmosRepository<SyncSnapshot>> logger)
            : base(migrator, options, logger)
        {

        }

        protected override async Task<Container> GetContainerAsync(CancellationToken cancellationToken = default)
        {
            return await Migrator.MigrateAsync(Options.EmployeeSync, cancellationToken);
        }
    }
}
