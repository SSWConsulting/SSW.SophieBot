using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.SophieBot.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SSW.SophieBot.Entities
{
    public class SswPersonNames : IClosedList, IDisposable
    {
        private readonly ILUISAuthoringClient _luisAuthoringClient;
        private readonly IPeopleApiClient _peopleApiClient;
        private readonly LuisOptions _luisOptions;
        private readonly ILogger<SswPersonNames> _logger;

        public ICollection<SubClosedList> SubLists { get; } = new List<SubClosedList>();

        public SswPersonNames(
            ILUISAuthoringClient luisAuthoringClient,
            IPeopleApiClient peopleApiClient,
            IOptions<LuisOptions> luisOptions,
            ILogger<SswPersonNames> logger)
        {
            _luisAuthoringClient = luisAuthoringClient;
            _peopleApiClient = peopleApiClient;
            _luisOptions = luisOptions.Value;
            _logger = logger;
        }

        public async IAsyncEnumerable<bool> SeedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var (appId, activeVersion) = await LuisHelper.GetLuisAppIdAndActiveVersionAsync(_luisAuthoringClient, _luisOptions, cancellationToken);

            var clEntityId = await _luisAuthoringClient.GetClEntityIdAsync(
                appId,
                activeVersion,
                ModelAttribute.GetName(typeof(SswPersonNames)),
                cancellationToken);

            if (!clEntityId.HasValue)
            {
                yield return false;
            }

            var employeePages = _peopleApiClient.GetPagedEmployeesAsync();

            await foreach (var employees in employeePages)
            {
                _logger.LogInformation("Received employees from People API: {Count}", employees.Count());

                var patchResponse = await _luisAuthoringClient.Model.PatchClosedListAsync(
                    appId,
                    activeVersion,
                    clEntityId.Value,
                    ToPatchObject(employees),
                    cancellationToken);

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

        private ClosedListModelPatchObject ToPatchObject(IEnumerable<Employee> employees)
        {
            var patchObject = new ClosedListModelPatchObject(new List<WordListObject>());

            if (!employees.IsNullOrEmpty())
            {
                foreach (var employee in employees)
                {
                    patchObject.SubLists.Add(CreateWordList(employee));
                }
            }

            return patchObject;
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
            _luisAuthoringClient.Dispose();
        }
    }
}
