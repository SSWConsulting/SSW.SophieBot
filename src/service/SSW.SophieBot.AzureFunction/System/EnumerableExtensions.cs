using System.Collections.Generic;
using System.Linq;

namespace SSW.SophieBot.AzureFunction.System
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> data)
        {
            return data == null || !data.Any();
        }
    }
}
