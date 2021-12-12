using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot
{
    public class BasicExample : IExample
    {
        public FormattableString FormatText { get; protected set; }

        public ICollection<ExampleLabel> ChildLabels { get; } = new List<ExampleLabel>();

        public BasicExample(FormattableString formatText)
        {
            FormatText = ProcessFormatText(formatText);
        }

        public virtual List<ExampleLabel> GetAllExampleLabels()
        {
            return EnumerateLabelsRecursively().ToList();
        }

        public virtual List<Type> GetAllEntityTypes()
        {
            return EnumerateEntityTypesRecursively().ToList();
        }

        private IEnumerable<ExampleLabel> EnumerateLabelsRecursively(ExampleLabel label = null)
        {
            var exampleLabels = label?.ChildLabels ?? ChildLabels;

            foreach (var childLabel in exampleLabels)
            {
                exampleLabels = exampleLabels.Concat(EnumerateLabelsRecursively(childLabel)).ToList();
            }

            return exampleLabels.ToList();
        }

        private IEnumerable<Type> EnumerateEntityTypesRecursively(ExampleLabel label = null)
        {
            var targetFormatText = label != null ? label.FormatText : FormatText;
            var topLevelLabels = targetFormatText.GetArguments()
                .OfType<ExampleLabel>();

            var entityTypes = Enumerable.Empty<Type>();
            if (label != null)
            {
                entityTypes = entityTypes.Append(label.EntityType);
            }

            foreach (var topLevelLabel in topLevelLabels)
            {
                entityTypes = entityTypes.Concat(EnumerateEntityTypesRecursively(topLevelLabel));
            }

            return entityTypes.Distinct();
        }

        protected virtual FormattableString ProcessFormatText(FormattableString formatText)
        {
            Check.NotNullOrWhiteSpace(formatText?.ToString(), nameof(formatText));

            var childLabels = formatText.GetArguments()
                .OfType<ExampleLabel>();

            ProcessChildLabels(childLabels);

            return formatText;
        }

        protected virtual void ProcessChildLabels(IEnumerable<ExampleLabel> childLabels)
        {
            foreach (var childLabel in childLabels)
            {
                ChildLabels.Add(childLabel);
                childLabel.Parent = this;
            }
        }

        public override string ToString()
        {
            return FormatText.ToString(new ExampleLabelFormatter());
        }

        public static implicit operator BasicExample(FormattableString formatText)
        {
            return new BasicExample(formatText);
        }
    }
}
