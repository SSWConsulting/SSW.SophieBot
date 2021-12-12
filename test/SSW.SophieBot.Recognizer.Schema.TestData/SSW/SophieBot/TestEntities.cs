using System;
using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot
{
    public class NameEntity : IEntity
    {
        public virtual ICollection<Type> Children => new List<Type>
            {
                typeof(FirstNameEntity),
                typeof(LastNameEntity)
            };

        public async IAsyncEnumerable<bool> SeedAsync(CancellationToken cancellationToken = default)
        {
            yield return true;
        }
    }

    public class FirstNameEntity : NameEntity
    {
        public override ICollection<Type> Children => new List<Type>();
    }

    public class LastNameEntity : NameEntity
    {
        public override ICollection<Type> Children => new List<Type>();
    }
}
