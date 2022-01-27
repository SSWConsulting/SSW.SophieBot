using Newtonsoft.Json;
using SSW.SophieBot.HttpClientAction.Models;
using System.Collections.Generic;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Models
{
    public class EmployeeProfileWithStatusModel
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("avatarUrl")]
        public string AvatarUrl { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("clients")]
        public List<EmployeeProfileClient> Clients { get; set; }

        [JsonProperty("bookingStatus")]
        public BookingStatus BookingStatus { get; set; }

        [JsonProperty("lastSeenAt")]
        public GetLastSeenAtSiteModel LastSeenAt { get; set; }

        [JsonProperty("lastSeenTime")]
        public string LastSeenTime { get; set; }

        [JsonProperty("skills")]
        public List<GetSkillModel> Skills { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("mobilePhone")]
        public string MobilePhone { get; set; }

        [JsonProperty("defaultSite")]
        public GetLocationModel DefaultSite { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("billableRate")]
        public double BillableRate { get; set; }

        [JsonProperty("bookedDays")]
        public int BookedDays { get; set; }

        [JsonProperty("additionalStatus")]
        public string AdditionalStatus { get; set; }

        [JsonProperty("appointments")]
        public List<EmployeeProfileAppointment> Appointments { get; set; }
    }

    public class EmployeeProfileAppointment
    {
        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("bookingStatus")]
        public BookingStatus BookingStatus { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("regarding")]
        public string Regarding { get; set; }

        [JsonProperty("additionalStatus")]
        public string AdditionalStatus { get; set; }
    }

    public class EmployeeProfileClient
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("accountManager")]
        public string AccountManager { get; set; }
    }
}
