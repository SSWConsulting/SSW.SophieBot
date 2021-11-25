using Newtonsoft.Json;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeBillableItemModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("billedDays")]
        public int BilledDays { get; set; }

        [JsonProperty("bookingStatus")]
        public BookingStatus BookingStatus { get; set; }

        [JsonProperty("lastSeen")]
        public string LastSeen { get; set; }
    }
}
