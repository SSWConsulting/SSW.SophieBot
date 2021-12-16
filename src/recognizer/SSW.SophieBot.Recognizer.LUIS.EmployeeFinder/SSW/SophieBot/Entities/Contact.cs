using System;

namespace SSW.SophieBot.Entities
{
    [Feature(typeof(SswPersonNames))]
    [Feature(typeof(PersonName))]
    public class Contact : RecognizerModelBase, IEntity
    {
        public Type Parent { get; }
    }
}
