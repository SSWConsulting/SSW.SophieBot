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
                return $"Today";
            }
            else if (dateOffset == -1)
            {
                return $"Yesterday";
            }
            else if (dateOffset == 1)
            {
                return $"Tomorrow";
            }
            else if (dateTime.Year == now.Year)
            {
                return dateTime.ToString("ddd d MMM");
            }
            else
            {
                return dateTime.ToString("ddd d MMM yyyy");
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
