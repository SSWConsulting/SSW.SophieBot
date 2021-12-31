using Microsoft.Azure.ApplicationInsights.Query.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot.Components.Models
{
    public class UsageByUserQueryResult : List<UsageByUserQueryResultItem>
    {
        public UsageByUserQueryResult()
            : base()
        {

        }

        public UsageByUserQueryResult(QueryResults queryResults, string userNameKey = "user", string usageCountKey = "usageCount")
            : base()
        {
            var results = queryResults?.Results ?? throw new ArgumentNullException(nameof(queryResults));
            foreach (var row in results)
            {
                if (row.ContainsKey(userNameKey) && row.ContainsKey(usageCountKey))
                {
                    if (row[userNameKey] is string userName && int.TryParse(Convert.ToString(row[usageCountKey]), out var usageCount))
                    {
                        Add(new UsageByUserQueryResultItem(userName, usageCount));
                    }
                }
            }
        }

        public UsageByUserQueryResult(IEnumerable<UsageByUserQueryResultItem> items)
            : base(items)
        {

        }

        public UsageByUserQueryResult OrderByUsageCountDesc()
        {
            return new UsageByUserQueryResult(this.OrderByDescending(item => item.UsageCount));
        }
    }

    public class UsageByUserQueryResultItem
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("usageCount")]
        public int UsageCount { get; set; }

        public UsageByUserQueryResultItem(string userName, int usageCount)
        {
            UserName = userName;
            UsageCount = usageCount;
        }
    }
}
