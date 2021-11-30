﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SSW.SophieBot.AzureFunction.DependencyInjection
{
    public static class FunctionConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddAppsettings(this IFunctionsConfigurationBuilder builder)
        {
            var context = builder.GetContext();

            return builder.ConfigurationBuilder
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
                .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();
        }
    }
}
