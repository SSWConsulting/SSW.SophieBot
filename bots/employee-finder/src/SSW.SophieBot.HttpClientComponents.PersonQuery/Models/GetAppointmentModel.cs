using System;

namespace SSW.SophieBot.HttpClientAction.Models
{
    public class GetAppointmentModel
    {
        public string AppointmentId { get; set; }
        public string[] RequiredAttendees { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public string Regarding { get; set; }
        public string Subject { get; set; }

        public GetAppointmentModel Clone()
        {
            return new GetAppointmentModel
            {
                AppointmentId = AppointmentId,
                RequiredAttendees = RequiredAttendees,
                Start = Start,
                End = End,
                Regarding = Regarding,
                Subject = Subject
            };
        }

        public bool IsAllDay()
        {
            return Start == Start.Date && End == End.Date;
        }
    }
}
