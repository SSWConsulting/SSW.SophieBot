using Newtonsoft.Json;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class ProjectWithEmployeesCountModel
    { 
        [JsonProperty("crmProjectId")]
        public string CrmProjectId { get; set; }

        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        [JsonProperty("employeesCount")]
        public int EmployeesCount { get; set; }

        public ProjectWithEmployeesCountModel(string crmProjectId, string projectName, int employeeCount)
        {
            CrmProjectId = crmProjectId;
            ProjectName = projectName;
            EmployeesCount = employeeCount;
        }
    }
}
