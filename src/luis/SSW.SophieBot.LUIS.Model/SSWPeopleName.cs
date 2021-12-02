using SSW.SophieBot.Persistence;
using System;

namespace SSW.SophieBot.LUIS.Model
{
    public class SSWPeopleName
    {
        public string UserId { get; set; } = string.Empty;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Nickname { get; set; }

        public SyncMode SyncMode { get; set; }
    }
}
