﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[assembly: FunctionsStartup(typeof(SSW.SophieBot.LUIS.Sync.Startup))]
namespace SSW.SophieBot.LUIS.Sync
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddSerilog();

            var configuration = builder.GetContext().Configuration;
            builder.Services.AddLuis<EmployeeFinderRecognizerSchema>(configuration);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            builder.AddAppsettings();
        }
    }
}
