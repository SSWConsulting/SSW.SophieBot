using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using SSW.SophieBot.ClosedListEntity;
using SSW.SophieBot.LUIS.Migrator.HttpClients;
using System.Reflection;

namespace SSW.SophieBot.LUIS.Migrator
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.WithProperty("ApplicationName", Assembly.GetExecutingAssembly().GetName().Name)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} ({SourceContext})] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            await CreateHostBuilder(args).RunConsoleAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(builder => builder.AddSerilog());

                    services.AddHttpClient();
                    services.AddTransient<IPeopleClient, PeopleClient>();

                    services.Configure<PeopleApiOptions>(hostContext.Configuration.GetSection("PeopleApi"));

                    services.AddLuis(hostContext.Configuration);
                    services.AddEntities<EntityBase>();

                    services.AddHostedService<LuisMigratorHostedService>();
                });
    }
}