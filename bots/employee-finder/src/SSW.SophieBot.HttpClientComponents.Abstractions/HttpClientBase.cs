using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSW.SophieBot.HttpClientComponents.PersonQuery;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SSW.SophieBot.HttpClientComponents.Abstractions
{
    public abstract class HttpClientBase<TResponse>
        where TResponse : class
    {
        public abstract string EndpointName { get; }

        protected virtual HttpClient Client { get; }

        protected virtual HttpClientOptions Options { get; }

        public HttpClientBase(HttpClient client, IOptions<HttpClientOptions> options)
        {
            Options = options.Value;

            client.BaseAddress = new Uri(Options.BaseAddress);
            var endpointOptions = Options.FindEndpointOptionsByName(EndpointName);
            if (endpointOptions != null)
            {
                client.BaseAddress = new Uri(client.BaseAddress, endpointOptions.Path);
                foreach (var header in endpointOptions.RequestHeaders)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            Client = client;
        }

        public virtual async Task<TResponse> SendRequestAsync(Action<HttpRequestMessage> action = null)
        {
            var responseMessage = await GetResponseMessageAsync(action);
            return await GetResponseAsync(responseMessage);
        }

        public virtual async Task<TResponse> GetResponseAsync(HttpResponseMessage responseMessage)
        {
            return await GetContentAsync<TResponse>(responseMessage);
        }

        public virtual async Task<T> GetContentAsync<T>(HttpResponseMessage responseMessage)
        {
            using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
            using var reader = new JsonTextReader(new StreamReader(responseStream, Encoding.UTF8));
            return new JsonSerializer().Deserialize<T>(reader);
        }

        protected abstract Task<HttpResponseMessage> GetResponseMessageAsync(Action<HttpRequestMessage> action = null);
    }

    public abstract class HttpClientBase : HttpClientBase<HttpResponseMessage>
    {
        public HttpClientBase(HttpClient client, IOptions<HttpClientOptions> options)
            : base(client, options)
        {

        }

        public override Task<HttpResponseMessage> GetResponseAsync(HttpResponseMessage responseMessage)
        {
            return Task.FromResult(responseMessage);
        }
    }
}
