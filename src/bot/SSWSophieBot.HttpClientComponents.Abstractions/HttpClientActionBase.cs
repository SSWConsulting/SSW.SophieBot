using AdaptiveExpressions.Properties;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace SSWSophieBot.HttpClientComponents.Abstractions
{
    public abstract class HttpClientActionBase<TClient, TResponse> : ActionBase
        where TClient : HttpClientBase<TResponse>
        where TResponse : class
    {
        [JsonProperty("queryString")]
        public List<QueryStringExpression> QueryString { get; set; }

        [JsonProperty("statusCodeProperty")]
        public StringExpression StatusCodeProperty { get; set; }

        [JsonProperty("reasonPhraseProperty")]
        public StringExpression ReasonPhraseProperty { get; set; }

        [JsonConstructor]
        public HttpClientActionBase([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        protected virtual TClient GetClient(DialogContext dc)
        {
            var client = dc.Services.Get<TClient>();
            if (client == null)
            {
                throw new InvalidOperationException($"Http client {typeof(TClient).Name} is not registered");
            }

            return client;
        }

        protected virtual void AddQueryStrings(HttpRequestMessage requestMessage, DialogContext dc)
        {
            var queryStringPairs = QueryString?.Select(qs => qs.GetQueryStrings(dc)).Where(qs => qs.HasValue);
            if (queryStringPairs != null && queryStringPairs.Any())
            {
                var queryStrings = new Dictionary<string, string>(queryStringPairs.Select(pair => pair.Value));
                requestMessage.RequestUri = new Uri(QueryHelpers.AddQueryString(requestMessage.RequestUri.OriginalString, queryStrings));
            }
        }
    }

    public abstract class HttpClientActionBase<TClient> : HttpClientActionBase<TClient, HttpResponseMessage>
        where TClient : HttpClientBase
    {
        [JsonConstructor]
        public HttpClientActionBase([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }
    }
}
