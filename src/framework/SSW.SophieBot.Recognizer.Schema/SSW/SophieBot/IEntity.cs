using System;
using System.Collections.Generic;

namespace SSW.SophieBot
{
    public interface IEntity : IRecognizerModel
    {
        ICollection<Type> Children { get; }
    }
}
