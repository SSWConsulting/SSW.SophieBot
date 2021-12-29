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

        public static string GetUserFriendlyTimeInterval(this DateTime sourceDate, DateTime targetDate)
        {
            var intervalDays = (int)Math.Ceiling((targetDate - sourceDate).TotalDays);
            if(intervalDays < 5)
            {
                return string.Empty;
            }

            var intervalWeeks = (int)Math.Ceiling(intervalDays / 7d);
            return $"approximately {intervalWeeks} week{(intervalWeeks > 1 ? "s" : string.Empty)}";
        }

        public static string ToUserFriendlyDate(this DateTime dateTime, DateTime? now = null, bool dayOfWeek = true)
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
                    return $"{(dayOfWeek ? $"{dateTime:ddd d}" : $"{dateTime.Day}")}{GetDateSuffix(dateTime)} {dateTime:MMM}";
                }
            }

            return $"{(dayOfWeek ? $"{dateTime:ddd d}" : $"{dateTime.Day}")}{GetDateSuffix(dateTime)} {dateTime:MMM yyyy}";
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

        private static string GetDateSuffix(DateTime date)
        {
            return (date.Day % 10 == 1 && date.Day % 100 != 11) ? "st"
            : (date.Day % 10 == 2 && date.Day % 100 != 12) ? "nd"
            : (date.Day % 10 == 3 && date.Day % 100 != 13) ? "rd"
            : "th";
        }
    }
}
