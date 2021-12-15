using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using SSW.SophieBot.DataSync.Crm.Functions;
using SSW.SophieBot.DataSync.Crm.Test.Data;
using SSW.SophieBot.Employees;
using SSW.SophieBot.Persistence;
using SSW.SophieBot.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SSW.SophieBot.DataSync.Crm.Test
{
    public class EmployeeSyncTests
    {
        private readonly TestData _testData;
        private readonly string _syncVersion;
        private readonly TimerInfo _testTimerInfo;

        private TestEmployeeOdataService _testEmployeeOdataService;
        private TestSyncSnapshotRepository _testSyncSnapshotRepository;
        private TestServiceBusClient _testServiceBusClient;
        private TestSyncVersionGenerator _testSyncVersionGenerator;

        private EmployeeSync _employeeSync;

        public EmployeeSyncTests()
        {
            // Context Arrange
            _testData = new TestData();
            _syncVersion = Guid.NewGuid().ToString();
            _testTimerInfo = TestTimerInfo.Instance;

            _testEmployeeOdataService = new TestEmployeeOdataService(_testData);
            _testSyncSnapshotRepository = new TestSyncSnapshotRepository(_syncVersion, _testData);
            _testServiceBusClient = new TestServiceBusClient();
            _testSyncVersionGenerator = new TestSyncVersionGenerator(_syncVersion);

            SetEmployeeSyncInstance();
        }

        [Fact]
        public async void Should_Sync_All_Profiles_Initially()
        {
            // Act
            await _employeeSync.SyncEmployeeProfileAsync(_testTimerInfo, default);

            // Assert
            _testData.Snapshots.Count.ShouldBe(7);

            var batchContent = AssertBatchMode();
            batchContent.Count.ShouldBe(7);
            batchContent.ShouldAllBe(message => message.SyncMode == SyncMode.Create);
        }

        [Fact]
        public async void Should_Sync_New_Profile()
        {
            // Arrange
            var firstVersion = Guid.NewGuid().ToString();
            var newId = Guid.NewGuid().ToString();

            ManuallySyncSnapshots(firstVersion);

            // Act
            _testData.CrmEmployees.Add(new CrmEmployee
            {
                Systemuserid = newId,
                Organizationid = "ssw",
                Firstname = "New",
                Lastname = "Northwind",
                Fullname = "New Northwind",
                Modifiedon = DateTime.Now
            });
            await _employeeSync.SyncEmployeeProfileAsync(_testTimerInfo, default);

            // Assert
            _testData.Snapshots.Count.ShouldBe(8);
            _testData.Snapshots.Single(snapshot => snapshot.Id == newId).ShouldNotBeNull();
            _testData.Snapshots.ShouldAllBe(snapshot => snapshot.SyncVersion == _syncVersion);

            var batchContent = AssertBatchMode();
            batchContent.Single().SyncMode.ShouldBe(SyncMode.Create);
        }

        [Fact]
        public async void Should_Sync_Modified_Profile()
        {
            // Arrange
            const string NewFullName = "NewFullName";
            const string NewOrganizationId = "sswtest";

            var firstVersion = Guid.NewGuid().ToString();
            ManuallySyncSnapshots(firstVersion);

            // Act
            var employeeToModify = _testData.CrmEmployees.First();
            employeeToModify.Fullname = NewFullName;
            employeeToModify.Organizationid = NewOrganizationId;
            employeeToModify.Modifiedon = DateTime.Now;

            await _employeeSync.SyncEmployeeProfileAsync(_testTimerInfo, default);

            // Assert
            _testData.Snapshots
                .Single(snapshot => snapshot.Id == employeeToModify.Systemuserid)
                .OrganizationId
                .ShouldBe(NewOrganizationId);
            _testData.Snapshots.ShouldAllBe(snapshot => snapshot.SyncVersion == _syncVersion);

            var batchContent = AssertBatchMode();
            batchContent.Single().SyncMode.ShouldBe(SyncMode.Update);
            batchContent.Single().Message.FullName.ShouldBe(NewFullName);
        }

        [Fact]
        public async void Should_Sync_Deleted_Profile()
        {
            // Arrange
            var firstVersion = Guid.NewGuid().ToString();

            var mockSyncSnapshotRepository = new Mock<TestSyncSnapshotRepository>(_syncVersion, _testData)
            {
                CallBase = true,
            };
            var batchMock = new Mock<ITransactionBatch<PatchOperation>>();
            batchMock.Setup(batch => batch.SaveChangesAsync().Result).Callback(() =>
            {
                for (int i = 1; i < _testData.Snapshots.Count; i++)
                {
                    _testData.Snapshots[i].SyncVersion = _syncVersion;
                }
            }).Returns(true);
            mockSyncSnapshotRepository.Setup(func => func.BeginTransactionAsync(It.IsAny<CancellationToken>()).Result).Returns(batchMock.Object);

            _testSyncSnapshotRepository = mockSyncSnapshotRepository.Object;
            SetEmployeeSyncInstance();

            ManuallySyncSnapshots(firstVersion);

            // Act
            var employeeToDelete = _testData.CrmEmployees.First();
            _testData.CrmEmployees.Remove(employeeToDelete);
            await _employeeSync.SyncEmployeeProfileAsync(_testTimerInfo, default);

            // Assert
            _testData.Snapshots.Count.ShouldBe(6);
            _testData.Snapshots.ShouldAllBe(snapshot => snapshot.SyncVersion == _syncVersion);
            _testData.Snapshots.ShouldAllBe(snapshot => snapshot.Id != employeeToDelete.Systemuserid);

            var batchContent = AssertBatchMode();
            batchContent.Single().SyncMode.ShouldBe(SyncMode.Delete);
        }

        // TODO: composite sync test

        private void SetEmployeeSyncInstance()
        {
            _employeeSync = new EmployeeSync(
                _testEmployeeOdataService,
                _testSyncSnapshotRepository,
                _testServiceBusClient,
                _testSyncVersionGenerator,
                new TestSyncOptions(),
                NullLogger<EmployeeSync>.Instance
             );
        }

        private void ManuallySyncSnapshots(string syncVersion)
        {
            _testData.Snapshots = _testData.CrmEmployees.Select(employee => new SyncSnapshot
            {
                Id = employee.Systemuserid,
                OrganizationId = employee.Organizationid,
                Modifiedon = employee.Modifiedon,
                SyncVersion = syncVersion
            }).ToList();
        }

        private List<MqMessage<Employee>> AssertBatchMode()
        {
            var batch = _testServiceBusClient.MqMessages ?? Enumerable.Empty<MqMessage<Employee>>();

            batch.Count(message => message.BatchMode == BatchMode.BatchStart || message.BatchMode == BatchMode.BatchEnd).ShouldBe(2);
            batch.First().BatchMode.ShouldBe(BatchMode.BatchStart);
            batch.Last().BatchMode.ShouldBe(BatchMode.BatchEnd);

            var batchContent = batch.Where(message => message.BatchMode == BatchMode.BatchContent).ToList();
            batchContent.Count.ShouldBe(batch.Count() - 2);

            return batchContent;
        }
    }
}
