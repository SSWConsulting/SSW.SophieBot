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
        public List<string> Clients { get; set; }

        [JsonProperty("lastSeenAt")]
        public GetLastSeenAtSiteModel LastSeenAt { get; set; }

        [JsonProperty("lastSeenTime")]
        public string LastSeenTime { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("bookedDays")]
        public int BookedDays { get; set; }
    }
}
