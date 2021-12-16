using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using SSW.SophieBot;

namespace Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring
{
    public static class ModelExtensions
    {
        public static OperationStatus EnsureSuccessOperationStatus(this OperationStatus operationStatus)
        {
            if (operationStatus.Code != OperationStatusType.Success)
            {
                throw new LuisException($"LUIS operation didn't indicate success: {operationStatus.Message}");
            }

            return operationStatus;
        }
    }
}
