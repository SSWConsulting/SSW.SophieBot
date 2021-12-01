using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SSW.SophieBot.DataSync.Domain
{
    public class OdataResponse<T>
    {
        public T Value { get; set; }
    }

    public class OdataPagedResponse<T> : OdataResponse<List<T>>
    {
        [JsonPropertyName("@odata.nextLink")]
        public string OdataNextLink { get; set; }

        public OdataPagedResponse()
        {
            Value = new List<T>();
        }

        public bool HasNext() => !string.IsNullOrEmpty(OdataNextLink) && Value.Any();
    }
}
