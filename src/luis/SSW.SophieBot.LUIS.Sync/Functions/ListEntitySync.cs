using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.LUIS.Core;
using SSW.SophieBot.LUIS.Model;
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
            Connection = SbConnectionStringName)] SSWPeopleName[] peopleNames)
        {
            _logger.LogInformation($"C# ServiceBus topic trigger function processed message: {string.Join(", ", peopleNames.Select(name => name.FirstName))}");
        }
    }
}
