using System.Collections.Generic;

namespace SSW.SophieBot.Persistence
{
    public class SyncData<T>
    {
        public T Data { get; set; }

        public bool IsVersionUpdated { get; set; }

        public SyncData()
        {

        }

        public SyncData(T data, bool isVersionUpdated = true)
        {
            Data = data;
            IsVersionUpdated = isVersionUpdated;
        }
    }

    public class SyncListData<T> : SyncData<List<T>>
    {
        public SyncListData() : base()
        {
            Data = new List<T>();
        }

        public SyncListData(List<T> data, bool isVersionUpdated = true) : base(data, isVersionUpdated)
        {

        }
    }
}
