using Microsoft.Azure.WebJobs;

namespace SSW.SophieBot.DataSync.Crm.Test.Data
{
    public class TestTimerInfo
    {
        public static TimerInfo Instance => new(null, null);
    }
}
