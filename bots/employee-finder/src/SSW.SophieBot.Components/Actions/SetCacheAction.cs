using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSW.SophieBot.Components.Services;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class SetCacheAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "SetCacheAction";

        [JsonConstructor]
        public SetCacheAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("cacheKey")]
        public StringExpression CacheKey { get; set; }

        [JsonProperty("cacheData")]
        public StringExpression CacheData { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var cacheKey = dc.GetValue(CacheKey);
            var cacheData = dc.GetValue(CacheData);

            var cacheSettings = dc.Services.Get<IOptions<CacheSettings>>().Value;
            var redisConnection = await RedisService.InitializeAsync(cacheSettings.ConnectionString);

            await redisConnection.BasicRetryAsync(async db => await db.StringSetAsync(cacheKey, cacheData, TimeSpan.FromMinutes(10)));

            return await dc.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
