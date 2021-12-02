using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Serilog;
using SSW.SophieBot.LUIS.Core.DependencyInjection;

[assembly: FunctionsStartup(typeof(SSW.SophieBot.LUIS.Sync.Startup))]
namespace SSW.SophieBot.LUIS.Sync
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddSerilog()
                .AddLuis();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.AddAppsettings();
        }
    }
}
