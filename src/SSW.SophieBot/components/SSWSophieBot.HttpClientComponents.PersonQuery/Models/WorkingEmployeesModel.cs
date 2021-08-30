using Newtonsoft.Json;
using System.Collections.Generic;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class WorkingEmployeesModel
    {
        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("employees")]
        public List<WorkingEmployeeItem> Employees { get; set; }
    }

    public class WorkingEmployeeItem
    {
        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("billedDays")]
        public int BilledDays { get; set; }

        [JsonProperty("lastSeen")]
        public string LastSeen { get; set; }
    }
}
