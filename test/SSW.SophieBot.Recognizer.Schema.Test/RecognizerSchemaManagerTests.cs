using Shouldly;
using SSW.SophieBot.Recognizer.Schema.Test.Data;
using System.Linq;
using Xunit;

namespace SSW.SophieBot.Recognizer.Schema.Test
{
    public class RecognizerSchemaManagerTests
    {
        [Fact]
        public void Should_Scan_All_Dependent_Schema_Types_And_Distincted()
        {
            // Act
            var schemaTypes = RecognizerSchemaHelper.GetInheritedSchemaTypes<TestSchema>();

            // Assert
            schemaTypes.Count.ShouldBe(2);
            schemaTypes.Count.ShouldBe(schemaTypes.Distinct().Count());
            schemaTypes.ShouldContain(typeof(BaseTestSchema));
            schemaTypes.ShouldContain(typeof(BasicTestSchemaBase));
        }
    }
}