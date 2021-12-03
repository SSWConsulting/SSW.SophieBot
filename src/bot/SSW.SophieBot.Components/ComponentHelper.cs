using Microsoft.Bot.Builder.Dialogs;
using System;

namespace SSW.SophieBot.Components
{
    public static class ComponentHelper
    {
        public static DateTime ToUserLocalTime(this DateTime serverTime, DialogContext dc)
        {
            serverTime = DateTime.SpecifyKind(serverTime, DateTimeKind.Utc);
            var utcOffset = dc.Context.Activity.LocalTimestamp.GetValueOrDefault().Offset;
            return serverTime.Add(utcOffset);
        }

        public static DateTime FromUserLocalTime(this DateTime userLocalTime, DialogContext dc)
        {
            var utcUserLocalTime = DateTime.SpecifyKind(userLocalTime, DateTimeKind.Utc);
            var utcOffset = dc.Context.Activity.LocalTimestamp.GetValueOrDefault().Offset;
            return utcUserLocalTime.Subtract(utcOffset);
        }

        public static DateTime GetDateByDayOfWeek(this DateTime date, DayOfWeek dayOfWeek)
        {
            int diff = dayOfWeek - date.DayOfWeek;

            if (dayOfWeek == DayOfWeek.Sunday)
            {
                diff += 7;
            }

            return date.AddDays(diff).Date;
        }

        public static string ToUserFriendlyDate(this DateTime dateTime, DateTime? now = null)
        {
            if (now.HasValue)
            {
                var dateOffset = (dateTime.Date - now.Value.Date).TotalDays;

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
                else if (dateTime.Year == now.Value.Year)
                {
                    return dateTime.ToString("ddd d MMM");
                }
            }

            return dateTime.ToString("ddd d MMM yyyy");
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
