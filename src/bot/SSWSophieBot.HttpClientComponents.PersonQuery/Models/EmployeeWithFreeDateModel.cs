using Newtonsoft.Json;
using SSWSophieBot.HttpClientAction.Models;
using System.Collections.Generic;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeWithFreeDateModel
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("FreeDate")]
        public string FreeDate { get; set; }

        [JsonProperty("bookedDays")]
        public int BookedDays { get; set; }
    }
}
