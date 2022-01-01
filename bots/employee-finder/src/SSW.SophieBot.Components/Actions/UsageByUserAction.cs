using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSW.SophieBot.Components.Services;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class UsageByUserAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "UsageByUserAction";

        [JsonConstructor]
        public UsageByUserAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("pastDays")]
        public IntExpression PastDays { get; set; }

        [JsonProperty("maxItemsCount")]
        public IntExpression MaxItemsCount { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var telemetryService = dc.Services.Get<ITelemetryService>();

            var pastDays = PastDays?.GetValue(dc.State);
            var maxItemsCount = MaxItemsCount?.GetValue(dc.State);

            var result = await telemetryService.GetUsageByUserAsync(pastDays, maxItemsCount, groupKey => $"replace_string({groupKey}, ' www.ssw.com.au', '')");

            if (ResultProperty != null)
            {
                dc.State.SetValue(ResultProperty.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
