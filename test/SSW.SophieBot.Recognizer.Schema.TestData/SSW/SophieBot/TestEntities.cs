using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot
{
    [TestModelDependency(typeof(TestModelDependency))]
    public class NameEntity : IEntity
    {
        public async IAsyncEnumerable<bool> SeedAsync(CancellationToken cancellationToken = default)
        {
            yield return true;
        }
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
