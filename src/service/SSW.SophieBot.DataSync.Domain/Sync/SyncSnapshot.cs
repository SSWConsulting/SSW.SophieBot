using System;

namespace SSW.SophieBot.DataSync.Domain.Sync
{
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
