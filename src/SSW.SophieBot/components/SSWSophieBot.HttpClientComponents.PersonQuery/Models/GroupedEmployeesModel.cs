using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class GroupedEmployeesModel
    {
        private readonly int _countPerSet = 0;

        [JsonProperty("sets")]
        public List<GroupedEmployeesSet> Sets { get; set; }

        public GroupedEmployeesModel(int countPerSet)
        {
            _countPerSet = countPerSet;
            Sets = new List<GroupedEmployeesSet>();
        }

        public void AddItem(GroupedEmployeesItem item)
        {
            if (item == null)
            {
                return;
            }

            if (!Sets.Any())
            {
                var newSet = new GroupedEmployeesSet();
                newSet.Groups.Add(item);
                Sets.Add(newSet);
            }
            else
            {
                var lastSet = Sets.Last();
                if (lastSet.Groups.Count < _countPerSet)
                {
                    lastSet.Groups.Add(item);
                }
                else
                {
                    var newSet = new GroupedEmployeesSet();
                    newSet.Groups.Add(item);
                    Sets.Add(newSet);
                }
            }
        }
    }

    public class GroupedEmployeesSet
    {
        [JsonProperty("groups")]
        public List<GroupedEmployeesItem> Groups { get; set; }

        public GroupedEmployeesSet()
        {
            Groups = new List<GroupedEmployeesItem>();
        }
    }

    public class GroupedEmployeesItem
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
