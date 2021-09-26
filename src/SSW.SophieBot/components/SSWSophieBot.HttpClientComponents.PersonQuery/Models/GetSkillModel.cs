using System;

namespace SSWSophieBot.HttpClientAction.Models
{
    public class GetSkillModel
    {
        public string Technology { get; set; }
        public string ExperienceLevel { get; set; }
        public int SortOrder { get; set; }
    }

    [Flags]
    public enum ExperienceLevel
    {
        Intermediate = 1,
        Advanced = 2
    }
}
