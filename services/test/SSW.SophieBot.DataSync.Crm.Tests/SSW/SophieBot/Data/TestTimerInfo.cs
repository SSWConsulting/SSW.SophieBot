using Microsoft.Azure.WebJobs;

namespace SSW.SophieBot
{
    public class TestTimerInfo
    {
        public static TimerInfo Instance => new(null, null);
    }
}
