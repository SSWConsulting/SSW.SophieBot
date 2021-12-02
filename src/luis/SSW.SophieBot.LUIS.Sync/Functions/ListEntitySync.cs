using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Sync;
using System.Linq;

namespace SSW.SophieBot.LUIS.Sync.Functions
{
    public class ListEntitySync
    {
        private const string SbConnectionStringName = "SophieBotServiceBus";

        private readonly LuisOptions _options;
        private readonly ILogger<ListEntitySync> _logger;

        public ListEntitySync(IOptions<LuisOptions> options, ILogger<ListEntitySync> log)
        {
            _options = options.Value;
            _logger = log;
        }

        [FunctionName(nameof(SyncSSWPeopleNames))]
        public void SyncSSWPeopleNames([ServiceBusTrigger(
            "%ServiceBus:SswPeopleNames:Topic%",
            "%ServiceBus:SswPeopleNames:Subscription%",
            Connection = SbConnectionStringName)] MqMessage<Employee>[] employees)
        {
            _logger.LogInformation($"C# ServiceBus topic trigger function processed message: " +
                $"{string.Join(", ", employees.Select(employee => employee.Message.FirstName))}");
        }
    }
}
