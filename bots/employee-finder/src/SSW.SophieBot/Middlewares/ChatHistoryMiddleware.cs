using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

public class Conversation
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
		public ChatHistoryMiddleware()
		{
		}

		public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
		{
			Conversation conversation = new Conversation();

			//TODO: Only capture MS Teams history
			// if (string.Equals(Channels.Msteams, turnContext.Activity.ChannelId, StringComparison.OrdinalIgnoreCase))
			// {
			// }

			if (turnContext?.Activity?.Text != null
			&& turnContext?.Activity?.Text != string.Empty
			&& turnContext?.Activity?.Text != "")
			{
				conversation.UserCommand = turnContext?.Activity?.Text;
				conversation.UserName = turnContext.Activity.From.Name;
				conversation.Time = DateTime.Now;
			}

			turnContext.OnSendActivities(async (ctx, activities, nextSend) =>
			{
				foreach (var item in activities)
				{
					if (item.Type != ActivityTypes.Typing && item.Type == ActivityTypes.Message && conversation.UserCommand != null)
					{
						foreach (var i in item.Attachments)
						{
							conversation.BotResponse.Add(i.Content);
						}
					}
				}
				return await nextSend();
			});

			if (conversation.UserCommand != null)
			{
				Console.WriteLine("------------------------");
				Console.WriteLine("User Message: " + conversation.UserCommand);
				Console.WriteLine("User Name: " + conversation.UserName);
				Console.WriteLine("Time: " + conversation.Time);
				Console.WriteLine("Bot Message: ");
				foreach (var j in conversation.BotResponse)
				{
					Console.WriteLine(j);
				}
				Console.WriteLine("------------------------");
			}

			await next(cancellationToken);
		}
	}
}

