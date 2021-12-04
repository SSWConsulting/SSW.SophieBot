using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using SSW.SophieBot.Entities;
using SSW.SophieBot.LUIS.Sync.Functions;
using SSW.SophieBot.LUIS.Sync.Test.Data;
using System.Linq;
using Xunit;

namespace SSW.SophieBot.LUIS.Sync.Test
{
    public class ListEntitySyncTests
    {
        private readonly ILUISAuthoringClient _luisAuthoringClient;
        private readonly TestData _testData;
        private readonly SswPeopleNames _sswPeopleNamesClEntity;
        private readonly ListEntitySync _listEntitySync;

        public ListEntitySyncTests()
        {
            _testData = new TestData();
            _luisAuthoringClient = new TestLUISAuthoringClient(_testData);
            _sswPeopleNamesClEntity = new TestSswPeopleNamesClEntity();
            _listEntitySync = new ListEntitySync(
                _luisAuthoringClient,
                _sswPeopleNamesClEntity,
                new TestLuisOptions(),
                NullLogger<ListEntitySync>.Instance);
        }

        [Fact]
        public void Should_Have_Old_Data_For_Test()
        {
            // Assert
            _testData.SswPeopleNames.SubLists[0].List.Contains(_testData.MqEmployees[0].Message.FullName).ShouldBeTrue();
            _testData.SswPeopleNames.SubLists[1].List.Contains(_testData.MqEmployees[1].Message.FullName).ShouldBeFalse();
        }

        [Fact]
        public async void Should_Update_SubLists()
        {
            // Arrange
            var upsertCanonicalForms = _testData.MqEmployees.Select(mqEmployee => 
                _sswPeopleNamesClEntity.GetCanonicalForm(mqEmployee.Message));

            //Act
            await _listEntitySync.SyncSswPeopleNames(_testData.MqEmployees, default);

            // Assert
            _testData.SswPeopleNames.SubLists.Count.ShouldBe(2);
            _testData.SswPeopleNames.SubLists.All(subList => upsertCanonicalForms.Contains(subList.CanonicalForm)).ShouldBeTrue();
        }
    }
}