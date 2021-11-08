using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SSWSophieBot.settings;

namespace SSWSophieBot.Integration
{
	public class SSWSophieBotTelemetryInitializer : ITelemetryInitializer
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ApplicationSettings _applicationSettings;

		public SSWSophieBotTelemetryInitializer(IHttpContextAccessor httpContextAccessor, IOptions<ApplicationSettings> options)
		{
			_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
			_applicationSettings = options.Value;
		}

		public void Initialize(ITelemetry telemetry)
		{
			var httpContext = _httpContextAccessor.HttpContext;
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
