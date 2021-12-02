using SSW.SophieBot.Persistence;
using System;

namespace SSW.SophieBot.Sync
{
    public class MqMessage<T>
    {
        public T Message { get; set; }

        public SyncMode SyncMode { get; set; }

        public DateTime ModifiedOn { get; set; }

        public string SyncVersion { get; set; }

        public MqMessage()
        {
            SyncMode = SyncMode.None;
        }

        public MqMessage(T message, SyncMode syncMode, DateTime modifiedOn, string syncVersion)
        {
            Message = message;
            SyncMode = syncMode;
            ModifiedOn = modifiedOn;
            SyncVersion = syncVersion;
        }
    }
}
