using System;

namespace SSW.SophieBot.Persistence
{
    // TODO: override object.Equals
    public class SyncSnapshot
    {
        public string Id { get; set; }

        public string OrganizationId { get; set; }

        public DateTime Modifiedon { get; set; }

        public string SyncVersion { get; set; }
    }

    public class SyncSnapshot<T> : SyncSnapshot
    {
        public T Data { get; set; }
    }
}
