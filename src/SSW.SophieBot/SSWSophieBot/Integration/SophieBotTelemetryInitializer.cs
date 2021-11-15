using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SSWSophieBot.settings;

namespace SSWSophieBot.Integration
{
	public class SophieBotTelemetryInitializer : ITelemetryInitializer
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ApplicationSettings _applicationSettings;

		public SophieBotTelemetryInitializer(IServiceProvider serviceProvider, IOptions<ApplicationSettings> options)
		{
			_serviceProvider = serviceProvider;
			_applicationSettings = options.Value;
		}

		public void Initialize(ITelemetry telemetry)
		{
			using (var scope = _serviceProvider.CreateScope())
			{
				var httpContext = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
				var items = httpContext?.Items;

				if (items != null)
				{
					if ((telemetry is RequestTelemetry || telemetry is EventTelemetry
						|| telemetry is TraceTelemetry || telemetry is DependencyTelemetry || telemetry is PageViewTelemetry)
						&& items.ContainsKey(TelemetryBotIdInitializer.BotActivityKey))
					{
						if (items[TelemetryBotIdInitializer.BotActivityKey] is JObject body)
						{
							var from = body["from"];
							if (!string.IsNullOrWhiteSpace(from?.ToString()))
							{
								var userName = (string)from["name"];
								telemetry.Context.User.AccountId = userName;
							}
						}
					}
				}
			}

			if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
			{
				if (!string.IsNullOrEmpty(_applicationSettings.CloudRoleName))
				{
					telemetry.Context.Cloud.RoleName = _applicationSettings.CloudRoleName;
				}

				if (!string.IsNullOrEmpty(_applicationSettings.CloudRoleInstance))
				{
					telemetry.Context.Cloud.RoleInstance = _applicationSettings.CloudRoleInstance;
				}
			}
		}
	}
}
