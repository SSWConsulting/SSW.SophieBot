using Newtonsoft.Json;
using SSW.SophieBot.HttpClientComponents.PersonQuery;
using System;

namespace SSW.SophieBot.HttpClientAction.Models
{
    public class NextClientModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonIgnore]
        public DateTime Date { get; set; }

        [JsonProperty("date")]
        public string DateText { get; set; }

        [JsonProperty("type")]
        public BookingStatus Type { get; set; }
    }
}
