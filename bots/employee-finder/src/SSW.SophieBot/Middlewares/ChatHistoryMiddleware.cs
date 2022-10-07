using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureGems.CosmosDB;
using AzureGems.Repository.Abstractions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using SSW.SophieBot.CosmosClient;

public class ChatHistoryModel : BaseEntity
{
	public string UserCommand { get; set; } = null;
	public string UserName { get; set; } = null;
	public List<object> BotResponse { get; set; } = new List<object>();
	public DateTime Time { get; set; }
	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}
}

namespace SSW.SophieBot.Middlewares
{
	public class ChatHistoryMiddleware : IMiddleware
	{
		private readonly IChatHistoryService _chatHistoryService;

		public ChatHistoryMiddleware(IChatHistoryService chatHistoryService) {
			_chatHistoryService = chatHistoryService;
		}

		public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
		{
			ChatHistoryModel history = new ChatHistoryModel();

			// TODO: Only capture Teams history
			// if (string.Equals(Channels.Msteams, turnContext.Activity.ChannelId, StringComparison.OrdinalIgnoreCase))
			// {

			if (turnContext?.Activity?.Text != null
			&& turnContext?.Activity?.Text != string.Empty
			&& turnContext?.Activity?.Text != "")
			{
				history.Id = Guid.NewGuid().ToString();
				history.UserCommand = turnContext?.Activity?.Text;
				history.UserName = turnContext.Activity.From.Name;
				history.Time = DateTime.Now;
			}

			turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
			{
				foreach (var item in activities)
				{
					if (item.Type != ActivityTypes.Typing && item.Type == ActivityTypes.Message && item.Attachments != null)
					{
						foreach (var i in item.Attachments)
						{
							history.BotResponse.Add(i.Content);
						}
						if (history.UserCommand != null)
						{
							//Save history to the database
							//Todo: this is only storing 0 index of list from BotResponse var, need to store whole list
							await _chatHistoryService.SaveHistory(history);
						}
					}
				}
				return await nextSend();
			});
			// }
			await next(cancellationToken);
		}
	}
}

