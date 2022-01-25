using Newtonsoft.Json;
using SSW.SophieBot.HttpClientAction.Models;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Models
{
    public class FreeEmployeeModel
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("defaultSite")]
        public GetLocationModel DefaultSite { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("billedDays")]
        public int BilledDays { get; set; }

        [JsonProperty("billedHours")]
        public int BilledHours { get; set; }

        [JsonProperty("inOffice")]
        public bool InOffice { get; set; }

        [JsonProperty("lastSeen")]
        public string LastSeen { get; set; }

        [JsonProperty("nextClient")]
        public NextClientModel NextClient { get; set; }

        [JsonProperty("freeDays")]
        public int FreeDays { get; set; }
    }
}
