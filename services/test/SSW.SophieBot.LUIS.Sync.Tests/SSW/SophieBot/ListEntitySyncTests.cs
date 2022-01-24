using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SSW.SophieBot.Entities;
using SSW.SophieBot.LUIS.Sync.Functions;
using SSW.SophieBot.Persistence;
using System.Linq;
using Xunit;

namespace SSW.SophieBot
{
    public class ListEntitySyncTests
    {
        private readonly ILuisService _luisService;
        private readonly TestData _testData;
        private readonly PersonNames _personNamesClEntity;
        private readonly ListEntitySync _listEntitySync;

        public ListEntitySyncTests()
        {
            _testData = new TestData();
            var testLuisOptions = new TestLuisOptions();
            var luisAuthoringClient = new TestLuisAuthoringClient(_testData);
            _luisService = new LuisService(luisAuthoringClient, null, testLuisOptions);

            _personNamesClEntity = new TestPersonNamesClEntity(
                _luisService,
                new TestPeopleClient(),
                NullLogger<PersonNames>.Instance);
            _listEntitySync = new ListEntitySync(
                _luisService,
                _personNamesClEntity,
                NullLogger<ListEntitySync>.Instance);
        }

        [Fact]
        public void Should_Have_Old_Data_For_Test()
        {
            // Assert
            _testData.PersonNames.SubLists[0].List.ShouldContain(_testData.MqEmployees[0].Message.FullName);
            _testData.PersonNames.SubLists[1].List.ShouldNotContain(_testData.MqEmployees[1].Message.FullName);
        }

        [Fact]
        public async void Should_Correctly_Update_SubLists()
        {
            // Arrange
            var upsertCanonicalForms = _testData.MqEmployees
                .Where(mqEmployee => mqEmployee.SyncMode != SyncMode.Delete)
                .Select(mqEmployee =>
                    _personNamesClEntity.GetCanonicalForm(mqEmployee.Message));

            //Act
            await _listEntitySync.SyncPersonNames(_testData.MqEmployees, default);
            var newUpsertCanonicalForms = _testData.PersonNames.SubLists.Select(subList => subList.CanonicalForm);

            // Assert
            newUpsertCanonicalForms.Count().ShouldBe(3);
            upsertCanonicalForms.ShouldAllBe(form => newUpsertCanonicalForms.Contains(form));
        }
    }
}