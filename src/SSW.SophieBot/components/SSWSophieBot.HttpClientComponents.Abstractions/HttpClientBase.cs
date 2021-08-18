using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SSWSophieBot.HttpClientComponents.PersonQuery;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.Abstractions
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

            responseMessage.EnsureSuccessStatusCode();

            return await GetResponseAsync(responseMessage);
        }

        protected abstract Task<HttpResponseMessage> GetResponseMessageAsync(Action<HttpRequestMessage> action = null);

        protected virtual async Task<TResponse> GetResponseAsync(HttpResponseMessage responseMessage)
        {
            using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
            using var reader = new JsonTextReader(new StreamReader(responseStream, Encoding.UTF8));
            return new JsonSerializer().Deserialize<TResponse>(reader);
        }
    }
}
