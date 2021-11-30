using Newtonsoft.Json;
using SSW.SophieBot.HttpClientComponents.PersonQuery;

namespace SSW.SophieBot.HttpClientAction.Models
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
