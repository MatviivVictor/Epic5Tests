using AwesomeAssertions;
using Epic5Task.Application.Events.Commands;
using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Exceptions;
using Epic5Task.Application.Interfaces;
using Epic5Task.Domain.Entities;
using Epic5Task.Domain.Enums;
using Epic5Task.Infrastructure.DataContext;
using Epic5Task.Infrastructure.DataProviders;
using Moq;
using NUnit.Framework;

namespace Epic5Task.Tests.Application.Events.Commands;

[TestFixture]
public class UpdateEventCommandHandlerTests
{
    private Mock<IUserContextProvider> _userContextProviderMock;
    private IEventProvider _eventProvider;
    private IUserProvider _userProvider;
    private UpdateEventCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        // Clear in-memory data before each test
        Data.Events.Clear();
        Data.Users.Clear();
        Data.EventCapacities.Clear();
        Data.Tickets.Clear();
        Data.TicketStatusHistory.Clear();

        _userContextProviderMock = new Mock<IUserContextProvider>();

        // We use the real UserProvider and EventProvider for integration testing
        _userProvider = new UserProvider();
        _eventProvider = new EventProvider(_userProvider);

        _handler = new UpdateEventCommandHandler(_userContextProviderMock.Object, _eventProvider, _userProvider);
    }

    [Test]
    public async Task Handle_ShouldUpdateEventDetails_WhenEventExists()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var existingEventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Old Title",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Other,
            EventOwner = 1
        });

        var model = new EventRequestModel
        {
            EventTitle = "Updated Title",
            EventDate = new DateOnly(2026, 12, 31),
            EventTime = new TimeOnly(23, 59),
            EventType = EventTypesEnum.Concert,
            EventCapacities = []
        };

        var command = new UpdateEventCommand(existingEventId, model);

        // Act
        var resultEventId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        resultEventId.Should().Be(existingEventId);

        var updatedEvent = Data.Events.Single(x => x.EventId == existingEventId);
        updatedEvent.EventTitle.Should().Be(model.EventTitle);
        updatedEvent.EventDate.Should().Be(model.EventDate);
        updatedEvent.EventTime.Should().Be(model.EventTime);
        updatedEvent.EventType.Should().Be(model.EventType);
    }

    [Test]
    public async Task Handle_ShouldUpdateEventCapacities_WhenCapacitiesAreChanged()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var existingEventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Event with capacities",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Other,
            EventOwner = 1
        });

        // Add initial capacity (to be updated)
        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = existingEventId,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 100,
            TicketCapacityLimit = 50,
            TicketSold = 0
        });

        // Add initial capacity (to be removed)
        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = existingEventId,
            TicketType = TicketTypesEnum.VIP,
            TicketPrice = 500,
            TicketCapacityLimit = 10,
            TicketSold = 0
        });

        var model = new EventRequestModel
        {
            EventTitle = "Updated Title",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Other,
            EventCapacities = new[]
            {
                // Update Regular
                new EventCapacityRequestModel
                {
                    TicketType = TicketTypesEnum.Regular,
                    TicketPrice = 120,
                    TicketCapacityLimit = 60
                },
                // Add Student (New)
                new EventCapacityRequestModel
                {
                    TicketType = TicketTypesEnum.Student,
                    TicketPrice = 50,
                    TicketCapacityLimit = 100
                }
                // VIP is missing, should be removed
            }
        };

        var command = new UpdateEventCommand(existingEventId, model);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedCapacities = Data.EventCapacities.Where(x => x.EventId == existingEventId).ToList();
        updatedCapacities.Should().HaveCount(2);

        // Verify Regular updated
        var regular = updatedCapacities.Single(x => x.TicketType == TicketTypesEnum.Regular);
        regular.TicketPrice.Should().Be(120);
        regular.TicketCapacityLimit.Should().Be(60);

        // Verify Student added
        var student = updatedCapacities.Single(x => x.TicketType == TicketTypesEnum.Student);
        student.TicketPrice.Should().Be(50);
        student.TicketCapacityLimit.Should().Be(100);

        // Verify VIP removed
        updatedCapacities.Should().NotContain(x => x.TicketType == TicketTypesEnum.VIP);
    }

    [Test]
    public void Handle_ShouldThrowEntityNotFoundException_WhenEventDoesNotExist()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var model = new EventRequestModel
        {
            EventTitle = "Non-existent Event",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Other,
            EventCapacities = []
        };

        var command = new UpdateEventCommand(999, model);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<EntityNotFoundException>().WithMessage("Event not found");
    }

    [Test]
    public void Handle_ShouldThrowEntityConflictException_WhenCapacityLimitIsReducedBelowSold()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var existingEventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Sold Tickets Event",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Other,
            EventOwner = 1
        });

        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = existingEventId,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 100,
            TicketCapacityLimit = 50,
            TicketSold = 20
        });

        var model = new EventRequestModel
        {
            EventTitle = "Sold Tickets Event",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Other,
            EventCapacities = new[]
            {
                new EventCapacityRequestModel
                {
                    TicketType = TicketTypesEnum.Regular,
                    TicketPrice = 100,
                    TicketCapacityLimit = 10 // Attempting to set limit below 20 (sold)
                }
            }
        };

        var command = new UpdateEventCommand(existingEventId, model);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<EntityConflictException>().WithMessage("Ticket sold limit reached");
    }
}
