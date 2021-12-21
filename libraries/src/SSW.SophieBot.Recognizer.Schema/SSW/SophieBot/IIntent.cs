using System.Collections.Generic;

namespace SSW.SophieBot
{
    public interface IIntent<TExample> : IRecognizerModel
        where TExample : IExample
    {
        ICollection<TExample> Examples { get; }
    }
}
