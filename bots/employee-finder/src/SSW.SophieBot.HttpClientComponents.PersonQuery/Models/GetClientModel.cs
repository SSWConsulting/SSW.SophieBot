namespace SSW.SophieBot.HttpClientAction.Models
{
    public class GetClientModel
    {
        public string AccountId { get; set; }
        public string ClientName { get; set; }
        public string ClientNumber { get; set; }
        public string PrimaryContactId { get; set; }
        public string PrimaryContactName { get; set; }
        public string PrimaryContactPhone { get; set; }
        public string PrimaryContactEmail { get; set; }
        public string AccountManagerId { get; set; }
        public string AccountManagerName { get; set; }
        public string Address { get; set; }
        public string Industry { get; set; }
        public int? IndustryCode { get; set; }
    }
}
