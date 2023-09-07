using Microsoft.Bot.Builder.Dialogs;
using SSW.SophieBot.Components;
using SSW.SophieBot.HttpClientComponents.PersonQuery;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot.HttpClientAction.Models
{
    public class GetEmployeeModel
    {
        public string OrganisationId { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string PersonalEmailAddress { get; set; }
        public string Birthdate { get; set; }
        public string MobilePhone { get; set; }
        public bool InOffice { get; set; }
        public bool IsActive { get; set; }
        public bool IsDuplicateFirstname { get; set; }
        public GetLocationModel DefaultSite { get; set; }
        public List<GetDeviceModel> Devices { get; set; }
        public List<GetAppointmentModel> Appointments { get; set; }
        public List<GetAppointmentModel> NormalizedAppointments { get; set; }
        public List<GetEmployeeProjectModel> Projects { get; set; }
        public List<GetEmployeeProjectModel> CurrentProjects { get; set; }
        public GetLastSeenAtSiteModel LastSeenAt { get; set; }
        public List<GetSkillModel> Skills { get; set; }
        public double YtdBillableHours { get; set; }
        public double YtdLeaveHours { get; set; }
        public double YtdTotalWorkHours { get; set; }
        public string AvatarUrl { get; set; }
        public string EmployeePublicProfileUrl { get; set; }
        public string Nickname { get; set; }
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
        public string WorkHours { get; set; }
        public string Title { get; set; }
        public ProfileCategory? ProfileCategory { get; set; }

        public bool HasSkill(string technology, ExperienceLevel? experienceLevel = ExperienceLevel.Advanced | ExperienceLevel.Intermediate)
        {
            if (Skills == null || !Skills.Any())
            {
                return false;
            }

            return Skills.Any(s =>
            {
                if (string.IsNullOrWhiteSpace(s.Technology) || !s.Technology.Equals(technology, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                if (experienceLevel.HasValue)
                {
                    if (Enum.TryParse<ExperienceLevel>(s.ExperienceLevel, true, out var levelEnum))
                    {
                        return experienceLevel.Value.HasFlag(levelEnum);
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        public void NormalizeAppointments(DialogContext dc)
        {
            NormalizedAppointments = Appointments.Select(appointment =>
            {
                var normalizedAppointment = appointment.Clone();

                if (!normalizedAppointment.IsAllDay())
                {
                    normalizedAppointment.Start = appointment.Start.DateTime.ToUserLocalTime(dc);
                    normalizedAppointment.End = appointment.End.DateTime.ToUserLocalTime(dc);
                }

                return normalizedAppointment;
            })
            .ToList();
        }
    }
}
