using System.IO;
using AzureGems.CosmosDB;
using AzureGems.Repository.CosmosDB;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Runtime.Extensions;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SSW.SophieBot.Components;
using SSW.SophieBot.CosmosClient;
using SSW.SophieBot.Components.Services;
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

			services.AddSingleton<ITelemetryInitializer, SophieBotTelemetryInitializer>();
			services.AddSingleton<IMiddleware, TeamsAuthenticationMiddleware>();
			services.AddSingleton<IMiddleware, ChatHistoryMiddleware>();
			services.AddSingleton<IChatHistoryService, ChatHistoryService>();

			services.Configure<ApplicationSettings>(Configuration.GetSection(ConfigurationConstants.AppSettingsKey));
			services.Configure<AppInsightsSettings>(Configuration.GetSection(ConfigurationConstants.AppInsightsSettingsKey));
			services.Configure<AppInsightsSettings>(Configuration.GetSection(ConfigurationConstants.CosmosDbConnection));

			services.ConfigureSophieBotHttpClient()
				.AddPersonQueryClient();

			services.AddCosmosDb(builder =>
			{
				builder
					.Connect(endPoint: Configuration.GetSection("CosmosDbConnection:Endpoint").Value,
									authKey: Configuration.GetSection("CosmosDbConnection:Authkey").Value)
					.UseDatabase(databaseId: Configuration.GetSection("CosmosDbConnection:DatabaseId").Value)
					.WithSharedThroughput(400)
					.WithContainerConfig(c =>
					{
						c.AddContainer<ChatHistoryModel>(containerId: "ChatHistoryContainer", partitionKeyPath: "/id", queryByDiscriminator: false, throughput: 20000);
					});
			});
			services.AddCosmosContext<ChatHistoryCosmosContext>();
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
