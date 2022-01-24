using System;
using System.Collections.Generic;

namespace SSW.SophieBot
{
    public static class Check
    {
        public static T NotNull<T>(this T obj, string paramName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(paramName);
            }

            return obj;
        }

        public static T NotNullOrDefault<T>(this T obj, string paramName)
        {
            if (obj == null || obj.Equals(GetDefaultValue<T>()))
            {
                throw new ArgumentNullException(paramName);
            }

            return obj;
        }

        public static string NotNullOrEmpty(this string str, string paramName)
        {
            if (str.IsNullOrEmpty())
            {
                throw new ArgumentNullException(paramName);
            }

            return str;
        }

        public static string NotNullOrWhiteSpace(this string str, string paramName)
        {
            if (str.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(paramName);
            }

            return str;
        }

        public static IEnumerable<T> NotNullOrEmpty<T>(this IEnumerable<T> collection, string paramName)
        {
            if (collection.IsNullOrEmpty())
            {
                throw new ArgumentNullException(paramName);
            }

            return collection;
        }

        public static T GetDefaultValue<T>()
        {
            return default;
        }

        public static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}
