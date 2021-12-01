using System;

namespace SSW.SophieBot.DataSync.Domain.Employees
{
    public class LastSeenAtSite
    {
        public string SiteId { get; set; }
        public DateTimeOffset LastSeen { get; set; }
        public string MacAddress { get; set; }
    }
}
