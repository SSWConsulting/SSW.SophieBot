using System.Collections.Generic;

namespace SSW.SophieBot
{
    public interface IIntent<TExample> : IModel
        where TExample : IExample
    {
        ICollection<TExample> Examples { get; }
    }
}
