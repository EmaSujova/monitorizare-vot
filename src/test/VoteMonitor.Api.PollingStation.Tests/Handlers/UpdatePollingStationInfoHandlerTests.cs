using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using VoteMonitor.Api.PollingStation.Handlers;
using VoteMonitor.Api.PollingStation.Profiles;
using VoteMonitor.Api.PollingStation.Queries;
using VoteMonitor.Entities;
using Xunit;

namespace VoteMonitor.Api.PollingStation.Tests.Handlers
{
    public class UpdatePollingStationInfoHandlerTests
    {
        private readonly DbContextOptions<VoteMonitorContext> _dbContextOptions;
        private readonly Mock<ILogger<UpdatePollingStationInfoHandler>> _mockLogger;
        private readonly MapperConfiguration _mapperConfiguration;

        public UpdatePollingStationInfoHandlerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<VoteMonitorContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PollingStationProfile>();
            });

            _mockLogger = new Mock<ILogger<UpdatePollingStationInfoHandler>>();
        }

        [Fact]
        public async Task Handle_WhenPollingStationNotFound_ReturnsEmpty()
        {
            //var mockContext = new Mock<VoteMonitorContext>(_dbContextOptions);
            //mockContext.Setup(m => m.PollingStationInfos).Returns<PollingStationInfo>(null);
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var pollingStation = new Entities.PollingStationInfo
                {
                    IdObserver = 22,
                    IdPollingStation = 3
                };
                SetupContextWithPollingStation(pollingStation);
                var sut = new UpdatePollingStationInfoHandler(context, new Mapper(_mapperConfiguration), _mockLogger.Object);

                var requestNonExistingPollingStation = new UpdatePollingStationInfo
                {
                    IdObserver = 1,
                    IdPollingStation = 1,
                    ObserverLeaveTime = DateTime.Now
                };
                var result = await sut.Handle(requestNonExistingPollingStation, new CancellationToken());

                result.Should().Be(Unit.Value);
            }
        }

        [Fact]
        public async Task Handle_WhenCorrectObserverLeaveTimeFound_ReturnsEmpty()
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var observerLeaveTime = DateTime.Now;
                var pollingStation = new Entities.PollingStationInfo
                {
                    IdObserver = 22,
                    IdPollingStation = 3
                };
                SetupContextWithPollingStation(pollingStation);
                var sut = new UpdatePollingStationInfoHandler(context, new Mapper(_mapperConfiguration), _mockLogger.Object);

                var requestPollingStation = new UpdatePollingStationInfo
                {
                    IdObserver = 22,
                    IdPollingStation = 3,
                    ObserverLeaveTime = observerLeaveTime
                };
                var result = await sut.Handle(requestPollingStation, new CancellationToken());

                var updatedPollingStationInfo = context.PollingStationInfos.First(p => p.IdObserver == pollingStation.IdObserver);
                updatedPollingStationInfo.ObserverLeaveTime.Should().Be(observerLeaveTime);
            }
        }

        [Fact]
        public async Task Handle_WhenContextNull_ThrowsException()
        {
            var mockContext = new Mock<VoteMonitorContext>(_dbContextOptions);
            mockContext.Setup(m => m.PollingStationInfos).Returns<PollingStationInfo>(null);

            var pollingStation = new Entities.PollingStationInfo
            {
                IdObserver = 22,
                IdPollingStation = 3
            };
            SetupContextWithPollingStation(pollingStation);
            var sut = new UpdatePollingStationInfoHandler(mockContext.Object, new Mapper(_mapperConfiguration), _mockLogger.Object);

            var requestNonExistingPollingStation = new UpdatePollingStationInfo
            {
                IdObserver = 1,
                IdPollingStation = 1,
                ObserverLeaveTime = DateTime.Now
            };
            Unit result = Unit.Value;
            await Record.ExceptionAsync(async () => result = await sut.Handle(requestNonExistingPollingStation, new CancellationToken()));
        }

        private void SetupContextWithPollingStation(Entities.PollingStationInfo pollingStationInfo)
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                context.PollingStationInfos.Add(pollingStationInfo);
                context.SaveChanges();
            }
        }
    }
}
