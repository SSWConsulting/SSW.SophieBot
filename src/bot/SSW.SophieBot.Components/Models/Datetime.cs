using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SSW.SophieBot.Components.Models
{
    public class Datetime
    {
        [JsonProperty("timex")]
        public List<string> Timex { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
