using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Rest;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestApps
    {
        public static IApps CreateInstance()
        {
            var mock = new Mock<IApps>();
            mock.Setup(apps => apps.GetWithHttpMessagesAsync(
                It.IsAny<Guid>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()).Result)
                .Returns(new HttpOperationResponse<ApplicationInfoResponse>
                {
                    Body = new ApplicationInfoResponse(activeVersion: "1.0")
                });

            return mock.Object;
        }
    }
}
