using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace SSWSophieBot.HttpClientComponents.Abstractions
{
    public abstract class HttpClientActionBase<TClient, TResponse> : Dialog
        where TClient : HttpClientBase<TResponse>
        where TResponse : class
    {
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
    }
}
