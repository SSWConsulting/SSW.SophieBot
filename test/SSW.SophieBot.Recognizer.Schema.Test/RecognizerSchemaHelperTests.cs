using Shouldly;
using SSW.SophieBot.Recognizer.Schema.Test.Data;
using System.Linq;
using Xunit;

namespace SSW.SophieBot.Recognizer.Schema.Test
{
    public class RecognizerSchemaHelperTests
    {
        [Fact]
        public void Should_Scan_All_Dependended_Schema_Types_And_Distincted()
        {
            // Act
            var schemaTypes = RecognizerSchemaHelper.GetInheritedSchemaTypes<TestSchema>();

            // Assert
            schemaTypes.Count.ShouldBe(2);
            schemaTypes.Count.ShouldBe(schemaTypes.Distinct().Count());
            schemaTypes.ShouldContain(typeof(BaseTestSchema));
            schemaTypes.ShouldContain(typeof(BasicTestSchemaBase));
        }

        [Fact]
        public void Should_Sort_Model_Types_By_Dependency_Order()
        {
            // Act
            var modelTypes = RecognizerSchemaHelper.GetAllModelTypes<TestSchema>().ToList();

            // Assert
            modelTypes.Count.ShouldBe(5);
            modelTypes[0].ModelType.ShouldBe(typeof(TestModelDependency));
            modelTypes[1].ModelType.ShouldBe(typeof(NameEntity));
            modelTypes[2].ModelType.ShouldBe(typeof(FirstNameEntity));
            modelTypes[3].ModelType.ShouldBe(typeof(LastNameEntity));
            modelTypes[4].ModelType.ShouldBe(typeof(TestSubEntity));
        }
    }
}