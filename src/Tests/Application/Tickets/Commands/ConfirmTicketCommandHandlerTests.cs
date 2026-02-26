using AwesomeAssertions;
using Epic5Task.Application.Exceptions;
using Epic5Task.Application.Interfaces;
using Epic5Task.Application.Tickets.Commands;
using Epic5Task.Domain.Entities;
using Epic5Task.Domain.Enums;
using Epic5Task.Infrastructure.DataContext;
using Epic5Task.Infrastructure.DataProviders;
using Moq;
using NUnit.Framework;

namespace Epic5Task.Tests.Application.Tickets.Commands;

[TestFixture]
public class ConfirmTicketCommandHandlerTests
{
    private Mock<IUserContextProvider> _userContextProviderMock;
    private ITicketProvider _ticketProvider;
    private IEventProvider _eventProvider;
    private IUserProvider _userProvider;
    private ConfirmTicketCommandHandler _handler;

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

        // Use real providers for integration testing
        _userProvider = new UserProvider();
        _eventProvider = new EventProvider(_userProvider);
        _ticketProvider = new TicketProvider(_eventProvider);

        _handler = new ConfirmTicketCommandHandler(_ticketProvider, _userProvider, _userContextProviderMock.Object,
            _eventProvider);
    }

    [Test]
    public async Task Handle_ShouldConfirmTicket_WhenRequestIsValid()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        var userId = Data.AddUser(new UserEntity { PhoneNumber = phoneNumber });
        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Test Event",
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EventTime = TimeOnly.FromDateTime(DateTime.UtcNow),
            EventType = EventTypesEnum.Concert,
            EventOwner = userId
        });
        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 100,
            TicketCapacityLimit = 10,
            TicketSold = 0
        });

        var ticketId = Data.AddTicket(new TicketEntity
        {
            EventId = eventId,
            UserId = userId,
            TicketType = TicketTypesEnum.Regular,
            TicketStatus = TicketStatusesEnum.Pending,
            CreatedAt = DateTime.UtcNow
        });

        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var command = new ConfirmTicketCommand(ticketId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TicketId.Should().Be(ticketId);
        result.TicketStatus.Should().Be(TicketStatusesEnum.Confirmed);

        // Verify Data
        var updatedTicket = Data.Tickets.Single(x => x.TicketId == ticketId);
        updatedTicket.TicketStatus.Should().Be(TicketStatusesEnum.Confirmed);

        var capacity = Data.EventCapacities.Single(x => x.EventId == eventId && x.TicketType == TicketTypesEnum.Regular);
        capacity.TicketSold.Should().Be(1);
    }

    [Test]
    public void Handle_ShouldThrowEntityConflictException_WhenTicketIsNotPending()
    {
        // Arrange
        var ticketId = Data.AddTicket(new TicketEntity
        {
            TicketStatus = TicketStatusesEnum.Confirmed,
            CreatedAt = DateTime.UtcNow
        });

        var command = new ConfirmTicketCommand(ticketId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<EntityConflictException>().WithMessage("Ticket is not pending");
    }

    [Test]
    public void Handle_ShouldThrowAuthZException_WhenUserIsNotOwner()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        var otherPhoneNumber = "+380997654321";
        
        var ownerId = Data.AddUser(new UserEntity { PhoneNumber = phoneNumber });
        var strangerId = Data.AddUser(new UserEntity { PhoneNumber = otherPhoneNumber });

        var ticketId = Data.AddTicket(new TicketEntity
        {
            UserId = ownerId,
            TicketStatus = TicketStatusesEnum.Pending,
            CreatedAt = DateTime.UtcNow
        });

        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(otherPhoneNumber);

        var command = new ConfirmTicketCommand(ticketId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<AuthZException>().WithMessage("User is not authorized to confirm this ticket");
    }

    [Test]
    public void Handle_ShouldThrowInvalidOperationException_WhenEventCapacityNotFound()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        var userId = Data.AddUser(new UserEntity { PhoneNumber = phoneNumber });
        var eventId = Data.AddEvent(new EventEntity
        {
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EventTime = TimeOnly.FromDateTime(DateTime.UtcNow),
        });
        // No capacities added

        var ticketId = Data.AddTicket(new TicketEntity
        {
            EventId = eventId,
            UserId = userId,
            TicketType = TicketTypesEnum.Regular,
            TicketStatus = TicketStatusesEnum.Pending,
            CreatedAt = DateTime.UtcNow
        });

        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var command = new ConfirmTicketCommand(ticketId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*capacity for ticket type Regular*");
    }

    [Test]
    public void Handle_ShouldThrowInvalidOperationException_WhenEventCapacityIsFull()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        var userId = Data.AddUser(new UserEntity { PhoneNumber = phoneNumber });
        var eventId = Data.AddEvent(new EventEntity
        {
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EventTime = TimeOnly.FromDateTime(DateTime.UtcNow),
        });
        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.Regular,
            TicketCapacityLimit = 5,
            TicketSold = 5
        });

        var ticketId = Data.AddTicket(new TicketEntity
        {
            EventId = eventId,
            UserId = userId,
            TicketType = TicketTypesEnum.Regular,
            TicketStatus = TicketStatusesEnum.Pending,
            CreatedAt = DateTime.UtcNow
        });

        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var command = new ConfirmTicketCommand(ticketId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*enough capacity*");
    }

    [Test]
    public async Task Handle_ShouldThrowEntityConflictException_AndSetExpired_WhenTicketIsExpired()
    {
        // Arrange
        var phoneNumber = "+380991234567";
        var userId = Data.AddUser(new UserEntity { PhoneNumber = phoneNumber });
        var eventId = Data.AddEvent(new EventEntity
        {
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EventTime = TimeOnly.FromDateTime(DateTime.UtcNow),
        });
        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.Regular,
            TicketCapacityLimit = 10,
            TicketSold = 0
        });

        var ticketId = Data.AddTicket(new TicketEntity
        {
            EventId = eventId,
            UserId = userId,
            TicketType = TicketTypesEnum.Regular,
            TicketStatus = TicketStatusesEnum.Pending,
            CreatedAt = DateTime.UtcNow.AddMinutes(-16) // Expired
        });

        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(phoneNumber);

        var command = new ConfirmTicketCommand(ticketId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityConflictException>().WithMessage("Ticket is expired");

        // Verify status changed to Expired in the handler before throwing
        var updatedTicket = Data.Tickets.Single(x => x.TicketId == ticketId);
        updatedTicket.TicketStatus.Should().Be(TicketStatusesEnum.Expired);
    }
}
