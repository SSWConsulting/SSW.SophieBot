using System;
using System.Collections.Generic;

namespace SSW.SophieBot
{
    public interface IEntity : IModel
    {
        Type Parent { get; }

        ICollection<Type> Children { get; }
    }
}
