using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.Dialogs;

namespace SSWSophieBot.Components
{
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
