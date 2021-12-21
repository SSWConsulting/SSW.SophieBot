namespace SSW.SophieBot.DataSync.Crm.Config
{
    public class CrmOptions
    {
        public string AppId { get; set; }

        public string AppSecret { get; set; }

        public string AppTenantId { get; set; }

        public string AccessTokenEndPoint { get; set; }

        public string AccessTokenScope { get; set; }

        public string BaseUri { get; set; }

        public int OdataCallTimeOut { get; set; }

        public string GetFormatedTokenEndpoint()
        {
            if (!string.IsNullOrEmpty(AccessTokenEndPoint) && !string.IsNullOrWhiteSpace(AppTenantId))
            {
                return string.Format(AccessTokenEndPoint, AppTenantId);
            }

            return default;
        }
    }
}
