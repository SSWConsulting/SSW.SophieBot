using Extensions.DependencyInjection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SSW.SophieBot.Entities;

[assembly: FunctionsStartup(typeof(SSW.SophieBot.LUIS.Sync.Startup))]
namespace SSW.SophieBot.LUIS.Sync
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddSerilog()
                .AddLuis()
                .AddEntities<EntityBase>();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.AddAppsettings();
        }
    }
}
