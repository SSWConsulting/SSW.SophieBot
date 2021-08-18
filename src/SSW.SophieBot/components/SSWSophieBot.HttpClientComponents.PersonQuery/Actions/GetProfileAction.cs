using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.Abstractions;
using SSWSophieBot.HttpClientComponents.PersonQuery.Clients;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetProfileAction : HttpClientActionBase<GetProfileClient, List<GetEmployeeModel>>
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetProfileAction";

        public GetProfileAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("firstName")]
        public StringExpression FirstName { get; set; }

        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var firstName = FirstName.GetValue(dc.State);
            if (string.IsNullOrWhiteSpace(firstName))
            {
                throw new ArgumentNullException(nameof(FirstName), $"{nameof(FirstName)} param value cannot be null");
            }

            var profiles = await GetClient(dc).SendRequestAsync(request => request.RequestUri = new Uri(request.RequestUri, $"?firstName={firstName}"));

            if (ResultProperty != null)
            {
                dc.State.SetValue(ResultProperty.GetValue(dc.State), profiles);
            }

            return await dc.EndDialogAsync(result: profiles, cancellationToken: cancellationToken);
        }
    }
}
