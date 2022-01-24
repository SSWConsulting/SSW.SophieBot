using Newtonsoft.Json;
using SSW.SophieBot.HttpClientAction.Models;
using System.Collections.Generic;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeByDateModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

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

        [JsonProperty("Clients")]
        public List<string> Clients { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
