using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public interface IRecognizerSchemaManager
    {
        Task PublishSchemaAsync(CancellationToken cancellationToken = default);

        Task SeedAsync(CancellationToken cancellationToken = default);
    }
}
