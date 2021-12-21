using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SSW.SophieBot.Sync
{
    public class OdataResponse<T>
    {
        public T Value { get; set; }
    }

    public class OdataPagedResponse<T> : OdataResponse<List<T>>
    {
        [JsonPropertyName("@odata.nextLink")]
        public string OdataNextLink { get; set; }

        [JsonIgnore]
        public bool HasMoreResults => !string.IsNullOrEmpty(OdataNextLink);

        public OdataPagedResponse()
        {
            Value = new List<T>();
        }
    }
}
