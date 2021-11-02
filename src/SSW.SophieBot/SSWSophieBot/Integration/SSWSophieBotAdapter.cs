using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SSWSophieBot.Integration
{
	public class SSWSophieBotAdapter : CloudAdapter
	{
		protected IBotTelemetryClient AdapterBotTelemetryClient { get; }

		public SSWSophieBotAdapter(
			IConfiguration configuration,
			IHttpClientFactory httpClientFactory,
			ILogger<SSWSophieBotAdapter> logger,
			IEnumerable<IMiddleware> middlewares,
			IBotTelemetryClient botTelemetryClient)
			: base(configuration, httpClientFactory, logger)
		{
			AdapterBotTelemetryClient = botTelemetryClient;

			foreach (IMiddleware middleware in middlewares)
			{
				Use(middleware);
			}

			OnTurnError = OnTurnErrorAsync;
		}

		protected virtual async Task OnTurnErrorAsync(ITurnContext turnContext, Exception exception)
		{
			var properties = new Dictionary<string, string>
				{{"Bot exception caught in", $"{nameof(SSWSophieBotAdapter)} - {nameof(OnTurnError)}"}};

			AdapterBotTelemetryClient.TrackException(exception, properties);

			// Log any leaked exception from the application.
			// NOTE: In production environment, you should consider logging this to
			// Azure Application Insights. Visit https://aka.ms/bottelemetry to see how
			// to add telemetry capture to your bot.
			//Logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");

			var errorMessageText = "The bot encountered an error or bug.";
			var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.IgnoringInput);
			await turnContext.SendActivityAsync(errorMessage);

			errorMessageText = exception.Message;
			errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.IgnoringInput);
			await turnContext.SendActivityAsync(errorMessage);

			var conversationState = turnContext.TurnState.Get<ConversationState>();
			if (conversationState != null)
			{
				try
				{
					await conversationState.DeleteAsync(turnContext);
				}
				catch (Exception e)
				{
					Logger.LogError(e, $"Exception caught on attempting to Delete ConversationState : {e.Message}");
				}
			}

			await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message, "https://www.botframework.com/schemas/error", "TurnError");
		}
	}
}
