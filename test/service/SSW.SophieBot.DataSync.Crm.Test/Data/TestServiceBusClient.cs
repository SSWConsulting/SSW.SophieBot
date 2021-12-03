using SSW.SophieBot.Employees;
using SSW.SophieBot.Sync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Crm.Test.Data
{
    public class TestServiceBusClient : IBatchMessageService<MqMessage<Employee>, string>
    {
        public virtual List<MqMessage<Employee>> MqMessages { get; private set; } = new();

        public virtual Task SendMessageAsync(IEnumerable<MqMessage<Employee>> messages, string options, CancellationToken cancellationToken = default)
        {
            MqMessages = MqMessages.Concat(messages).ToList();
            return Task.CompletedTask;
        }
    }
}
