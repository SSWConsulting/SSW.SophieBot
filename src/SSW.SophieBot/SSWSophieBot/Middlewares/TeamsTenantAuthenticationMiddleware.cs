using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Options;
using SSWSophieBot.settings;

namespace SSWSophieBot.Middlewares
{
	public class TeamsTenantAuthenticationMiddleware : IMiddleware
	{
		private readonly UserState _userState;
		private readonly ApplicationSettings _applicationSettings;

		public TeamsTenantAuthenticationMiddleware(UserState userState, IOptions<ApplicationSettings> options)
		{
			_userState = userState;
			_applicationSettings = options.Value;
		}

		public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
		{
			if (string.Equals(Channels.Msteams, turnContext.Activity.ChannelId, StringComparison.OrdinalIgnoreCase))
			{
				var authenticated = false;
				var property = _userState.CreateProperty<bool>("TenantAuthenticated");

				var tenantId = _applicationSettings.TenantId;
				if (!string.IsNullOrEmpty(tenantId))
				{
					var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
					if (member.TenantId == tenantId)
					{
						authenticated = true;
					}
				}

				await property.SetAsync(turnContext, authenticated);
			}

			await next(cancellationToken);
		}
	}
}
