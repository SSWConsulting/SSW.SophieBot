using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSW.SophieBot.Components.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Components.Actions
{
    public class EnrichDatatimeAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "EnrichDatatimeAction";

        [JsonConstructor]
        public EnrichDatatimeAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("datetime")]
        public ObjectExpression<Datetime> Datetime { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        private static readonly Regex _dayOfWeekRegex = new Regex("^XXXX-WXX-(?<index>[1-7])$");

        private static readonly Regex _dayRegex = new Regex("^XXXX-(?<dayString>\\d{2}-\\d{2})$");

        private static readonly string _dateFormat = "yyyy-MM-dd";

        private static readonly DayOfWeek[] _dayOfWeek = new[]
       {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday,
        };

        public override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var datetime = dc.GetValue(Datetime);

            if (datetime == null)
            {
                throw new ArgumentNullException(nameof(Datetime));
            }

            static string GetValueFrom(List<string> timex)
            {
                var dateString = timex.FirstOrDefault();

                var dayOfWeekMatches = _dayOfWeekRegex.Match(dateString);
                if (dayOfWeekMatches.Success)
                {
                    var indexOfWeek = Int32.Parse(dayOfWeekMatches.Groups["index"].Value) - 1;
                    var dayOfWeek = _dayOfWeek.ElementAtOrDefault(indexOfWeek);
                    return DateTime.Now.Date.ToUniversalTime().GetDateByDayOfWeek(dayOfWeek).ToString(_dateFormat);
                }

                var dayMatches = _dayRegex.Match(dateString);
                if (dayMatches.Success)
                {
                    var dayString = dayMatches.Groups["dayString"].Value;
                    return DateTime.Parse(dayString).ToString(_dateFormat);
                }

                return dateString;
            }

            var result = new EnrichedDatetime
            {
                Timex = datetime.Timex,
                Type = datetime.Type,
                Value = GetValueFrom(datetime.Timex)
            };

            if (ResultProperty != null)
            {
                dc.State.SetValue(dc.GetValue(ResultProperty), result);
            }

            return dc.EndDialogAsync(result, cancellationToken);
        }
    }
}
