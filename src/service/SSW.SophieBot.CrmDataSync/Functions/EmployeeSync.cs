using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace SSW.SophieBot.CrmDataSync.Functions
{
    public class EmployeeSync
    {
        [FunctionName(nameof(SyncEmployeeProfile))]
        public void SyncEmployeeProfile([TimerTrigger("%Timer:Employee%")]TimerInfo timer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
