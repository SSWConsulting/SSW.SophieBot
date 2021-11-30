using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Serilog;
using SSW.SophieBot.AzureFunction.DependencyInjection;

[assembly: FunctionsStartup(typeof(SSW.SophieBot.CrmDataSync.Startup))]
namespace SSW.SophieBot.CrmDataSync
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddSerilog();
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.AddAppsettings();
        }
    }
}
