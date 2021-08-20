using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;

namespace SSWSophieBot.HttpClientComponents.Abstractions
{
    // TODO: extract to a base lib
    public static class DialogContextExtensions
    {
        public static T GetValue<T>(this DialogContext dialogContext, ExpressionProperty<T> expressionProperty)
        {
            if (expressionProperty == null)
            {
                return default;
            }

            return expressionProperty.GetValue(dialogContext.State);
        }
    }
}
