using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring.Models;
using Microsoft.Rest;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SSW.SophieBot
{
    public class TestModel
    {
        public static IModel CreateInstance(TestData testData)
        {
            var mock = new Mock<IModel>();

            mock.Setup(model => model.ListClosedListsWithHttpMessagesAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()).Result)
                .Returns((Guid _, string _, int skip, int _, Dictionary<string, List<string>> _, CancellationToken _) =>
                    new HttpOperationResponse<IList<ClosedListEntityExtractor>>
                    {
                        Body = skip == 0 ? testData.ClosedListEntities : new List<ClosedListEntityExtractor>()
                    });

            mock.Setup(model => model.GetClosedListWithHttpMessagesAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()).Result)
                .Returns((Guid _, string _, Guid entityId, Dictionary<string, List<string>> _, CancellationToken _) =>
                    new HttpOperationResponse<ClosedListEntityExtractor>
                    {
                        Body = testData.ClosedListEntities.FirstOrDefault(entity => entity.Id == entityId)
                    });

            mock.Setup(model => model.UpdateClosedListWithHttpMessagesAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<ClosedListModelUpdateObject>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()).Result)
                .Callback((
                    Guid _,
                    string _,
                    Guid entityId,
                    ClosedListModelUpdateObject clObject,
                    Dictionary<string, List<string>> _,
                    CancellationToken _) =>
                    {
                        var clEntity = testData.ClosedListEntities.FirstOrDefault(entity => entity.Id == entityId);
                        if (clEntity != null)
                        {
                            clEntity.SubLists = clObject.SubLists
                                .Select(subList => new SubClosedListResponse(subList.CanonicalForm, subList.List))
                                .ToList();
                        }
                    })
                .Returns(new HttpOperationResponse<OperationStatus>
                {
                    Body = new OperationStatus(OperationStatusType.Success)
                });

            return mock.Object;
        }
    }
}
