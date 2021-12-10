using System;
using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot
{
    public class BasicExample : IExample
    {
        public FormattableString FormatText { get; }

        public BasicExample(FormattableString formatText)
        {
            FormatText = Check.NotNull(formatText, nameof(formatText));
        }

        protected virtual List<ExampleLabel> GetLabels()
        {
            return FormatText.GetArguments()
                .OfType<ExampleLabel>()
                .ToList();
        }

        public override string ToString()
        {
            return FormatText.ToString();
        }

        public static implicit operator BasicExample(FormattableString formatText)
        {
            return new BasicExample(formatText);
        }
    }
}
