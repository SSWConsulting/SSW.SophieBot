using Newtonsoft.Json;

namespace SSW.SophieBot.Components.Models
{
    public class CacheState
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }
}
