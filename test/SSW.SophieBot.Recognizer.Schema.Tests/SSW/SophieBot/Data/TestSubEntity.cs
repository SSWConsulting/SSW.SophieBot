namespace SSW.SophieBot
{
    [TestModelDependency(typeof(LastNameEntity))]
    public class TestSubEntity : RecognizerModelBase, IEntity
    {
    }
}
