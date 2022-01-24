using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Reflection;

namespace Microsoft.Azure.Functions.Extensions.DependencyInjection
{
    public static class FunctionsHostBuilderExtensions
    {
        public static IServiceCollection AddSerilog(this IFunctionsHostBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Azure.Core", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient.CrmClient.ClientHandler", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient.CrmClient.LogicalHandler", LogEventLevel.Warning)
                .Enrich.WithProperty("ApplicationName", Assembly.GetExecutingAssembly().GetName().Name)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} ({SourceContext})] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            return builder.Services.AddLogging(builder => builder.AddSerilog());
        }
    }
}
