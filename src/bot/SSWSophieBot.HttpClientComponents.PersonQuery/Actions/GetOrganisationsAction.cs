using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.HttpClientComponents.Abstractions;
using SSWSophieBot.HttpClientComponents.PersonQuery.Clients;
using SSWSophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetOrganisationsAction : HttpClientActionBase<GetOrganisationsClient, HttpResponseMessage>
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetOrganisationsAction";

        public GetOrganisationsAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("sitesProperty")]
        public StringExpression SitesProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var httpClient = GetClient(dc);

            var responseMessage = await httpClient.SendRequestAsync(request =>
            {
                AddQueryStrings(request, dc);
            });

            if (StatusCodeProperty != null)
            {
                dc.State.SetValue(dc.GetValue(StatusCodeProperty), (int)responseMessage.StatusCode);
            }

            if (ReasonPhraseProperty != null)
            {
                dc.State.SetValue(dc.GetValue(ReasonPhraseProperty), responseMessage.ReasonPhrase);
            }

            if (SitesProperty != null)
            {
                var organisations = await httpClient.GetContentAsync<List<GetOrganisationsModel>>(responseMessage);
                var sswSites = organisations.FirstOrDefault(site => site.Name.Equals("ssw", StringComparison.OrdinalIgnoreCase))?.Sites;

                dc.State.SetValue(dc.GetValue(SitesProperty), sswSites);
            }

            return await dc.EndDialogAsync(result: responseMessage, cancellationToken: cancellationToken);
        }
    }
}
