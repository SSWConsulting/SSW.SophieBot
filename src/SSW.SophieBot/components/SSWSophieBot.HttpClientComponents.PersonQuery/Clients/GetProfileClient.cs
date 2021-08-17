using Microsoft.Extensions.Options;
using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Clients
{
    public class GetProfileClient : HttpClientBase<List<GetEmployeeModel>>
    {
        public override string EndpointName { get; } = "GetProfile";

        public GetProfileClient(HttpClient client, IOptions<HttpClientOptions> options)
            : base(client, options)
        {

        }

        protected override async Task<HttpResponseMessage> GetResponseMessageAsync(Action<HttpRequestMessage> action = null)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, Client.BaseAddress);
            action?.Invoke(requestMessage);

            return await Client.SendAsync(requestMessage);
        }
    }
}
