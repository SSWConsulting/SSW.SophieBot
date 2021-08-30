namespace SSWSophieBot.HttpClientAction.Models
{
    public class GetSkillModel
    {
        public string Technology { get; set; }
        public string ExperienceLevel { get; set; }
        public int SortOrder { get; set; }
    }

    public enum ExperienceLevel
    {
        Intermediate,
        Advanced
    }
}
