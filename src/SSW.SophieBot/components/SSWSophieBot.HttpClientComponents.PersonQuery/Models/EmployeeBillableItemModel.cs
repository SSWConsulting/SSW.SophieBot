using Newtonsoft.Json;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeBillableItemModel
    {
        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("billedDays")]
        public int BilledDays { get; set; }

        [JsonProperty("onClientWork")]
        public bool? OnClientWork { get; set; }

        [JsonProperty("lastSeen")]
        public string LastSeen { get; set; }
    }
}
