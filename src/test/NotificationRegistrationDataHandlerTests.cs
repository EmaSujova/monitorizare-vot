using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using System.Linq;
using VoteMonitor.Api.Core.Commands;
using VoteMonitor.Api.Core.Services;
using VoteMonitor.Api.Notification.Commands;
using VoteMonitor.Entities;
using VoteMonitor.Api.Notification.Handlers;
using Microsoft.Extensions.Logging;
using Moq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using VoteMonitor.Api.Notification.Models;
using NSubstitute;

namespace VoteMonitor.Api.Notification.Tests.Handlers
{
    public class NotificationRegistrationDataHandlerTests
    {
        private readonly DbContextOptions<VoteMonitorContext> _dbContextOptions;
        private readonly MapperConfiguration _mapperConfiguration;
        private readonly IFirebaseService _firebaseService;

        public NotificationRegistrationDataHandlerTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<VoteMonitorContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RequestNotificationMapperProfile>();
            });

            _firebaseService = Substitute.For<IFirebaseService>();
        }

        [Fact]
        public async Task Handle_WhenDataInputed_ReturnsTrue()
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var sut = new NotificationRegistrationDataHandler(context, _firebaseService, new Mapper(_mapperConfiguration));

                var notificationRegistrationDataCommand = new NotificationRegistrationDataCommand
                {
                    ChannelName = "channel",
                    ObserverId = 1,
                    Token = "token"
                };

                await sut.Handle(notificationRegistrationDataCommand, new CancellationToken());

                context.NotificationRegistrationData.Any().Should().BeTrue();
            }
        }

        [Fact]
        public async Task Handle_WhenEnteredNewNotificationRegistration_ReturnsTrue()
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var sut = new NotificationRegistrationDataHandler(context, _firebaseService, new Mapper(_mapperConfiguration));

                var notificationRegistrationDataCommand = new NotificationRegistrationDataCommand
                {
                    ChannelName = "channel",
                    ObserverId = 1,
                    Token = "token"
                };

                await sut.Handle(notificationRegistrationDataCommand, new CancellationToken());

                context.NotificationRegistrationData.Any(c => c.ChannelName == notificationRegistrationDataCommand.ChannelName && c.ObserverId == notificationRegistrationDataCommand.ObserverId && c.Token == notificationRegistrationDataCommand.Token).Should().BeTrue();
            }
        }

        [Fact]
        public async Task Handle_WhenCorrectInputData_ReturnsTrue()
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var sut = new NotificationRegistrationDataHandler(context, _firebaseService, new Mapper(_mapperConfiguration));

                var notificationRegistrationData = new Entities.NotificationRegistrationData
                {
                    ChannelName = "channel",
                    ObserverId = 2,
                    Token = "token1"
                };

                SetupContextWithNotificationRegistrationData(notificationRegistrationData);

                var notificationRegistrationDataCommand = new NotificationRegistrationDataCommand
                {
                    ChannelName = "channel",
                    ObserverId = 2,
                    Token = "token"
                };

                await sut.Handle(notificationRegistrationDataCommand, new CancellationToken());

                context.NotificationRegistrationData.Any(c => c.Token == "token" && c.ChannelName == "channel" && c.ObserverId == 2).Should().BeTrue();
            }
        }

        [Fact]
        public async Task Handle_WhenEnteredNewNotification_ReturnsTrue()
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                var sut = new NotificationRegistrationDataHandler(context, _firebaseService, new Mapper(_mapperConfiguration));

                var newNotificationCommand = new NewNotificationCommand
                {
                    Channel = "channel",
                    From = "from",
                    Message = "message",
                    Recipients = new System.Collections.Generic.List<string>(),
                    SenderAdminId = 2,
                    Title = "title"
                    
                     
                };
                context.Notifications.Any().Should().BeFalse();
                await sut.Handle(newNotificationCommand, new CancellationToken());
                context.Notifications.Any().Should().BeTrue();
            }
        }

        private void SetupContextWithNotificationRegistrationData(Entities.NotificationRegistrationData notificationRegistrationData)
        {
            using (var context = new VoteMonitorContext(_dbContextOptions))
            {
                context.NotificationRegistrationData.Add(notificationRegistrationData);
                context.SaveChanges();
            }
        }

    }
}
