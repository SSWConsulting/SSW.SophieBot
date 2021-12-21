using SSW.SophieBot.DataSync.Crm.HttpClients;

namespace SSW.SophieBot.DataSync.Crm.Sync
{
    public abstract class CrmOdataService
    {
        protected virtual CrmClient CrmClient { get; }

        public CrmOdataService(CrmClient crmClient)
        {
            CrmClient = crmClient;
        }
    }
}
