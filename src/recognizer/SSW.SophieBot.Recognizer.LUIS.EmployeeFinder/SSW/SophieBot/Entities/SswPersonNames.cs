using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
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
        private readonly PeopleApiClient _peopleApiClient;
        private readonly LuisOptions _luisOptions;

        public ICollection<SubClosedList> SubLists { get; } = new List<SubClosedList>();

        public SswPersonNames(
            ILUISAuthoringClient luisAuthoringClient,
            PeopleApiClient peopleApiClient,
            IOptions<LuisOptions> luisOptions)
        {
            _luisAuthoringClient = luisAuthoringClient;
            _peopleApiClient = peopleApiClient;
            _luisOptions = luisOptions.Value;
        }

        public async IAsyncEnumerable<bool> SeedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var (appId, activeVersion) = await LuisHelper.GetLuisAppIdAndActiveVersionAsync(_luisAuthoringClient, _luisOptions, cancellationToken);

            var createObject = LuisHelper.GetClEntityCreateObject(this);
            var clEntityId = await _luisAuthoringClient.EnsureClEntityExistAsync(appId, activeVersion, createObject, cancellationToken);
            var employeePages = _peopleApiClient.GetPagedEmployeesAsync();

            await foreach (var employees in employeePages)
            {
                var patchResponse = await _luisAuthoringClient.Model.PatchClosedListAsync(
                    appId,
                    activeVersion,
                    clEntityId,
                    ToPatchObject(employees),
                    cancellationToken);

                patchResponse.EnsureSuccessOperationStatus();
            }

            yield return true;
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

        public virtual WordListObject CreateWordList(Employee employee)
        {
            Check.NotNull(employee, nameof(employee));
            return new WordListObject(GetCanonicalForm(employee), GetSubList(employee));
        }

        private string GetCanonicalForm(Employee employee)
        {
            return employee.UserId;
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
