using System.Collections.Generic;

namespace SSW.SophieBot
{
    public interface IPhraseList : IRecognizerModel
    {
        ICollection<string> Phrases { get; }
    }
}
