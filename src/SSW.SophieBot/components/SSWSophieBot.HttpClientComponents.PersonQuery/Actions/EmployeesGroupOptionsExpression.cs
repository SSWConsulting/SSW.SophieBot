using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class EmployeesGroupOptionsExpression
    {
        [JsonProperty("showAll")]
        public BoolExpression ShowAll { get; set; }

        [JsonProperty("groupKey")]
        public StringExpression GroupKey { get; set; }

        [JsonProperty("countPerSet")]
        public IntExpression CountPerSet { get; set; }

        [JsonProperty("maxGroupCount")]
        public IntExpression MaxGroupCount { get; set; }

        public EmployeesGroupOptionsExpression()
        {
            ShowAll = new BoolExpression(false);
            GroupKey = new StringExpression(string.Empty);
            CountPerSet = new IntExpression(6);
            MaxGroupCount = new IntExpression(int.MaxValue);
        }
    }
}
