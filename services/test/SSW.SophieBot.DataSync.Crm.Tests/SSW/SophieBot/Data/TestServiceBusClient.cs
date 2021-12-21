using SSW.SophieBot.Employees;
using SSW.SophieBot.Sync;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public class TestServiceBusClient : IBatchMessageService<MqMessage<Employee>, string>
    {
        public virtual List<MqMessage<Employee>> MqMessages { get; private set; } = new();

        public virtual Task SendMessageAsync(IEnumerable<MqMessage<Employee>> messages, string options, CancellationToken cancellationToken = default)
        {
            MqMessages = MqMessages.Concat(messages).ToList();
            return Task.CompletedTask;
        }

        public Task SendBatchEndAsync(string topicName, CancellationToken cancellationToken = default)
        {
            MqMessages.Add(MqMessage<Employee>.BatchEnd());
            return Task.CompletedTask;
        }

        public Task SendBatchStartAsync(string topicName, CancellationToken cancellationToken = default)
        {
            MqMessages.Add(MqMessage<Employee>.BatchStart());
            return Task.CompletedTask;
        }
    }
}
