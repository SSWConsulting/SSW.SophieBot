using System;

namespace SSW.SophieBot
{
    public class LuisOptions
    {
        public string AuthoringEndpoint { get; set; } = string.Empty;

        public string AuthoringKey { get; set; } = string.Empty;

        public string AppId { get; set; } = string.Empty;

        public Guid GetGuidAppId()
        {
            if (AppId.IsNullOrWhiteSpace())
            {
                return Guid.Empty;
            }

            try
            {
                return new Guid(AppId);
            }
            catch
            {
                return Guid.Empty;
            }
        }
    }
}
