using Newtonsoft.Json;
using SSW.SophieBot.HttpClientAction.Models;
using System.Collections.Generic;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeWithBookingInfoModel
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("bookingStatus")]
        public BookingStatus BookingStatus { get; set; }

        [JsonProperty("clients")]
        public List<EmployeeProfileClient> Clients { get; set; }

        [JsonProperty("lastSeenAt")]
        public GetLastSeenAtSiteModel LastSeenAt { get; set; }

        [JsonProperty("lastSeenTime")]
        public string LastSeenTime { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("bookedDays")]
        public int BookedDays { get; set; }

        [JsonProperty("clientsInfo")]
        public List<ClientsInfoModel> ClientsInfo { get; set; }
    }
    public class ClientsInfoModel
    {
        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("clientNumber")]
        public string ClientNumber { get; set; }

        [JsonProperty("primaryContactId")]
        public string PrimaryContactId { get; set; }

        [JsonProperty("primaryContactName")]
        public string PrimaryContactName { get; set; }

        [JsonProperty("primaryContactPhone")]
        public string PrimaryContactPhone { get; set; }

        [JsonProperty("primaryContactEmail")]
        public string PrimaryContactEmail { get; set; }

        [JsonProperty("accountManagerId")]
        public string AccountManagerId { get; set; }

        [JsonProperty("accountManagerName")]
        public string AccountManagerName { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("industry")]
        public string Industry { get; set; }

        [JsonProperty("industryCode")]
        public int? IndustryCode { get; set; }
    }
}
