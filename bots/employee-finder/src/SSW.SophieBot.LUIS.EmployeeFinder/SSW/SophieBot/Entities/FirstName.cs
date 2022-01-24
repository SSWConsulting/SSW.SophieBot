namespace SSW.SophieBot.Entities
{
    [Model("firstName")]
    [ChildOf(typeof(Contact))]
    [Feature(typeof(PersonNames), typeof(PersonName))]
    public class FirstName : RecognizerModelBase, IEntity
    {

    }
}
