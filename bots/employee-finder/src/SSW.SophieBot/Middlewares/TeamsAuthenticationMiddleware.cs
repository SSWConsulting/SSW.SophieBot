using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Extensions.Options;
using SSW.SophieBot.settings;

namespace SSW.SophieBot.Middlewares
{
	public class TeamsAuthenticationMiddleware : IMiddleware
	{
		private readonly UserState _userState;
		private readonly ApplicationSettings _applicationSettings;

		public TeamsAuthenticationMiddleware(UserState userState, IOptions<ApplicationSettings> options)
		{
			_userState = userState;
			_applicationSettings = options.Value;
		}

		public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
		{
			var channelId = turnContext.Activity.ChannelId;

			// Only Teams and the local Emulator may reach the bot, so channels like Direct Line can't pull employee data
			if (!string.Equals(Channels.Msteams, channelId, StringComparison.OrdinalIgnoreCase)
				&& !string.Equals(Channels.Emulator, channelId, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			if (string.Equals(Channels.Msteams, channelId, StringComparison.OrdinalIgnoreCase))
			{
				var tenantAuthenticated = false;

				var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);

				var tenantAuthenticatedProperty = _userState.CreateProperty<bool>("TenantAuthenticated");
				var identityProperty = _userState.CreateProperty<object>("Identity");

				var tenantId = _applicationSettings.TenantId;
				if (!string.IsNullOrEmpty(tenantId))
				{
					if (member.TenantId == tenantId)
					{
						tenantAuthenticated = true;
					}
				}

				await tenantAuthenticatedProperty.SetAsync(turnContext, tenantAuthenticated);
				await identityProperty.SetAsync(turnContext, member);
			}

			await next(cancellationToken);
		}
	}
}
