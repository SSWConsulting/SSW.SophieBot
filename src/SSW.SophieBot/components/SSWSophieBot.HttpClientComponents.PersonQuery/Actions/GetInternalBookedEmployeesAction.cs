﻿using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSWSophieBot.Components;
using SSWSophieBot.Components.Actions;
using SSWSophieBot.HttpClientAction.Models;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSWSophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetInternalBookedEmployeesAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetInternalBookedEmployeesAction";

        public GetInternalBookedEmployeesAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
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
            var employees = dc.GetValue(Employees);
            var dateString = dc.GetValue(Date);

            var date = dateString != null && dateString != ""
                ? ComponentHelper.ToUserLocalTime(dc, DateTime.Parse(dateString)).AddHours(9)
                : DateTime.Now.ToUniversalTime();

            var result = EmployeesHelper.GetInternalBookedEmployees(EmployeesHelper.FilterDevelopers(employees), date);


            if (Result != null)
            {
                dc.State.SetValue(Result.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
