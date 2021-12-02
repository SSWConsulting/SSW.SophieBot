using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public interface IRepository<T>
    {
        Task<List<T>> GetAllAsync(string query, IEnumerable<(string name, object value)> parameters, CancellationToken cancellationToken = default);
    }
}
