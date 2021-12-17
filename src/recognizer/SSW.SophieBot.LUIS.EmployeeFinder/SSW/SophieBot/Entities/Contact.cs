using System;

namespace SSW.SophieBot.Entities
{
    [Model("contact")]
    [Feature(typeof(SswPersonNames), typeof(PersonName))]
    public class Contact : RecognizerModelBase, IEntity
    {
        public Type Parent { get; }
    }
}
