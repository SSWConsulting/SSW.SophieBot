namespace SSW.SophieBot
{
    [TestModelDependency(typeof(TestModelDependency))]
    public class NameEntity : RecognizerModelBase, IEntity
    {

    }

    [TestModelDependency(typeof(NameEntity))]
    [TestModelDependency(typeof(TestModelDependency))]
    public class FirstNameEntity : NameEntity
    {

    }

    [TestModelDependency(typeof(FirstNameEntity))]
    public class LastNameEntity : NameEntity
    {

    }
}
