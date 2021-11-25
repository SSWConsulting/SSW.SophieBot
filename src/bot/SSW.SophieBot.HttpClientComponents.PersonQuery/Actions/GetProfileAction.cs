using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSW.SophieBot.Components;
using SSW.SophieBot.HttpClientAction.Models;
using SSW.SophieBot.HttpClientComponents.Abstractions;
using SSW.SophieBot.HttpClientComponents.PersonQuery.Clients;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Actions
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
            var avatarManager = dc.Services.Get<IAvatarManager>();

            var responseMessage = await httpClient.SendRequestAsync(request =>
            {
                AddQueryStrings(request, dc);
                if (string.IsNullOrWhiteSpace(request.RequestUri.Query))
                {
                    throw new ArgumentNullException(nameof(QueryString), $"Please specify a filtering item to query");
                }
            });

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

                if (avatarManager != null)
                {
                    employees = await avatarManager.GetAvatarUrlsAsync(employees);
                }

                dc.State.SetValue(dc.GetValue(EmployeesProperty), employees);
            }

            return await dc.EndDialogAsync(result: responseMessage, cancellationToken: cancellationToken);
        }
    }
}
