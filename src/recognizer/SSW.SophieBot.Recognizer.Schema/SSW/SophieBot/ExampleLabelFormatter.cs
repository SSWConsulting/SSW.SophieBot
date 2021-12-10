using System;
using System.Globalization;

namespace SSW.SophieBot
{
    public class ExampleLabelFormatter : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            if (formatType == typeof(ICustomFormatter))
            {
                return this;
            }

            return null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (arg is Type entityType && typeof(IEntity).IsAssignableFrom(entityType))
            {
                return Check.NotNullOrWhiteSpace(format, nameof(format));
            }

            return HandleOtherFormats(format, arg);
        }

        private string HandleOtherFormats(string format, object arg)
        {
            if (arg is IFormattable formattable)
            {
                return formattable.ToString(format, CultureInfo.CurrentCulture);
            }
            else if (arg != null)
            {
                return arg.ToString();
            }

            return string.Empty;
        }
    }
}
