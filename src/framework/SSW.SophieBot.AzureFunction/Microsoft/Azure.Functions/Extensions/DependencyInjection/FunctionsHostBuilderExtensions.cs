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
                .Enrich.WithProperty("ApplicationName", Assembly.GetExecutingAssembly().GetName().Name)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            return builder.Services.AddLogging(builder => builder.AddSerilog());
        }
    }
}
