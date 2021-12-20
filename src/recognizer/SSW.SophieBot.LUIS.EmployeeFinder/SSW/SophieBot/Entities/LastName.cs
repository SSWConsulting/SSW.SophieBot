namespace SSW.SophieBot.Entities
{
    [Model("lastName")]
    [ChildOf(typeof(Contact))]
    [Feature(typeof(PersonNames), typeof(PersonName))]
    public class LastName : RecognizerModelBase, IEntity
    {

    }
}
