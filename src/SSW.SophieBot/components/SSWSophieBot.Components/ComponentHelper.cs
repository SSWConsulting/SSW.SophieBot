using Microsoft.Bot.Builder.Dialogs;
using System;

namespace SSWSophieBot.Components
{
    public static class ComponentHelper
    {
        public static DateTime ToUserLocalTime(this DateTime dateTime, DialogContext dc)
        {
            var serverLocalTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            var utcOffset = dc.Context.Activity.LocalTimestamp.GetValueOrDefault().Offset;
            return serverLocalTime.Subtract(utcOffset);
        }

        public static string ToUserFriendlyTime(this DateTime dateTime, DateTime now)
        {
            var dateOffset = (dateTime.Date - now.Date).TotalDays;

            if (dateOffset == 0)
            {
                return $"{dateTime:h:mm tt} Today";
            }
            else if (dateOffset == -1)
            {
                return $"{dateTime:h:mm tt} Yesterday";
            }
            else if (dateOffset == 1)
            {
                return $"{dateTime:h:mm tt} Tomorrow";
            }
            else
            {
                return dateTime.ToString("h:mm tt ddd d MMM");
            }
        }

        public static string ToUserFriendlyDuration(this TimeSpan duration)
        {
            var totalHours = (int)Math.Ceiling(duration.TotalHours);
            if (totalHours > 9)
            {
                var totalDays = (int)Math.Ceiling(duration.TotalDays);
                return $"{totalDays} days";
            }
            else
            {
                return $"{totalHours} hours";
            }
        }
    }
}
