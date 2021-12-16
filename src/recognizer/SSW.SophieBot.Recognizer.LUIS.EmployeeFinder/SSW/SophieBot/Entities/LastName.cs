namespace SSW.SophieBot.Entities
{
    [Model("lastName")]
    [ChildOf(typeof(Contact))]
    [Feature(typeof(SswPersonNames), typeof(PersonName))]
    public class LastName : RecognizerModelBase, IEntity
    {

    }
}
