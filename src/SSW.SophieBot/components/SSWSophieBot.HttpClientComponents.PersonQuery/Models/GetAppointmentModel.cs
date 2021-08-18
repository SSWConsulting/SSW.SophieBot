using System;

namespace SSWSophieBot.HttpClientAction.Models
{
    public class GetAppointmentModel
    {
        public string AppointmentId { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public string Regarding { get; set; }
        public string Subject { get; set; }
    }
}
