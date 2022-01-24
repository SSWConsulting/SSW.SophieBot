using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class ClientNowAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "ClientNowAction";

        [JsonConstructor]
        public ClientNowAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("format")]
        public StringExpression TargetFormat { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var clientNow = DateTime.UtcNow.ToUserLocalTime(dc);
            var result = clientNow.ToString(dc.GetValue(TargetFormat) ?? "d");

            if (ResultProperty != null)
            {
                dc.State.SetValue(dc.GetValue(ResultProperty), result);
            }

            return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
