using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.Components.Actions
{
    public class StringJoinAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "StringJoinAction";

        public StringJoinAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("stringList")]
        public ArrayExpression<string> StringList { get; set; }

        [JsonProperty("delimiter")]
        public StringExpression Delimiter { get; set; }

        [JsonProperty("lastDelimiter")]
        public StringExpression LastDelimiter { get; set; }

        [JsonProperty("wrapCount")]
        public IntExpression WrapCount { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var resultString = string.Empty;

            var stringList = dc.GetValue(StringList);
            if (stringList != null && stringList.Any() && ResultProperty != null)
            {
                var delimiter = dc.GetValue(Delimiter) ?? string.Empty;
                var wrapCount = dc.GetValue(WrapCount);

                if (wrapCount > 0 && stringList.Count > wrapCount)
                {
                    stringList = stringList.Take(wrapCount - 1).Append($"{stringList.Count - wrapCount + 1} others").ToList();
                }

                if (LastDelimiter != null && stringList.Count > 1)
                {
                    var lastDelimiter = dc.GetValue(LastDelimiter) ?? string.Empty;
                    resultString = string.Join(delimiter, stringList.Take(stringList.Count - 1)) + lastDelimiter + stringList.Last();
                }
                else
                {
                    resultString = string.Join(delimiter, stringList);
                }

                dc.State.SetValue(dc.GetValue(ResultProperty), resultString);
            }

            return await dc.EndDialogAsync(result: resultString, cancellationToken: cancellationToken);
        }
    }
}
