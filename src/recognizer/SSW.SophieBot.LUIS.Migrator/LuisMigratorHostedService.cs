using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SSW.SophieBot.LUIS.Migrator
{
    public class LuisMigratorHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IRecognizerSchemaManager _recognizerSchemaManager;
        private readonly ILogger<LuisMigratorHostedService> _logger;

        public LuisMigratorHostedService(
            IHostApplicationLifetime hostApplicationLifetime,
            IRecognizerSchemaManager recognizerSchemaManager,
            ILogger<LuisMigratorHostedService> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _recognizerSchemaManager = recognizerSchemaManager;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _recognizerSchemaManager.SeedAsync(cancellationToken);
            _hostApplicationLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
