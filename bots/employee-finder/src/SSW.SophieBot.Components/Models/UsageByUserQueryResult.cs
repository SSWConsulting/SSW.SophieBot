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
                        var newItem = new UsageByUserQueryResultItem(userName, usageCount);
                        var duplication = Find(item => item.Equals(newItem));

                        if (duplication != null)
                        {
                            duplication.UsageCount += newItem.UsageCount;
                        }
                        else
                        {
                            Add(newItem);
                        }
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

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("usageCount")]
        public int UsageCount { get; set; }

        public UsageByUserQueryResultItem(string userName, int usageCount)
        {
            UserName = userName;
            UsageCount = usageCount;
            SetFirstNameAndLastName(userName);
        }

        public void SetFirstNameAndLastName(string fullName)
        {
            fullName = fullName.Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                return;
            }

            var sections = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (sections.Length > 0)
            {
                FirstName = sections[0];
            }

            if (sections.Length > 1)
            {
                LastName = sections[1];
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is UsageByUserQueryResultItem resultItem)
            {
                return FirstName == resultItem.FirstName && LastName == resultItem.LastName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return $"{FirstName}-{LastName}".GetHashCode();
        }
    }
}
