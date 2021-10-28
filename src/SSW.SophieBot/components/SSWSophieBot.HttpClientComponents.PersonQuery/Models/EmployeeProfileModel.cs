using Newtonsoft.Json;
using SSWSophieBot.HttpClientAction.Models;
using System.Collections.Generic;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeProfileWithStatusModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("clients")]
        public List<string> Clients { get; set; }

        [JsonProperty("bookingStatus")]
        public BookingStatus BookingStatus { get; set; }

        [JsonProperty("lastSeenAt")]
        public GetLastSeenAtSiteModel LastSeenAt { get; set; }

        [JsonProperty("lastSeenTime")]
        public string LastSeenTime { get; set; }

        [JsonProperty("skills")]
        public List<GetSkillModel> Skills { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("mobilePhone")]
        public string MobilePhone { get; set; }

        [JsonProperty("defaultSite")]
        public GetLocationModel DefaultSite { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("billableRate")]
        public double BillableRate { get; set; }
    }
}
