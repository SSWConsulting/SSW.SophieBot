using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.Components.Actions
{
    public class PersonNameAlterateAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "PersonNameAlterateAction";

        public PersonNameAlterateAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("originalName")]
        public StringExpression OriginalName { get; set; }

        [JsonProperty("alteredProperty")]
        public StringExpression AlteredProperty { get; set; }

        [JsonProperty("resultFirstNameProperty")]
        public StringExpression ResultFirstNameProperty { get; set; }

        [JsonProperty("resultLastNameProperty")]
        public StringExpression ResultLastNameProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var originalName = dc.GetValue(OriginalName)?.Trim();
            var altered = false;
            var resultFirstName = string.Empty;
            var resultLastName = string.Empty;

            if (!string.IsNullOrWhiteSpace(originalName))
            {
                var splits = originalName.Split(' ');
                var originalFirstName = splits.First();
                var originalLastName = splits.Length > 1 ? splits.Last() : null;

                (resultFirstName, resultLastName) = HandleApostrophe(originalFirstName, originalLastName, ref altered);

                if (AlteredProperty != null)
                {
                    dc.State.SetValue(dc.GetValue(AlteredProperty), altered);
                }

                if (ResultFirstNameProperty != null)
                {
                    dc.State.SetValue(dc.GetValue(ResultFirstNameProperty), resultFirstName);
                }

                if (ResultLastNameProperty != null)
                {
                    dc.State.SetValue(dc.GetValue(ResultLastNameProperty), resultLastName);
                }
            }

            return await dc.EndDialogAsync(result: $"{resultFirstName} {resultLastName}".Trim(), cancellationToken: cancellationToken);
        }

        private static (string altFirstName, string altLastName) HandleApostrophe(string firstName, string lastName, ref bool altered)
        {
            if (lastName != null && lastName.Length > 1 && lastName.EndsWith('s'))
            {
                altered = true;
                return (firstName, lastName[0..^1]);
            }
            else if (lastName == null && firstName.Length > 1 && firstName.EndsWith('s'))
            {
                altered = true;
                return (firstName[0..^1], null);
            }

            return (firstName, lastName);
        }
    }
}
