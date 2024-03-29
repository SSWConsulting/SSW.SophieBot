﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace SSW.SophieBot
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((hostingContext, builder) =>
				{
					var environmentName = hostingContext.HostingEnvironment.EnvironmentName;
					var applicationRoot = AppDomain.CurrentDomain.BaseDirectory;
					var settingsDirectory = "settings";

					builder.AddBotRuntimeConfiguration(applicationRoot, settingsDirectory, environmentName);

					builder.AddCommandLine(args);
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
		}
	}
}
