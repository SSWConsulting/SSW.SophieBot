using SSW.SophieBot.Persistence;
using System;

namespace SSW.SophieBot.Sync
{
    public class MqMessage<T>
    {
        public T Message { get; set; }

        public SyncMode SyncMode { get; set; }

        public BatchMode BatchMode { get; set; }

        public DateTime ModifiedOn { get; set; }

        public string SyncVersion { get; set; }

        public MqMessage()
        {
            SyncMode = SyncMode.None;
        }

        public MqMessage(T message, SyncMode syncMode, BatchMode batchMode, DateTime modifiedOn, string syncVersion)
        {
            Message = message;
            SyncMode = syncMode;
            BatchMode = batchMode;
            ModifiedOn = modifiedOn;
            SyncVersion = syncVersion;
        }

        public static MqMessage<T> BatchStart(T message = default)
        {
            return new MqMessage<T>
            {
                Message = message,
                BatchMode = BatchMode.BatchStart
            };
        }

        public static MqMessage<T> BatchEnd(T message = default)
        {
            return new MqMessage<T>
            {
                Message = message,
                BatchMode = BatchMode.BatchEnd
            };
        }
    }
}
