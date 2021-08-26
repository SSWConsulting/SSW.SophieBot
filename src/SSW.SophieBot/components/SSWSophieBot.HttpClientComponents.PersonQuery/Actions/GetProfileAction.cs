using AdaptiveExpressions.Properties;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.Abstractions;
using SSWSophieBot.HttpClientComponents.PersonQuery.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetProfileAction : HttpClientActionBase<GetProfileClient, HttpResponseMessage>
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetProfileAction";

        public GetProfileAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("employeesProperty")]
        public StringExpression EmployeesProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var httpClient = GetClient(dc);

            var responseMessage = await httpClient.SendRequestAsync(request => AddQueryStrings(request, dc));

            if (StatusCodeProperty != null)
            {
                dc.State.SetValue(dc.GetValue(StatusCodeProperty), (int)responseMessage.StatusCode);
            }

            if (ReasonPhraseProperty != null)
            {
                dc.State.SetValue(dc.GetValue(ReasonPhraseProperty), responseMessage.ReasonPhrase);
            }

            if (EmployeesProperty != null)
            {
                var employees = await httpClient.GetContentAsync<List<GetEmployeeModel>>(responseMessage);
                dc.State.SetValue(dc.GetValue(EmployeesProperty), employees);
            }

            return await dc.EndDialogAsync(result: responseMessage, cancellationToken: cancellationToken);
        }
    }
}
