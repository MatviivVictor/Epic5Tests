using AwesomeAssertions;
using Epic5Task.Application.Events.Commands;
using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Interfaces;
using Epic5Task.Domain.Entities;
using Epic5Task.Domain.Enums;
using Epic5Task.Infrastructure.DataContext;
using Epic5Task.Infrastructure.DataProviders;
using Moq;
using NUnit.Framework;

namespace Epic5Task.Tests.Application.Events.Commands;

[TestFixture]
public class CreateTicketsCommandHandlerTests
{
    private Mock<IUserContextProvider> _userContextProviderMock;
    private IEventProvider _eventProvider;
    private IUserProvider _userProvider;
    private ITicketProvider _ticketProvider;
    private CreateTicketsCommandHandler _handler;

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
        _userProvider = new UserProvider();
        _eventProvider = new EventProvider(_userProvider);
        _ticketProvider = new TicketProvider(_eventProvider);

        _handler = new CreateTicketsCommandHandler(
            _eventProvider,
            _userContextProviderMock.Object,
            _userProvider,
            _ticketProvider);
    }

    [Test]
    public async Task Handle_ShouldCreateTickets_WhenRequestIsValid()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Test Event",
            EventDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 100,
            TicketCapacityLimit = 10,
            TicketSold = 0
        });

        var model = new CreateTicketsRequestModel
        {
            Tickets = new[]
            {
                new BookingTicketsModel { TicketType = TicketTypesEnum.Regular, Quantity = 2 }
            }
        };

        var command = new CreateTicketsCommand(eventId, model);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        Data.Tickets.Should().HaveCount(2);
        Data.Tickets.All(x => x.EventId == eventId).Should().BeTrue();
        Data.Tickets.All(x => x.TicketType == TicketTypesEnum.Regular).Should().BeTrue();
        Data.Tickets.All(x => x.TicketStatus == TicketStatusesEnum.Pending).Should().BeTrue();
        
        var user = Data.Users.Single(u => u.PhoneNumber == phoneNumber);
        Data.Tickets.All(x => x.UserId == user.UserId).Should().BeTrue();
        
        Data.TicketStatusHistory.Should().HaveCount(2);
    }

    [Test]
    public void Handle_ShouldThrowInvalidOperationException_WhenEventDoesNotHaveCapacityForTicketType()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Test Event",
            EventDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        // Event has no capacities defined

        var model = new CreateTicketsRequestModel
        {
            Tickets = new[]
            {
                new BookingTicketsModel { TicketType = TicketTypesEnum.Regular, Quantity = 1 }
            }
        };

        var command = new CreateTicketsCommand(eventId, model);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Event does not have capacity for ticket type Regular");
    }

    [Test]
    public void Handle_ShouldThrowInvalidOperationException_WhenNotEnoughCapacity()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Test Event",
            EventDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 100,
            TicketCapacityLimit = 5,
            TicketSold = 4
        });

        var model = new CreateTicketsRequestModel
        {
            Tickets = new[]
            {
                new BookingTicketsModel { TicketType = TicketTypesEnum.Regular, Quantity = 2 } // Request 2, but only 1 left
            }
        };

        var command = new CreateTicketsCommand(eventId, model);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Event does not have enough capacity for ticket type Regular");
    }

    [Test]
    public async Task Handle_ShouldCreateUser_WhenUserDoesNotExist()
    {
        // Arrange
        var phoneNumber = "+380990000000";
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Test Event",
            EventDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            EventTime = new TimeOnly(12, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.VIP,
            TicketPrice = 500,
            TicketCapacityLimit = 10,
            TicketSold = 0
        });

        var model = new CreateTicketsRequestModel
        {
            Tickets = new[]
            {
                new BookingTicketsModel { TicketType = TicketTypesEnum.VIP, Quantity = 1 }
            }
        };

        var command = new CreateTicketsCommand(eventId, model);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Data.Users.Should().Contain(u => u.PhoneNumber == phoneNumber);
        var user = Data.Users.Single(u => u.PhoneNumber == phoneNumber);
        Data.Tickets.Single().UserId.Should().Be(user.UserId);
    }
}
