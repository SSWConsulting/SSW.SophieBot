using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using SSW.SophieBot.Components;
using SSW.SophieBot.Components.Actions;
using SSW.SophieBot.HttpClientAction.Models;
using SSW.SophieBot.HttpClientComponents.PersonQuery.Models;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot.HttpClientComponents.PersonQuery.Actions
{
    public class GetEmployeesWithFreeDateAction : ActionBase
    {
        [JsonProperty("$kind")]
        public const string Kind = "GetEmployeesWithFreeDateAction";

        public GetEmployeesWithFreeDateAction([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {

        }

        [JsonProperty("isFree")]
        public BoolExpression IsFree { get; set; }

        [JsonProperty("employees")]
        public ArrayExpression<GetEmployeeModel> Employees { get; set; }

        [JsonProperty("result")]
        public StringExpression Result { get; set; }

        [JsonProperty("isFreeForXDays")]
        public IntExpression IsFreeForXDays { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var isFree = dc.GetValue(IsFree);
            var employees = dc.GetValue(Employees);
            var isFreeForXDays = dc.GetValue(IsFreeForXDays);
            bool isFreeForXDaysFlag = false;

            var date = DateTime.Now.ToUserLocalTime(dc);

            var result = employees.Select(e =>
            {
                e.NormalizeAppointments(dc);
                DateTime? rawFreeDate = isFree
                    ? EmployeesHelper.GetFreeDate(e.Appointments, date)
                    : EmployeesHelper.GetNextUnavailability(e, date, out var _, false)?.Date;

                if (isFreeForXDays > 0)
                {
                    do
                    {
                        var firstFreeDay = EmployeesHelper.GetFreeDate(e.Appointments, date);
                        if (isFreeForXDays == 1) { isFreeForXDaysFlag = true; }
                        else
                        {
                            var nextDay = firstFreeDay;
                            for (int i = 1; i < isFreeForXDays; i++)
                            {
                                nextDay = nextDay.AddDays(1);
                                if (nextDay.DayOfWeek == DayOfWeek.Saturday) { nextDay = nextDay.AddDays(2); }
                                if (nextDay.DayOfWeek == DayOfWeek.Sunday) { nextDay = nextDay.AddDays(1); }
                                if (EmployeesHelper.IsFree(e, nextDay))
                                {
                                    isFreeForXDaysFlag = true;
                                }
                                else
                                {
                                    date = nextDay;
                                    isFreeForXDaysFlag = false;
                                    break;
                                }
                            }
                        }
                        rawFreeDate = firstFreeDay;
                    } while (!isFreeForXDaysFlag);
                    date = DateTime.Now.ToUserLocalTime(dc);
                }

                var resultModel = new EmployeeWithFreeDateModel
                {
                    DisplayName = $"{e.FirstName} {e.LastName}",
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    IsFreeForXDaysFlag = isFreeForXDaysFlag,
                    BookedDays = EmployeesHelper.GetBookedDays(e, date),
                };

                if (rawFreeDate.HasValue)
                {
                    resultModel.FreeDate = rawFreeDate.Value.ToUserFriendlyDate(date);
                    resultModel.TimeDuration = EmployeesHelper.GetFormatedTimeDuration(date.Date, rawFreeDate.Value);
                }

                return resultModel;
            })
            .ToList();

            if (Result != null)
            {
                dc.State.SetValue(Result.GetValue(dc.State), result);
            }

            return await dc.EndDialogAsync(result: result, cancellationToken: cancellationToken);
        }
    }
}
