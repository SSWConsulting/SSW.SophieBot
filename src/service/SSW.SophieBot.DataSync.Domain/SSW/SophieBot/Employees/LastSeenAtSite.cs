using System;

namespace SSW.SophieBot.Employees
{
    public class LastSeenAtSite
    {
        public string SiteId { get; set; }
        public DateTimeOffset LastSeen { get; set; }
        public string MacAddress { get; set; }
    }
}
