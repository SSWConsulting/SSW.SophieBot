using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSW.SophieBot.Components.Models;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class SendResponseFromCacheAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "SendResponseFromCacheAction";

        [JsonConstructor]
        public SendResponseFromCacheAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var cacheSettings = dc.Services.Get<IOptions<CacheSettings>>().Value;
            var cache = dc.GetValue(new ObjectExpression<CacheState>(cacheSettings.CacheStatePath));

            ResourceResponse response = null;
            if (cache?.Data != null)
            {
                var activity = JsonConvert.DeserializeObject<Activity>(cache.Data, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                response = await dc.Context.SendActivityAsync(activity, cancellationToken);
            }

            return await dc.EndDialogAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }
}
