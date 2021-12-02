using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Domain.Sync
{
    public interface IMessageService<TMessage, TOptions>
    {
        Task SendMessageAsync(TMessage message, TOptions options, CancellationToken cancellationToken = default);
    }

    public interface IBatchMessageService<TMessage, TOptions> : IMessageService<IEnumerable<TMessage>, TOptions>
    {

    }
}
