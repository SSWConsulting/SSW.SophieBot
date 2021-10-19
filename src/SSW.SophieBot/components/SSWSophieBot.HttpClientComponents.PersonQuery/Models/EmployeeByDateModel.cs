using Newtonsoft.Json;
using SSWSophieBot.HttpClientAction.Models;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeByDateModel
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("defaultSite")]
        public GetLocationModel DefaultSite { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("currentAppointment")]
        public GetAppointmentModel CurrentAppointment { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
