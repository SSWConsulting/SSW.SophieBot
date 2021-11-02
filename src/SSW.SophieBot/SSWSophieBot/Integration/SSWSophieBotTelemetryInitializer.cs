using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using SSWSophieBot.settings;

namespace SSWSophieBot.Integration
{
	public class SSWSophieBotTelemetryInitializer : ITelemetryInitializer
	{
		private readonly ApplicationSettings _applicationSettings;

		public SSWSophieBotTelemetryInitializer(IOptions<ApplicationSettings> options)
		{
			_applicationSettings = options.Value;
		}

		public void Initialize(ITelemetry telemetry)
		{
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
