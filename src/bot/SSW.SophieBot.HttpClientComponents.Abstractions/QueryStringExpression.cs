using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSW.SophieBot.Components;
using System;
using System.Collections.Generic;

namespace SSW.SophieBot.HttpClientComponents.Abstractions
{
    public class QueryStringExpression
    {
        [JsonProperty("key")]
        public StringExpression Key { get; set; }

        [JsonProperty("value")]
        public ValueExpression Value { get; set; }

        public KeyValuePair<string, string>? GetQueryStrings(DialogContext dc)
        {
            var key = dc.GetValue(Key);
            var value = Convert.ToString(dc.GetValue(Value));

            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
            {
                return new KeyValuePair<string, string>(dc.GetValue(Key), Convert.ToString(dc.GetValue(Value)));
            }
            else
            {
                return null;
            }
        }
    }
}
