using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSW.SophieBot.Components.Models;
using SSW.SophieBot.Components.Services;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class SophieBotSendActivity : SendActivity
    {
        [JsonProperty("$kind")]
        public new const string Kind = "SophieBotSendActivity";

        public SophieBotSendActivity(Activity activity, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(activity, callerPath, callerLine)
        {
            
        }

        [JsonConstructor]
        public SophieBotSendActivity(string text = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(text, callerPath, callerLine)
        {

        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var result = await base.BeginDialogAsync(dc, options, cancellationToken);

            var activity = await Activity.BindAsync(dc, dc.State);
            var activityCache = JsonConvert.SerializeObject(activity, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

            var cacheSettings = dc.Services.Get<IOptions<CacheSettings>>().Value;
            var cache = dc.GetValue(new ObjectExpression<CacheState>(cacheSettings.CacheStatePath));

            if (!string.IsNullOrEmpty(cache?.Key))
            {
                var redisConnection = await RedisService.InitializeAsync(cacheSettings.ConnectionString);
                await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync(cache.Key, activityCache, TimeSpan.FromMinutes(10)));
            }

            return result;
        }
    }
}
