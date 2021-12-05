using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Sync
{
    public interface IMessageService<TMessage, TOptions>
    {
        Task SendMessageAsync(TMessage message, TOptions options, CancellationToken cancellationToken = default);
    }

    public interface IBatchMessageService<TMessage, TOptions> : IMessageService<IEnumerable<TMessage>, TOptions>
    {
        Task SendBatchStartAsync(string topicName, CancellationToken cancellationToken = default);

        Task SendBatchEndAsync(string topicName, CancellationToken cancellationToken = default);
    }
}
