using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.UtcConvertAction
{
    public class UtcConvertAction : Dialog
    {
        [JsonConstructor]
        public UtcConvertAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            if (!string.IsNullOrWhiteSpace(sourceFilePath))
            {
                RegisterSourceLocation(sourceFilePath, sourceLineNumber);
            }
        }

        [JsonProperty("$kind")]
        public const string Kind = "UtcConvertAction";

        [JsonProperty("datetime")]
        public StringExpression DateTimeString { get; set; }

        [JsonProperty("format")]
        public StringExpression TargetFormat { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var dateTime = DateTimeString.GetValue(dc.State);
            var targetFormat = TargetFormat.GetValue(dc.State);

            string result;
            if (DateTime.TryParse(dateTime, out var dateTimeLocal))
            {
                dateTimeLocal = TimeZoneInfo.ConvertTimeToUtc(dateTimeLocal);
                result = dateTimeLocal.ToString(targetFormat ?? "yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            else
            {
                result = dateTime;
            }

            if (ResultProperty != null)
            {
                dc.State.SetValue(ResultProperty.GetValue(dc.State), result);
            }

            return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
