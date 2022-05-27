using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class TimeFormatAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "TimeFormatAction";

        [JsonConstructor]
        public TimeFormatAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("datetime")]
        public StringExpression DateTimeString { get; set; }

        [JsonProperty("format")]
        public StringExpression TargetFormat { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var dateTimeString = DateTimeString?.GetValue(dc.State);
            var targetFormat = TargetFormat?.GetValue(dc.State);

            string result;

            if (string.IsNullOrWhiteSpace(dateTimeString))
            {
                result = DateTime.Now.ToString(targetFormat ?? "yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            else if (DateTime.TryParse(dateTimeString, out var dateTime))
            {
                result = dateTime.ToString(targetFormat ?? "yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            else
            {
                result = dateTimeString;
            }

            if (ResultProperty != null)
            {
                dc.State.SetValue(ResultProperty.GetValue(dc.State), result);
            }

            return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
