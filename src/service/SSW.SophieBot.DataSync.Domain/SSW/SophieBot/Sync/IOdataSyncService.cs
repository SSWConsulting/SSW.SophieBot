namespace SSW.SophieBot.Sync
{
    public interface IOdataSyncService<T> : ISyncService<OdataResponse<T>>
    {
    }

    public interface IPagedOdataSyncService<T> : ISyncService<OdataPagedResponse<T>>
    {

    }
}
