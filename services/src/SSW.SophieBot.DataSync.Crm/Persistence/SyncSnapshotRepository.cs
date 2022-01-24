using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.System;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Persistence
{
    public class SyncSnapshotRepository : CosmosRepository<SyncSnapshot>
    {
        private readonly AsyncLazy<Container> _innerContainer;

        public SyncSnapshotRepository(
            IPersistenceMigrator<Container, SyncFunctionOptions> migrator,
            IOptions<SyncOptions> options,
            ILogger<CosmosRepository<SyncSnapshot>> logger)
            : base(migrator, options, logger)
        {
            _innerContainer = new AsyncLazy<Container>(async () => await Migrator.MigrateAsync(Options.EmployeeSync));
        }

        protected override async Task<Container> GetContainerAsync()
        {
            return await _innerContainer;
        }
    }
}
