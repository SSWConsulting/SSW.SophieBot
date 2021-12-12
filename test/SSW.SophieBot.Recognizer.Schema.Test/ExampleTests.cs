using Shouldly;
using System.Linq;
using Xunit;

namespace SSW.SophieBot.Recognizer.Schema.Test
{
    public class ExampleTests
    {
        private readonly ExampleLabel _firstNameLabel;
        private readonly ExampleLabel _lastNameLabel;
        private readonly ExampleLabel _nameLabel;
        private readonly BasicExample _example;

        public ExampleTests()
        {
            _firstNameLabel = (ExampleLabel)$"{typeof(FirstNameEntity):Adam}";
            _lastNameLabel = (ExampleLabel)$"{typeof(LastNameEntity):Cogan}";
            _nameLabel = new ExampleLabel(typeof(NameEntity), $"{_firstNameLabel} {_lastNameLabel}");
            _example = (BasicExample)$"who is {_nameLabel}";
        }

        [Fact]
        public void Should_Have_Correct_Parent_Label()
        {
            // Assert
            _nameLabel.Parent.ShouldBe(_example);
            _firstNameLabel.Parent.ShouldBe(_nameLabel);
            _lastNameLabel.Parent.ShouldBe(_nameLabel);
        }

        [Fact]
        public void Should_Have_Correct_Child_Label()
        {
            // Assert
            _example.ChildLabels.Count.ShouldBe(1);
            _nameLabel.ChildLabels.Count.ShouldBe(2);
            _nameLabel.ChildLabels.ToList()[1].ShouldBe(_lastNameLabel);
        }

        [Fact]
        public void Should_Get_Correct_Raw_Text_Calling_ToString()
        {
            // Assert
            _example.ToString().ShouldBe("who is Adam Cogan");
            _nameLabel.ToString().ShouldBe("Adam Cogan");
            _firstNameLabel.ToString().ShouldBe("Adam");
            _lastNameLabel.ToString().ShouldBe("Cogan");
        }

        [Fact]
        public void Should_Get_All_Example_Labels_Recursively()
        {
            // Act
            var exampleLabelsCount = _example.GetAllExampleLabels().Count;

            // Assert
            exampleLabelsCount.ShouldBe(3);
        }

        [Fact]
        public void Should_Get_All_Entity_Types_Recursively()
        {
            // Act
            var entityTypesCount = _example.GetAllEntityTypes().Count;

            // Assert
            entityTypesCount.ShouldBe(3);
        }
    }
}
