using Microsoft.Bot.Builder.Dialogs;
using System;

namespace SSWSophieBot.Components
{
    public static class ComponentHelper
    {
        public static DateTime ToUserLocalTime(DialogContext dc, DateTime dateTime)
        {
            var serverLocalTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            var utcOffset = dc.Context.Activity.LocalTimestamp.GetValueOrDefault().Offset;
            return serverLocalTime.Subtract(utcOffset);
        }
    }
}
