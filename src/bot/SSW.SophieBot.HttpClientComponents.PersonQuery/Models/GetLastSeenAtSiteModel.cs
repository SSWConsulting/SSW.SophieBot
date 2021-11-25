using System;

namespace SSW.SophieBot.HttpClientAction.Models
{
    public class GetLastSeenAtSiteModel
    {
        public string SiteId { get; set; }
        public DateTimeOffset LastSeen { get; set; }
        public string MacAddress { get; set; }
    }
}
