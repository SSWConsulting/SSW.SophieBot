using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;

public class ChatHistoryModel
{
	public string UserCommand { get; set; } = null;
	public string UserName { get; set; } = null;
	public List<object> BotResponse { get; set; } = new List<object>();
	public DateTime Time { get; set; }
}

namespace SSW.SophieBot.Middlewares
{
	public class ChatHistoryMiddleware : IMiddleware
	{
		public ChatHistoryMiddleware() { }

		public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
		{
			ChatHistoryModel history = new ChatHistoryModel();

			// Only capture Teams history
			// if (string.Equals(Channels.Msteams, turnContext.Activity.ChannelId, StringComparison.OrdinalIgnoreCase))
			// {

			if (turnContext?.Activity?.Text != null
			&& turnContext?.Activity?.Text != string.Empty
			&& turnContext?.Activity?.Text != "")
			{
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
							//TODO: Save history to database
							Console.WriteLine("------------------------");
							Console.WriteLine("User Message: " + history.UserCommand);
							Console.WriteLine("User Name: " + history.UserName);
							Console.WriteLine("Time: " + history.Time);
							Console.WriteLine("Bot Message: ");
							foreach (var j in history.BotResponse)
							{
								Console.WriteLine(j);
							}
							Console.WriteLine("------------------------");
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

