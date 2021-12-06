using System;

namespace SSW.SophieBot
{
    public class LuisOptions
    {
        public string AuthoringEndpoint { get; set; }

        public string AuthoringKey { get; set; }

        public string AppId { get; set; }

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
