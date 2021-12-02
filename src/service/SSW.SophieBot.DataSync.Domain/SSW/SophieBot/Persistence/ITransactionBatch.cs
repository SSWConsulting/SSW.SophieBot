using System.Threading.Tasks;

namespace SSW.SophieBot.Persistence
{
    public interface ITransactionBatch<TOperation>
    {
        ITransactionBatch<TOperation> Patch(string id, TOperation operation);

        Task<bool> SaveChangesAsync();
    }
}
