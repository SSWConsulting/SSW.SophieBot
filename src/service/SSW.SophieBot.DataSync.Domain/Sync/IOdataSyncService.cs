namespace SSW.SophieBot.DataSync.Domain.Sync
{
    public interface IOdataSyncService<T> : ISyncService<OdataResponse<T>>
    {
    }

    public interface IPagedOdataSyncService<T> : ISyncService<OdataPagedResponse<T>>
    {

    }
}
