using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public interface IRepository<T>
    {
        Task<List<T>> GetAllAsync(string query, CancellationToken cancellationToken = default);
    }
}
