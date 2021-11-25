using Newtonsoft.Json;
using SSWSophieBot.HttpClientComponents.PersonQuery;

namespace SSWSophieBot.HttpClientAction.Models
{
    public class NextClientModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("type")]
        public BookingStatus Type { get; set; }
    }
}
