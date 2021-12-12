using Shouldly;
using System.Linq;
using Xunit;

namespace SSW.SophieBot.Recognizer.LUIS.Abstractions.Test
{
    public class LuisSchemaTests
    {
        private readonly ExampleLabel _firstNameLabel;
        private readonly ExampleLabel _lastNameLabel;
        private readonly ExampleLabel _nameLabel;
        private readonly LuisExample _example;

        public LuisSchemaTests()
        {
            _firstNameLabel = (ExampleLabel)$"{typeof(FirstNameEntity):Adam}";
            _lastNameLabel = (ExampleLabel)$"{typeof(LastNameEntity):Adam}";
            _nameLabel = new ExampleLabel(typeof(NameEntity), $"{_firstNameLabel} {_lastNameLabel}");
            _example = (LuisExample)$"who is {_nameLabel}";
        }

        [Fact]
        public void Should_Create_Correct_EntityLabelObjects()
        {
            // Act
            var entityLabelObjects = _example.GetEntityLabelObjects();
            var nameEntityLabelObject = entityLabelObjects.First(obj => obj.EntityName == ModelAttribute.GetName(typeof(NameEntity)));
            var firstNameEntityLabelObject = entityLabelObjects.First(obj => obj.EntityName == ModelAttribute.GetName(typeof(FirstNameEntity)));
            var lastNameEntityLabelObject = entityLabelObjects.First(obj => obj.EntityName == ModelAttribute.GetName(typeof(LastNameEntity)));

            // Assert
            entityLabelObjects.Count.ShouldBe(3);
            nameEntityLabelObject.StartCharIndex.ShouldBe(7);
            nameEntityLabelObject.EndCharIndex.ShouldBe(15);
            firstNameEntityLabelObject.StartCharIndex.ShouldBe(7);
            firstNameEntityLabelObject.EndCharIndex.ShouldBe(10);
            lastNameEntityLabelObject.StartCharIndex.ShouldBe(12);
            lastNameEntityLabelObject.EndCharIndex.ShouldBe(15);
        }
    }
}