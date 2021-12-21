using System;
using System.Collections.Generic;
using System.Reflection;

namespace SSW.SophieBot.Reflection
{
    public static class AssemblyHelper
    {
        public static IReadOnlyList<Type> GetAllTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types;
            }
        }
    }
}
