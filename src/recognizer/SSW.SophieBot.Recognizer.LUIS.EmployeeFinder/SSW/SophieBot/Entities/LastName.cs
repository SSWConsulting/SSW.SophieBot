namespace SSW.SophieBot.Entities
{
    [ChildOf(typeof(Contact))]
    [Feature(typeof(SswPersonNames))]
    [Feature(typeof(PersonName))]
    public class LastName : RecognizerModelBase, IEntity
    {

    }
}
