using AdaptiveExpressions.Properties;
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
    public class GetFirstNamesBySkillAndLocationAction : HttpClientActionBase<GetProfileClient, HttpResponseMessage>
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetFirstNamesBySkillAndLocationAction";

        public GetFirstNamesBySkillAndLocationAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("skill")]
        public StringExpression Skill { get; set; }

        [JsonProperty("currentLocation")]
        public StringExpression CurrentLocation { get; set; }

        [JsonProperty("firstNamesProperty")]
        public StringExpression FirstNamesProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var httpClient = GetClient(dc);
            var responseMessage = await httpClient.SendRequestAsync(request =>
                request.RequestUri = new Uri(GetQuery(request.RequestUri.OriginalString, dc)));

            if (StatusCodeProperty != null)
            {
                dc.State.SetValue(dc.GetValue(StatusCodeProperty), (int)responseMessage.StatusCode);
            }

            if (ReasonPhraseProperty != null)
            {
                dc.State.SetValue(dc.GetValue(ReasonPhraseProperty), responseMessage.ReasonPhrase);
            }

            if (FirstNamesProperty != null)
            {
                var requiredSkill = dc.GetValue(Skill ?? throw new ArgumentNullException(nameof(Skill)));
                var employees = await httpClient.GetContentAsync<List<GetEmployeeModel>>(responseMessage);
                var firstNames = employees
                    .Where(e => e.Skills.Any(s => s.ExperienceLevel.ToLower() == "advanced" && s.Technology.Contains(requiredSkill, StringComparison.OrdinalIgnoreCase)))
                    .Select(e => e.FirstName)
                    .ToList();

                dc.State.SetValue(dc.GetValue(FirstNamesProperty), firstNames);
            }

            return await dc.EndDialogAsync(result: responseMessage, cancellationToken: cancellationToken);
        }

        private string GetQuery(string uri, DialogContext dc)
        {
            AddQueryString(ref uri, dc, CurrentLocation, "currentLocation");
            return uri;
        }
    }
}
