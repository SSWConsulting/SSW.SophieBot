namespace SSW.SophieBot.DataSync.Crm.Config
{
    public class SyncOptions
    {
        public string OrganizationId { get; set; }

        public SyncFunctionOptions EmployeeSync { get; set; }
    }

    public class SyncFunctionOptions
    {
        public string Timer { get; set; }

        public int MaxRetrieveCountPerRequest { get; set; } = 100;

        public string ContainerId { get; set; }

        public string DatabaseId { get; set; }

        public string TopicName { get; set; }
    }
}
