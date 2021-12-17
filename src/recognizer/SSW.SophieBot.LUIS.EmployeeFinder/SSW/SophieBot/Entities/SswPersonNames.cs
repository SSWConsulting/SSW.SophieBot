using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Extensions.Logging;
using SSW.SophieBot.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SSW.SophieBot.Entities
{
    [Model("sswPersonNames")]
    public class SswPersonNames : IClosedList, IDisposable
    {
        private readonly ILuisService _luisService;
        private readonly IPeopleApiClient _peopleApiClient;
        private readonly ILogger<SswPersonNames> _logger;

        public ICollection<SubClosedList> SubLists { get; } = new List<SubClosedList>();

        public SswPersonNames(
            ILuisService luisService,
            IPeopleApiClient peopleApiClient,
            ILogger<SswPersonNames> logger)
        {
            _luisService = luisService;
            _peopleApiClient = peopleApiClient;
            _logger = logger;
        }

        public async IAsyncEnumerable<bool> SeedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var clEntityId = await _luisService.GetClEntityIdAsync(ModelAttribute.GetName(typeof(SswPersonNames)), cancellationToken);

            if (!clEntityId.HasValue)
            {
                yield return false;
            }

            var employeePages = _peopleApiClient.GetPagedEmployeesAsync();

            await foreach (var employees in employeePages)
            {
                _logger.LogInformation("Received employees from People API: {Count}", employees.Count());

                var patchResponse = await _luisService.UpdateClosedListAsync(clEntityId.Value, ToUpdateObject(employees), cancellationToken);
                patchResponse.EnsureSuccessOperationStatus();
            }

            yield return true;
        }

        public virtual WordListObject CreateWordList(Employee employee)
        {
            Check.NotNull(employee, nameof(employee));
            return new WordListObject(GetCanonicalForm(employee), GetSubList(employee));
        }

        public virtual string GetCanonicalForm(Employee employee)
        {
            return employee.UserId;
        }

        private ClosedListModelUpdateObject ToUpdateObject(IEnumerable<Employee> employees)
        {
            var updateObject = new ClosedListModelUpdateObject(new List<WordListObject>());

            if (!employees.IsNullOrEmpty())
            {
                foreach (var employee in employees)
                {
                    updateObject.SubLists.Add(CreateWordList(employee));
                }
            }

            return updateObject;
        }

        private IList<string> GetSubList(Employee employee)
        {
            var fullName = employee.FullName.IsNullOrWhiteSpace() ? $"{employee.FirstName} {employee.LastName}" : employee.FullName;
            var nameList = new List<string>
            {
                employee.FirstName,
                employee.LastName,
                $"{employee.FirstName}s",
                fullName,
                $"{fullName}s"
            };

            if (!employee.NickName.IsNullOrWhiteSpace())
            {
                nameList.Add(employee.NickName);
                nameList.Add($"{employee.NickName}s");
            }

            return nameList.Distinct().ToList();
        }

        public void Dispose()
        {
            _luisService.Dispose();
        }
    }
}
