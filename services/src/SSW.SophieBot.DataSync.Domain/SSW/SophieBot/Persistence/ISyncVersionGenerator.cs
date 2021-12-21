using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public interface ISyncVersionGenerator
    {
        Task<string> GenerateAsync(CancellationToken cancellationToken = default);
    }
}
