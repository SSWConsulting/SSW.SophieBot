﻿using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class UtcConvertAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "UtcConvertAction";

        [JsonConstructor]
        public UtcConvertAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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
            var dateTime = dc.GetValue(DateTimeString);
            var targetFormat = dc.GetValue(TargetFormat);

            string result;
            if (string.IsNullOrEmpty(dateTime))
            {
                result = null;
            }
            else if (DateTime.TryParse(dateTime, out var dateTimeLocal))
            {
                dateTimeLocal = dateTimeLocal.FromUserLocalTime(dc);
                result = dateTimeLocal.ToString(targetFormat ?? "yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            else
            {
                result = dateTime;
            }

            if (ResultProperty != null)
            {
                dc.State.SetValue(dc.GetValue(ResultProperty), result);
            }

            return dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
