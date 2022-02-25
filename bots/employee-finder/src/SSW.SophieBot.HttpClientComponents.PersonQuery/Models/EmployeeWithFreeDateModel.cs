using Newtonsoft.Json;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeWithFreeDateModel
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("freeDate")]
        public string FreeDate { get; set; }

        [JsonProperty("bookedDays")]
        public int BookedDays { get; set; }

        [JsonProperty("timeDuration")]
        public string TimeDuration { get; set; }
        
        [JsonProperty("isFreeForXDaysFlag")]
        public bool IsFreeForXDaysFlag { get; set; }
    }
}
