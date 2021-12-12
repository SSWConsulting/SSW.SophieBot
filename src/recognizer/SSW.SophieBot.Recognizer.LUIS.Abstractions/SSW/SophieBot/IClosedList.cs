using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using System.Collections.Generic;

namespace SSW.SophieBot
{
    public interface IClosedList : IModel
    {
        ICollection<SubClosedList> SubLists { get; }
    }
}
