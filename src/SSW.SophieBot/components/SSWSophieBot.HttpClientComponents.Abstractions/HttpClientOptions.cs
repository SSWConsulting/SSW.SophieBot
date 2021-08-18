using System.Collections.Generic;
using System.Linq;

namespace SSWSophieBot.HttpClientComponents.PersonQuery
{
    public class HttpClientOptions
    {
        public const string ConfigName = "HttpClient";

        public string BaseAddress { get; set; }

        public List<HttpClientEndpointOptions> Endpoints { get; set; }

        public HttpClientOptions()
        {
            Endpoints = new List<HttpClientEndpointOptions>();
        }

        public HttpClientEndpointOptions FindEndpointOptionsByName(string name)
        {
            return Endpoints.FirstOrDefault(q => q.Name == name);
        }
    }

    public class HttpClientEndpointOptions
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public Dictionary<string, string> RequestHeaders { get; set; }

        public HttpClientEndpointOptions()
        {
            RequestHeaders = new Dictionary<string, string>();
        }
    }
}
