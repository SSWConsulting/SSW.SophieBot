using System.Collections.Generic;

namespace SSW.SophieBot.DataSync.Domain.Sync
{
    public class SyncData<T>
    {
        public T Data { get; set; }

        public string SyncVersion { get; set; }

        public bool IsVersionUpdated { get; set; }

        public SyncData()
        {

        }

        public SyncData(T data, string syncVersion, bool isVersionUpdated = true)
        {
            Data = data;
            SyncVersion = syncVersion;
            IsVersionUpdated = isVersionUpdated;
        }
    }

    public class SyncListData<T> : SyncData<List<T>>
    {
        public SyncListData() : base()
        {
            Data = new List<T>();
        }

        public SyncListData(List<T> data, string syncVersion, bool isVersionUpdated = true) : base(data, syncVersion, isVersionUpdated)
        {

        }
    }
}
