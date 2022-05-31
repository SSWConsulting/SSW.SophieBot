using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSW.SophieBot.Components.Services;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class ReadCacheAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "ReadCacheAction";

        [JsonConstructor]
        public ReadCacheAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("cacheKey")]
        public StringExpression CacheKey { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var cacheKey = dc.GetValue(CacheKey);

            var cacheSettings = dc.Services.Get<IOptions<CacheSettings>>().Value;
            var redisConnection = await RedisService.InitializeAsync(cacheSettings.ConnectionString);

            var cacheData = await redisConnection.BasicRetryAsync(async db => await db.StringGetAsync(cacheKey));

            if (ResultProperty != null && !cacheData.IsNull)
            {
                dc.State.SetValue(dc.GetValue(ResultProperty), cacheData.ToString());
            }

            return await dc.EndDialogAsync(result: cacheData, cancellationToken: cancellationToken);
        }
    }
}
