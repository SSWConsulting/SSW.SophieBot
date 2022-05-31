using System.Linq;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SSW.SophieBot.Components;
using SSW.SophieBot.Components.Actions;
using SSW.SophieBot.HttpClientComponents.Abstractions;
using SSW.SophieBot.HttpClientComponents.PersonQuery;
using SSW.SophieBot.Integration;
using SSW.SophieBot.Middlewares;
using SSW.SophieBot.settings;

namespace SSW.SophieBot
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers().AddNewtonsoftJson();
			services.AddBotRuntime(Configuration);

			services.AddSingleton<SophieBotAdapter>();
			services.Replace(ServiceDescriptor.Singleton<IBotFrameworkHttpAdapter>(sp => sp.GetRequiredService<SophieBotAdapter>()));
			services.Replace(ServiceDescriptor.Singleton<BotAdapter>(sp => sp.GetRequiredService<SophieBotAdapter>()));

			services.Remove(services.Last(s => s.ServiceType == typeof(DeclarativeType)));
			services.AddSingleton<DeclarativeType>(sp => new DeclarativeType<SophieBotLuisAdaptiveRecognizer>(LuisAdaptiveRecognizer.Kind));

			services.AddSingleton<ITelemetryInitializer, SophieBotTelemetryInitializer>();
			services.AddSingleton<IMiddleware, TeamsAuthenticationMiddleware>();

			services.Configure<ApplicationSettings>(Configuration.GetSection(ConfigurationConstants.AppSettingsKey));
			services.Configure<AppInsightsSettings>(Configuration.GetSection(ConfigurationConstants.AppInsightsSettingsKey));
			services.Configure<CacheSettings>(Configuration.GetSection(ConfigurationConstants.CacheSettingsKey));

			services.ConfigureSophieBotHttpClient()
				.AddPersonQueryClient();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();

				if (ChannelValidation.ToBotFromChannelTokenValidationParameters != null)
				{
					ChannelValidation.ToBotFromChannelTokenValidationParameters.ValidateLifetime = false;
				}
			}

			app.UseDefaultFiles();

			// Set up custom content types - associating file extension to MIME type.
			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings[".lu"] = "application/vnd.microsoft.lu";
			provider.Mappings[".qna"] = "application/vnd.microsoft.qna";

			// Expose static files in manifests folder for skill scenarios.
			app.UseStaticFiles(new StaticFileOptions
			{
				ContentTypeProvider = provider
			});
			app.UseWebSockets();
			app.UseRouting();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
