using Newtonsoft.Json;
using System.Collections.Generic;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeBillableItemModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("billedDays")]
        public int BilledDays { get; set; } // This is for a specific project in certain dialogs e.g. "GetEmployeesByProject"

        [JsonProperty("billedHours")]
        public int BilledHours { get; set; } // This is for a specific project in certain dialogs e.g. "GetEmployeesByProject"

        [JsonProperty("billedProjects")]
        public List<BilledProject> BilledProjects { get; set; } // This is for all projects in certain dialogs e.g. "GetEmployeesProject"

        [JsonProperty("bookingStatus")]
        public BookingStatus BookingStatus { get; set; }

        [JsonProperty("lastSeen")]
        public string LastSeen { get; set; }
    }

    public class BilledProject
    {
        [JsonProperty("projectId")]
        public string ProjectId { get; set; }

        [JsonProperty("billedDays")]
        public int BilledDays { get; set; }

        [JsonProperty("billedHours")]
        public int BilledHours { get; set; }

        [JsonProperty("projectName")]
        public string ProjectName { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("crmProjectId")]
        public string CrmProjectId { get; set; }
    }
}
