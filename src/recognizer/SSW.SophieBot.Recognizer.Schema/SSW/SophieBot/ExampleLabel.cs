using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot
{
    public class ExampleLabel
    {
        public Type EntityType { get; }

        public FormattableString FormatText { get; }

        public ExampleLabel(FormattableString formatText)
        {
            FormatText = Check.NotNull(formatText, nameof(formatText));
            var entityTypes = EnumerateEntityTypes();
            if (entityTypes.Count() != 1)
            {
                throw new ArgumentException("Label should have a single root entity type", nameof(formatText));
            }

            EntityType = entityTypes.Single();
        }

        public ExampleLabel(Type entityType, FormattableString formatText)
        {
            EntityType = Check.NotNull(entityType, nameof(entityType));

            Check.NotNullOrWhiteSpace(formatText.ToString(), nameof(formatText));
            FormatText = formatText;
        }

        protected virtual List<ExampleLabel> GetChildren()
        {
            return FormatText.GetArguments()
                .OfType<ExampleLabel>()
                .ToList();
        }

        protected virtual IEnumerable<Type> EnumerateEntityTypes()
        {
            return FormatText.GetArguments()
                .OfType<Type>()
                .Where(type => typeof(IEntity).IsAssignableFrom(type));
        }

        public override string ToString()
        {
            return FormatText.ToString(new ExampleLabelFormatter());
        }

        public static implicit operator ExampleLabel(FormattableString formatText)
        {
            return new ExampleLabel(formatText);
        }
    }
}
