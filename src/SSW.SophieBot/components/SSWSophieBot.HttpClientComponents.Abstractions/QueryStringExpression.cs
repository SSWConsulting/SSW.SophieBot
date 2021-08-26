using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using System;
using System.Collections.Generic;

namespace SSWSophieBot.HttpClientComponents.Abstractions
{
    public class QueryStringExpression
    {
        [JsonProperty("key")]
        public StringExpression Key { get; set; }

        [JsonProperty("value")]
        public ValueExpression Value { get; set; }

        public KeyValuePair<string, string> GetQueryStrings(DialogContext dc)
        {
            return new KeyValuePair<string, string>(dc.GetValue(Key), Convert.ToString(dc.GetValue(Value)));
        }
    }
}
