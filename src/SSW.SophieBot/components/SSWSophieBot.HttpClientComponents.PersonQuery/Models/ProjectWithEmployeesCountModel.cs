using Newtonsoft.Json;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Models
{
    public class ProjectWithEmployeesCountModel
    {
        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        [JsonProperty("employeesCount")]
        public int EmployeesCount { get; set; }

        public ProjectWithEmployeesCountModel(string projectName, int employeeCount)
        {
            ProjectName = projectName;
            EmployeesCount = employeeCount;
        }
    }
}
