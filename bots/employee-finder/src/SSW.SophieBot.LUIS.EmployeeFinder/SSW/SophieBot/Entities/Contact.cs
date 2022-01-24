using System;

namespace SSW.SophieBot.Entities
{
    [Model("contact")]
    [Feature(typeof(PersonNames), typeof(PersonName))]
    public class Contact : RecognizerModelBase, IEntity
    {
        public Type Parent { get; }
    }
}
