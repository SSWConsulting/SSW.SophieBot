using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using SSW.SophieBot.Components.Models;
using SSW.SophieBot.Components.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class SophieBotLuisAdaptiveRecognizer : LuisAdaptiveRecognizer
    {
        public SophieBotLuisAdaptiveRecognizer()
            : base()
        {
        }

        public override async Task<RecognizerResult> RecognizeAsync(
            DialogContext dialogContext,
            Activity activity,
            CancellationToken cancellationToken = default,
            Dictionary<string, string> telemetryProperties = null,
            Dictionary<string, double> telemetryMetrics = null)
        {
            var result = await base.RecognizeAsync(dialogContext, activity, cancellationToken, telemetryProperties, telemetryMetrics);
            var cacheSettings = dialogContext.Services.Get<IOptions<CacheSettings>>().Value;

            var key = RedisService.GetRecognizerResultKey(result);
            var cacheState = new CacheState
            {
                Key = key,
            };
            dialogContext.State.SetValue(cacheSettings.CacheStatePath, cacheState);

            if (result.GetTopScoringIntent().intent == "GetPeopleBySkills")
            {
                var redisConnection = await RedisService.InitializeAsync(cacheSettings.ConnectionString);

                var cacheResult = await redisConnection.BasicRetryAsync(async db => await db.StringGetAsync(key));

                if (!cacheResult.IsNull)
                {
                    cacheState.Data = cacheResult.ToString();
                    dialogContext.State.SetValue(cacheSettings.CacheStatePath, cacheResult.ToString());
                }
            }

            return result;
        }
    }
}
