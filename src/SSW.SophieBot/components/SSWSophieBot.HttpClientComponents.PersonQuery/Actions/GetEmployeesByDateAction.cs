﻿using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.Components.Actions;
using SSWSophieBot.HttpClientAction.Models;
using SSWSophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetEmployeesByDateAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetEmployeesByDateAction";

        public GetEmployeesByDateAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("date")]
        public StringExpression Date { get; set; }

        [JsonProperty("result")]
        public StringExpression Result { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            static GetAppointmentModel GetAppointmentBy(DateTime date, List<GetAppointmentModel> appointments)
            {
                var leavePhrases = new string[] { "annual leave", "non working", "non-working", "leave", "holiday", "time in lieu", "hour leave", "hours leave", "day off", "days off" };
                var results = appointments
                    .Where(appointment => date.Ticks >= GetTicksFrom(appointment.Start) && date.Ticks <= GetTicksFrom(appointment.End))
                    .Where(appointment => !leavePhrases.Any(appointment.Subject.ToLower().Contains))
                    .ToList();
                return results.Count != 0 ? results[0] : null;
            }

            static long GetTicksFrom(DateTimeOffset date)
            {
                return date.UtcDateTime.Ticks;
            }

            DateTime ToUserLocalTime(DateTime dateTime)
            {
                var serverLocalTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                var utcOffset = dc.Context.Activity.LocalTimestamp.GetValueOrDefault().Offset;
                return serverLocalTime.Subtract(utcOffset);
            }

            var employees = dc.GetValue(Employees);
            var dateString = dc.GetValue(Date);

            var date = dateString != null && dateString != "" ? ToUserLocalTime(DateTime.Parse(dateString)).AddHours(9) : DateTime.Now.ToUniversalTime();

            var result = employees.Select(e => new EmployeeByDateModel
            {
                FirstName = e.FirstName,
                DefaultSite = e.DefaultSite,
                AvatarUrl = e.AvatarUrl,
                DisplayName = $"{e.FirstName} {e.LastName}",
                CurrentAppointment = GetAppointmentBy(date, e.Appointments),
                Title = e.Title,
            })
                .Where(e => e.CurrentAppointment != null)
                .ToList();


            if (Result != null)
            {
                dc.State.SetValue(Result.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
