using AdaptiveExpressions.Properties;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace SSWSophieBot.HttpClientComponents.Abstractions
{
    public abstract class HttpClientActionBase<TClient, TResponse> : Dialog
        where TClient : HttpClientBase<TResponse>
        where TResponse : class
    {
        [JsonProperty("statusCodeProperty")]
        public StringExpression StatusCodeProperty { get; set; }

        [JsonProperty("reasonPhraseProperty")]
        public StringExpression ReasonPhraseProperty { get; set; }

        [JsonConstructor]
        public HttpClientActionBase([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            if (!string.IsNullOrWhiteSpace(sourceFilePath))
            {
                RegisterSourceLocation(sourceFilePath, sourceLineNumber);
            }
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

        protected virtual void AddQueryString(ref string uri, DialogContext dc, ExpressionProperty<string> expressionProperty, string queryKey)
        {
            if (expressionProperty != null)
            {
                var queryValue = dc.GetValue(expressionProperty);
                if (!string.IsNullOrWhiteSpace(queryValue))
                {
                    uri = QueryHelpers.AddQueryString(uri, queryKey, queryValue);
                }
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
