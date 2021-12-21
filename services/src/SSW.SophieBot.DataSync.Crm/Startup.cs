using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SSW.SophieBot.DataSync.Crm.Config;
using SSW.SophieBot.DataSync.Crm.HttpClients;
using SSW.SophieBot.DataSync.Crm.Persistence;
using SSW.SophieBot.DataSync.Crm.Sync;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.Sync;

[assembly: FunctionsStartup(typeof(SSW.SophieBot.DataSync.Crm.Startup))]
namespace SSW.SophieBot.DataSync.Crm
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddSerilog();

            ConfigureSyncServices(builder);
            ConfigureSyncFunctions(builder);
            ConfigureAzureServices(builder);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.AddAppsettings();
        }

        private static void ConfigureSyncServices(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<CrmOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    configuration.GetSection("Crm").Bind(options);
                });

            builder.Services.AddHttpClient<AuthClient>();
            builder.Services.AddHttpClient<CrmClient>();

            builder.Services.AddDefaultDataSync();

            builder.Services.AddTransient<IPersistenceMigrator<Container, SyncFunctionOptions>, CosmosMigrator>();
            builder.Services.AddTransient<IPagedSyncService<CrmEmployee>, EmployeeOdataService>();
            builder.Services.AddTransient<ITransactionalBulkRepository<SyncSnapshot, PatchOperation>, SyncSnapshotRepository>();
        }

        private static void ConfigureSyncFunctions(IFunctionsHostBuilder builder)
        {
            builder.Services.AddOptions<SyncOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    options.OrganizationId = configuration["OrganizationId"];
                    configuration.GetSection("EmployeeSync").Bind(options.EmployeeSync);
                });
        }

        private static void ConfigureAzureServices(IFunctionsHostBuilder builder)
        {
            var serviceBusConString = builder.GetContext().Configuration.GetConnectionString("ServiceBus");
            var cosmosConString = builder.GetContext().Configuration.GetConnectionString("CosmosDb");

            builder.Services.AddAzureClients(builder =>
            {
                builder.AddClient((SyncOptions _) => new CosmosClientBuilder(cosmosConString)
                    .WithSerializerOptions(new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    }).Build()
                );
                builder.AddServiceBusClient(serviceBusConString);
            });

            builder.Services.AddTransient<IBatchMessageService<MqMessage<Employee>, string>, ServiceBusMessageService>();
        }
    }
}
