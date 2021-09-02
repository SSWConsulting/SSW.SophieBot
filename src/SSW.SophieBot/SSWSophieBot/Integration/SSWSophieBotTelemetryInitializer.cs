using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using SSWSophieBot.settings;

namespace SSWSophieBot.Integration
{
	public class SSWSophieBotTelemetryInitializer : ITelemetryInitializer
	{
		private readonly IConfiguration _configuration;

		public SSWSophieBotTelemetryInitializer(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void Initialize(ITelemetry telemetry)
		{
			if (string.IsNullOrEmpty(telemetry.Context.Cloud.RoleName))
			{
				var applicationSettings = _configuration.GetSection(ConfigurationConstants.AppSettingsKey).Get<ApplicationSettings>() ?? new ApplicationSettings();

				if (!string.IsNullOrEmpty(applicationSettings.CloudRoleName))
				{
					telemetry.Context.Cloud.RoleName = applicationSettings.CloudRoleName;
				}

				if (!string.IsNullOrEmpty(applicationSettings.CloudRoleInstance))
				{
					telemetry.Context.Cloud.RoleInstance = applicationSettings.CloudRoleInstance;
				}
			}
		}
	}
}
