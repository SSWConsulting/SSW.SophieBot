using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using System;
using System.IO;

namespace Microsoft.Extensions.Configuration
{
    public static class BotConfigurationBuilderExtensions
    {
        public static IConfigurationBuilder AddBotConfiguration(this IConfigurationBuilder builder, string environmentName)
        {
            var applicationRoot = AppDomain.CurrentDomain.BaseDirectory;
            var settingsDirectory = "settings";

            builder.AddBotRuntimeConfiguration(applicationRoot, settingsDirectory, environmentName);

            builder.AddJsonFile(Path.Combine(settingsDirectory, "botsettings.json"), true, true);
            builder.AddJsonFile(Path.Combine(settingsDirectory, $"botsettings.{environmentName}.json"), true, true);

            return builder;
        }
    }
}
