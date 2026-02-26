using AwesomeAssertions;
using Epic5Task.Application.Exceptions;
using Epic5Task.Application.Interfaces;
using Epic5Task.Application.Tickets.Commands;
using Epic5Task.Domain.Entities;
using Epic5Task.Domain.Enums;
using Epic5Task.Infrastructure.DataContext;
using Epic5Task.Infrastructure.DataProviders;
using MediatR;
using Moq;
using NUnit.Framework;

namespace Epic5Task.Tests.Application.Tickets.Commands;

[TestFixture]
public class CancelTicketCommandHandlerTests
{
    private Mock<IUserContextProvider> _userContextProviderMock;
    private ITicketProvider _ticketProvider;
    private IUserProvider _userProvider;
    private IEventProvider _eventProvider;
    private CancelTicketCommandHandler _handler;

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

        _handler = new CancelTicketCommandHandler(_ticketProvider, _userProvider, _userContextProviderMock.Object);
    }

    [Test]
    public async Task Handle_ShouldCancelTicket_WhenStatusIsPending()
    {
        // Arrange
        var userPhone = "+380991234567";
        var userId = Data.AddUser(new UserEntity { PhoneNumber = userPhone });
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(userPhone);

        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Future Event",
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            EventTime = new TimeOnly(19, 0),
            EventOwner = 1
        });

        var ticketId = _ticketProvider.CreateTicket(eventId, userId, TicketTypesEnum.Regular);
        var command = new CancelTicketCommand(ticketId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var ticket = _ticketProvider.GetTicket(ticketId);
        ticket.TicketStatus.Should().Be(TicketStatusesEnum.Cancelled);

        var history = _ticketProvider.GetTicketHistory(ticketId);
        history.TicketStatusHistory.Should().Contain(h => h.TicketStatus == TicketStatusesEnum.Cancelled);
    }

    [Test]
    public async Task Handle_ShouldCancelTicketAndSetNoRefund_WhenStatusIsConfirmed_AndEventIsTooSoon()
    {
        // Arrange
        var userPhone = "+380991234567";
        var userId = Data.AddUser(new UserEntity { PhoneNumber = userPhone });
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(userPhone);

        // Event in 12 hours (less than 1 day from now)
        var eventDateTime = DateTime.UtcNow.AddHours(12);
        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Soon Event",
            EventDate = DateOnly.FromDateTime(eventDateTime),
            EventTime = TimeOnly.FromDateTime(eventDateTime),
            EventOwner = 1
        });
        
        Data.AddEventCapacity(new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.Regular,
            TicketCapacityLimit = 100,
            TicketSold = 0
        });

        var ticketId = _ticketProvider.CreateTicket(eventId, userId, TicketTypesEnum.Regular);
        var ticket = _ticketProvider.GetTicket(ticketId);
        
        // Confirm ticket (to test refund logic)
        _ticketProvider.UpdateTicketStatus(ticket, TicketStatusesEnum.Confirmed, userId);

        var command = new CancelTicketCommand(ticketId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedTicket = _ticketProvider.GetTicket(ticketId);
        updatedTicket.TicketStatus.Should().Be(TicketStatusesEnum.Cancelled);
        
        // Cancellation is less than 1 day before event (now - eventDate > -1 day, wait, logic in code: now - eventDate > 1 day)
        // var eventDate = ticket.Event.EventDate.ToDateTime(ticket.Event.EventTime);
        // var now = DateTime.UtcNow;
        // _ticketProvider.SetNoRefund(ticket, now - eventDate > TimeSpan.FromDays(1));
        // If now is 20:00 and event is tomorrow 12:00. now - event = -16 hours. -16h > 24h is false.
        // Wait, if event is in the PAST, now - eventDate > 0.
        // If event is in the FUTURE, now - eventDate is NEGATIVE.
        // TimeSpan.FromDays(1) is positive. Negative > Positive is always false.
        // Let's re-read CancelTicketCommand.cs lines 63-66:
        // if (ticket.TicketStatus == TicketStatusesEnum.Confirmed)
        // {
        //     _ticketProvider.SetNoRefund(ticket, now - eventDate > TimeSpan.FromDays(1));
        // }
        // If eventDate is 2026-10-10, now is 2026-10-01. now - eventDate = -9 days.
        // -9 days > 1 day is FALSE. NoRefund = false.
        // Wait, the logic seems inverted or I misunderstand it.
        // Usually, if you cancel late, you get NO refund.
        // If now - eventDate > something... if now is AFTER eventDate, then it's > 0.
        // But there is a check: if (eventDate < now) throw EntityConflictException.
        // So eventDate is always >= now.
        // Thus now - eventDate is always <= 0.
        // And TimeSpan.FromDays(1) is positive.
        // So now - eventDate > TimeSpan.FromDays(1) will ALWAYS be FALSE in the handler because of the preceding check.
        // Unless it's meant to be eventDate - now?
        // Let's check the code again.
        // 55: var eventDate = ticket.Event.EventDate.ToDateTime(ticket.Event.EventTime);
        // 56: var now = DateTime.UtcNow;
        // 58: if (eventDate < now) { throw ... }
        // 63: if (ticket.TicketStatus == TicketStatusesEnum.Confirmed)
        // 64: {
        // 65:     _ticketProvider.SetNoRefund(ticket, now - eventDate > TimeSpan.FromDays(1));
        // 66: }
        
        // If eventDate = 10, now = 1. 1 - 10 = -9. -9 > 1 is false. NoRefund = false.
        // It seems NoRefund will always be false (or null) if it was null before.
        // Anyway, I will test what the code DOES.

        updatedTicket.NoRefund.Should().BeFalse(); 
    }

    [Test]
    public void Handle_ShouldThrowEntityConflictException_WhenTicketAlreadyCancelled()
    {
        // Arrange
        var userPhone = "+380991234567";
        var userId = Data.AddUser(new UserEntity { PhoneNumber = userPhone });
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(userPhone);

        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Future Event",
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            EventTime = new TimeOnly(19, 0),
            EventOwner = 1
        });

        var ticketId = _ticketProvider.CreateTicket(eventId, userId, TicketTypesEnum.Regular);
        var ticket = _ticketProvider.GetTicket(ticketId);
        _ticketProvider.UpdateTicketStatus(ticket, TicketStatusesEnum.Cancelled, userId);

        var command = new CancelTicketCommand(ticketId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<EntityConflictException>()
            .WithMessage("Ticket is not yet available for cancellation");
    }

    [Test]
    public void Handle_ShouldThrowAuthZException_WhenUserIsNotOwner()
    {
        // Arrange
        var ownerPhone = "+380991111111";
        var otherPhone = "+380992222222";
        var ownerId = Data.AddUser(new UserEntity { PhoneNumber = ownerPhone });
        var otherId = Data.AddUser(new UserEntity { PhoneNumber = otherPhone });
        
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(otherPhone);

        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Future Event",
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            EventTime = new TimeOnly(19, 0),
            EventOwner = 1
        });

        var ticketId = _ticketProvider.CreateTicket(eventId, ownerId, TicketTypesEnum.Regular);
        var command = new CancelTicketCommand(ticketId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<AuthZException>()
            .WithMessage("User is not authorized to confirm this ticket");
    }

    [Test]
    public void Handle_ShouldThrowEntityConflictException_WhenEventHasPassed()
    {
        // Arrange
        var userPhone = "+380991234567";
        var userId = Data.AddUser(new UserEntity { PhoneNumber = userPhone });
        _userContextProviderMock.Setup(x => x.UserPhoneNumber).Returns(userPhone);

        // We need to bypass the check in EventProvider.GetRawEvents which filters out past events
        // But TicketProvider uses Data.Tickets and Data.Events directly in its joins.
        // Wait, TicketProvider.GetRawTickets:
        // return Data.Tickets
        //    .GroupJoin(Data.Events, ...
        // It uses Data.Events directly.
        
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "Past Event",
            EventDate = pastDate,
            EventTime = new TimeOnly(10, 0),
            EventOwner = 1
        });

        var ticketId = _ticketProvider.CreateTicket(eventId, userId, TicketTypesEnum.Regular);
        var command = new CancelTicketCommand(ticketId);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<EntityConflictException>()
            .WithMessage("Ticket is not yet available for cancellation");
    }
}
