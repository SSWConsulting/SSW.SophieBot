namespace System
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }

        public static string EnsureStartsWith(this string source, string str)
        {
            if (string.IsNullOrEmpty(source))
            {
                return str;
            }

            if (string.IsNullOrEmpty(str) || source.EndsWith(str))
            {
                return source;
            }

            return $"{str}{source}";
        }

        public static string EnsureEndsWith(this string source, string str)
        {
            if (string.IsNullOrEmpty(source))
            {
                return str;
            }

            if (string.IsNullOrEmpty(str) || source.EndsWith(str))
            {
                return source;
            }

            return $"{source}{str}";
        }

        public static string EnsureSurroundsWith(this string source, string str)
        {
            return source.EnsureStartsWith(str).EnsureEndsWith(str);
        }
    }
}
