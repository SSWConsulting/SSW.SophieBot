using Shouldly;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace SSW.SophieBot.Recognizer.Schema.Test
{
    public class ExampleTests
    {
        [Fact]
        public void Should_Get_Raw_Text_Calling_ToString()
        {
            // Arrange
            var firstNameLabel = (ExampleLabel)$"{typeof(FirstNameEntity):Adam}";
            var lastNameLabel = (ExampleLabel)$"{typeof(LastNameEntity):Cogan}";
            var nameLabel = new ExampleLabel(typeof(NameEntity), $"{firstNameLabel} {lastNameLabel}");
            var example = (BasicExample)$"who is {nameLabel}";

            // Act
            var rawText = example.ToString();

            // Assert
            rawText.ShouldBe("who is Adam Cogan");
        }

        public class NameEntity : IEntity
        {
            public virtual Type Parent => null;

            public virtual ICollection<Type> Children => new List<Type>
            {
                typeof(FirstNameEntity),
                typeof(LastNameEntity)
            };

            public async IAsyncEnumerable<bool> SeedAsync(CancellationToken cancellationToken = default)
            {
                yield return true;
            }
        }

        public class FirstNameEntity : NameEntity
        {
            public override Type Parent => typeof(NameEntity);

            public override ICollection<Type> Children => new List<Type>();
        }

        public class LastNameEntity : NameEntity
        {
            public override Type Parent => typeof(NameEntity);

            public override ICollection<Type> Children => new List<Type>();
        }
    }
}
