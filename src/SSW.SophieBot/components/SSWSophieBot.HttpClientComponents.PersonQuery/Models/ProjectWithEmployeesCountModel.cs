using Newtonsoft.Json;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class ProjectWithEmployeesCountModel
    {
        [JsonProperty("crmId")]
        public string CrmId { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("employeesCount")]
        public int EmployeesCount { get; set; }

        public ProjectWithEmployeesCountModel(string crmId, string displayName, int employeeCount)
        {
            CrmId = crmId;
            DisplayName = displayName;
            EmployeesCount = employeeCount;
        }
    }
}
