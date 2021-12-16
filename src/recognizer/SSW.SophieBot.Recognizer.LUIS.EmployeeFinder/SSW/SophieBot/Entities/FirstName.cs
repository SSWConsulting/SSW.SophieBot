namespace SSW.SophieBot.Entities
{
    [Model("firstName")]
    [ChildOf(typeof(Contact))]
    [Feature(typeof(SswPersonNames), typeof(PersonName))]
    public class FirstName : RecognizerModelBase, IEntity
    {

    }
}
