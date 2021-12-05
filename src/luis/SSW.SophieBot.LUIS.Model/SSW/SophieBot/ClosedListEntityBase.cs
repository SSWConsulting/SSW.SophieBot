using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.SophieBot
{
    public abstract class ClosedListEntityBase : EntityBase
    {
        public virtual ISet<WordListObject> SubLists { get; } 
            = new HashSet<WordListObject>(new SubListEqualityComparer());

        public virtual Task FillSubListsAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        protected class SubListEqualityComparer : IEqualityComparer<WordListObject>
        {
            public bool Equals(WordListObject x, WordListObject y)
            {
                if (x == null && y == null)
                {
                    return true;
                }

                if (x == null ^ y == null)
                {
                    return false;
                }

                return x.CanonicalForm == y.CanonicalForm;
            }

            public int GetHashCode(WordListObject obj)
            {
                return obj.CanonicalForm.GetHashCode();
            }
        }
    }
}
