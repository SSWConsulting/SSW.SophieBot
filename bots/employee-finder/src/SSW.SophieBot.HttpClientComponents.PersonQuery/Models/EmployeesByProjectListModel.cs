using Newtonsoft.Json;
using System.Collections.Generic;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeesByProjectListModel
    {
        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("employees")]
        public List<EmployeeBillableItemModel> Employees { get; set; }
    }
}
