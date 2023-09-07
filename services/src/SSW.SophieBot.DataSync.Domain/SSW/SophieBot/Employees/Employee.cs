using SSW.SophieBot.Persistence;
using System;
using System.Collections.Generic;

namespace SSW.SophieBot.Employees
{
    public class Employee
    {
        public string UserId { get; set; }
        public string EmployeeCode { get; set; }
        public string OrganisationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string Birthdate { get; set; }
        public bool IsActive { get; set; }
        public string MobilePhone { get; set; }
        public Location DefaultSite { get; set; }
        public List<Device> Devices { get; set; }
        public List<EmployeeProject> Projects { get; set; }
        public LastSeenAtSite LastSeenAt { get; set; }
        public bool InOffice { get; set; }
        public double YtdBillableHours { get; set; }
        public double YtdLeaveHours { get; set; }
        public double YtdTotalWorkHours { get; set; }
        public string AvatarUrl { get; set; }
        public string EmployeePublicProfileUrl { get; set; }
        public double BillableRate { get; set; }
        public string BlogUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string SkypeUsername { get; set; }
        public string LinkedInUrl { get; set; }
        public string TwitterUsername { get; set; }
        public string GitHubUrl { get; set; }
        public string YouTubePlayListId { get; set; }
        public DateTime? EstimatedArrivalTime { get; set; }
        public string AboutMeAudioUrl { get; set; }
        public string PublicPhotoAlbumUrl { get; set; }
        public int? EmployeeCategory { get; set; }
        public string Title { get; set; }
        public int? ProfileCategory { get; set; }
        public string WorkHours { get; set; }

        public Employee()
        {

        }

        public Employee(string userId, string organisationId)
        {
            UserId = userId;
            OrganisationId = organisationId;
        }

        public static Employee FromCrmEmployee(CrmEmployee crmEmployee)
        {
            return new Employee
            {
                UserId = crmEmployee.Systemuserid,
                OrganisationId = crmEmployee.Organizationid,
                FirstName = crmEmployee.Firstname,
                LastName = crmEmployee.Lastname,
                NickName = crmEmployee.Nickname,
                FullName = crmEmployee.Fullname
            };
        }

        public SyncSnapshot ToSnapshot(DateTime modifiedOn, string syncVersion)
        {
            return new SyncSnapshot
            {
                Id = UserId,
                OrganizationId = OrganisationId,
                Modifiedon = modifiedOn,
                SyncVersion = syncVersion,
            };
        }
    }
}
