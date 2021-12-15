using System;

namespace SSW.SophieBot
{
    public interface IModelDependency
    {
        Type Dependency { get; }
    }
}
