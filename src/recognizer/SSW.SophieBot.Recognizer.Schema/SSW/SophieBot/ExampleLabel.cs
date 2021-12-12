using System;
using System.Linq;

namespace SSW.SophieBot
{
    public class ExampleLabel : BasicExample
    {
        public Type EntityType { get; }

        public BasicExample Parent { get; set; }

        public ExampleLabel(FormattableString formatText) : base(formatText)
        {
            var topLevelEntityTypes = FormatText.GetArguments()
                .OfType<Type>()
                .Where(type => typeof(IEntity).IsAssignableFrom(type));

            if (topLevelEntityTypes.Count() != 1)
            {
                throw new RecognizerSchemaException("Entity label should have a single root entity type");
            }

            EntityType = topLevelEntityTypes.Single();
        }

        public ExampleLabel(Type entityType, FormattableString formatText) : base(formatText)
        {
            EntityType = Check.NotNull(entityType, nameof(entityType));
        }

        public static implicit operator ExampleLabel(FormattableString formatText)
        {
            return new ExampleLabel(formatText);
        }
    }
}
