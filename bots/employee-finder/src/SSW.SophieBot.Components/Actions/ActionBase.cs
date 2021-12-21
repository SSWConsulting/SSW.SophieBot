using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace SSW.SophieBot.Components.Actions
{
    public abstract class ActionBase : Dialog
    {
        [JsonConstructor]
        public ActionBase([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            if (!string.IsNullOrWhiteSpace(sourceFilePath))
            {
                RegisterSourceLocation(sourceFilePath, sourceLineNumber);
            }
        }
    }
}
