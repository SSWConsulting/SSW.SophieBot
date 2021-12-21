using System;
using System.Collections.Generic;

namespace SSW.SophieBot
{
    public interface IModelDependency
    {
        List<Type> GetDependencies();
    }
}
