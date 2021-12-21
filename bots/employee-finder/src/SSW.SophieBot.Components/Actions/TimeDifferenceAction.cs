using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class TimeDifferenceAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "TimeDifferenceAction";

        [JsonConstructor]
        public TimeDifferenceAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("date")]
        public StringExpression Date { get; set; }

        [JsonProperty("result")]
        public StringExpression Result { get; set; }

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            String result;
            var dateString = Date?.GetValue(dc.State) ?? throw new ArgumentNullException(nameof(Date));
            var dateTime = DateTimeOffset.Parse(dateString).ToUniversalTime();

            static int GetInteger(double value)
            {
                return Math.Max(1, (int)Math.Floor(value));
            }

            var now = DateTimeOffset.UtcNow;
            var timeDiff = now - dateTime;

            if (timeDiff.TotalDays < 1)
            {
                var hours = GetInteger(timeDiff.TotalHours);
                result = $"{hours} {(hours == 1 ? "hour" : "hours")} ago";
            }
            else if (timeDiff.TotalDays < 30)
            {
                var days = GetInteger(timeDiff.TotalDays);
                result = $"{days} {(days == 1 ? "day" : "days")} ago";
            }
            else
            {
                var months = GetInteger(timeDiff.TotalDays / 30);
                result = $"{months} {(months == 1 ? "month" : "months")} ago";
            }

            if (Result != null)
            {
                dc.State.SetValue(Result.GetValue(dc.State), result);
            }

            return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
