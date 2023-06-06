using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Extensions.Logging;
using SSW.SophieBot.Projects;

namespace SSW.SophieBot.Entities
{
    [Model("projects")]
    public class Projects : IClosedList, IDisposable
    {
        private readonly ILuisService _luisService;
        private readonly IPeopleApiClient _peopleApiClient;
        private readonly ILogger<Projects> _logger;

        public ICollection<SubClosedList> SubLists { get; } = new List<SubClosedList>();

        public Projects(
            ILuisService luisService,
            IPeopleApiClient peopleApiClient,
            ILogger<Projects> logger)
        {
            _luisService = luisService;
            _peopleApiClient = peopleApiClient;
            _logger = logger;
        }

        public async IAsyncEnumerable<bool> SeedAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var clEntityId = await _luisService.GetClEntityIdAsync(ModelAttribute.GetName(typeof(Projects)), cancellationToken);

            if (!clEntityId.HasValue)
            {
                yield return false;
            }

            var crmProjectPages = _peopleApiClient.GetPagedCrmProjectsAsync();

            await foreach (var crmProjects in crmProjectPages)
            {
                _logger.LogInformation("Received crm projects from People API: {Count}", crmProjects.Count());

                var patchResponse = await _luisService.UpdateClosedListAsync(clEntityId.Value, ToUpdateObject(crmProjects), cancellationToken);
                patchResponse.EnsureSuccessOperationStatus();
            }

            yield return true;
        }

        public WordListObject CreateWordList(Project crmProject)
        {
            Check.NotNull(crmProject, nameof(crmProject));
            return new WordListObject(GetCanonicalForm(crmProject), GetSubList(crmProject));
        }

        public string GetCanonicalForm(Project crmProject)
        {
            return crmProject.ProjectId;
        }

        private IList<string> GetSubList(Project crmProject)
        {
            var nameList = new List<string>
            {
                crmProject.ProjectName,
            };

            return nameList;
        }

        private ClosedListModelUpdateObject ToUpdateObject(IEnumerable<Project> crmProjects)
        {
            var updateObject = new ClosedListModelUpdateObject(new List<WordListObject>());

            if (!crmProjects.IsNullOrEmpty())
            {
                foreach (var crmProject in crmProjects)
                {
                    updateObject.SubLists.Add(CreateWordList(crmProject));
                }
            }

            return updateObject;
        }

        public void Dispose()
        {
            _luisService.Dispose();
        }
    }
}