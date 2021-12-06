using Microsoft.Azure.CognitiveServices.Language.LUIS.Authoring;
using Microsoft.Rest;
using Newtonsoft.Json;
using System;

namespace SSW.SophieBot.LUIS.Sync.Test.Data
{
    public class TestLUISAuthoringClient : ILUISAuthoringClient
    {
        public JsonSerializerSettings SerializationSettings => throw new NotImplementedException();

        public JsonSerializerSettings DeserializationSettings => throw new NotImplementedException();

        public string Endpoint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public ServiceClientCredentials Credentials => throw new NotImplementedException();

        public IFeatures Features => throw new NotImplementedException();

        public IExamples Examples => throw new NotImplementedException();

        public IModel Model { get; }

        public IApps Apps { get; }

        public IVersions Versions => throw new NotImplementedException();

        public ITrain Train => throw new NotImplementedException();

        public IPermissions Permissions => throw new NotImplementedException();

        public IPattern Pattern => throw new NotImplementedException();

        public ISettings Settings => throw new NotImplementedException();

        public IAzureAccounts AzureAccounts => throw new NotImplementedException();

        public TestLUISAuthoringClient(TestData testData)
        {
            Apps = TestApps.CreateInstance();
            Model = TestModel.CreateInstance(testData);
        }

        public void Dispose()
        {

        }
    }
}
