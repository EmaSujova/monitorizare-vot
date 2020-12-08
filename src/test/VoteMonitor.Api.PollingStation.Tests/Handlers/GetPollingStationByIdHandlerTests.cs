using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
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
    public class GetPollingStationsByIdHandlerTests
    {
        private readonly DbContextOptions<VoteMonitorContext> _dbContextOptions;
        private readonly MapperConfiguration _mapperConfiguration;
        private readonly Mock<ILogger<GetPollingStationByIdHandler>> _mockLogger;

        public GetPollingStationsByIdHandlerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<VoteMonitorContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<PollingStationProfile>();
            });

            _mockLogger = new Mock<ILogger<GetPollingStationByIdHandler>>();
        }

        [Fact]
        public async Task Handle_WhenNoPollingStationsExists_ThrowsException()
        {
            //SetupContextWithPollingStations(new List<Entities.PollingStation>
            //{
            //    new PollingStationBuilder().WithId(1).Build()
            //});

            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var sut = new GetPollingStationByIdHandler(context, new Mapper(_mapperConfiguration), _mockLogger.Object);

                var getPollingStationByIdRequest = new GetPollingStationById
                {
                    Id = 10
                };
                await Record.ExceptionAsync(async () => await sut.Handle(new GetPollingStationById(), new CancellationToken()));
            }
        }

        [Fact]
        public async Task Handle_WhenPollingStationWithRequestedIdNotFound_ThrowsException()
        {
            SetupContextWithPollingStations(new List<Entities.PollingStation>
            {
                new PollingStationBuilder().WithId(1).Build()
            });

            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var sut = new GetPollingStationByIdHandler(context, new Mapper(_mapperConfiguration), _mockLogger.Object);

                var getPollingStationByIdRequest = new GetPollingStationById
                {
                    Id = 10
                };
                await Record.ExceptionAsync(async () => await sut.Handle(new GetPollingStationById(), new CancellationToken()));
            }
        }

        [Fact]
        public async Task Handle_PollingStationFound_ReturnPollingStation()
        {
            SetupContextWithPollingStations(new List<Entities.PollingStation>
            {
                new PollingStationBuilder().WithId(10).Build()
            });

            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var sut = new GetPollingStationByIdHandler(context, new Mapper(_mapperConfiguration), _mockLogger.Object);

                var getPollingStationByIdRequest = new GetPollingStationById
                {
                    Id = 10
                };
                var result = await sut.Handle(getPollingStationByIdRequest, new CancellationToken());

                result.Id.Should().Be(10);
            }
        }

        private void SetupContextWithPollingStations(IEnumerable<Entities.PollingStation> pollingStations)
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                context.PollingStations.AddRange(pollingStations);
                context.SaveChanges();
            }
        }
    }
}
