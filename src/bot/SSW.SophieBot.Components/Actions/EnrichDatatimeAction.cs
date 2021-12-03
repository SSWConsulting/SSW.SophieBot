using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSW.SophieBot.Components.Models;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private static readonly string _dayOfWeekPrefix = "XXXX-WXX-";

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

            var timex = datetime.Timex;
            string value;

            if (timex.FirstOrDefault() != null && timex[0].StartsWith(_dayOfWeekPrefix))
            {
                var indexOfWeek = Int32.Parse(timex[0].Last().ToString()) - 1;
                var dayOfWeek = _dayOfWeek.ElementAtOrDefault(indexOfWeek);
                value = DateTime.Now.Date.ToUniversalTime().GetDateByDayOfWeek(dayOfWeek).ToString("yyyy-MM-dd");
            }
            else
            {
                value = timex.FirstOrDefault();
            }

            var result = new EnrichedDatetime
            {
                Timex = datetime.Timex,
                Type = datetime.Type,
                Value = value
            };

            if (ResultProperty != null)
            {
                dc.State.SetValue(dc.GetValue(ResultProperty), result);
            }

            return dc.EndDialogAsync(result, cancellationToken);
        }
    }
}
