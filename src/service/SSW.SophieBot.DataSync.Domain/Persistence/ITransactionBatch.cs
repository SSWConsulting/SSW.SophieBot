using System.Threading.Tasks;

namespace SSW.SophieBot.DataSync.Domain.Persistence
{
    public interface ITransactionBatch<TOperation>
    {
        ITransactionBatch<TOperation> Patch(string id, TOperation operation);

        Task<bool> SaveChangesAsync();
    }
}
