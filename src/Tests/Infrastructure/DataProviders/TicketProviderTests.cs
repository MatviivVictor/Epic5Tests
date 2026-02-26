using AwesomeAssertions;
using Epic5Task.Application.Interfaces;
using Epic5Task.Domain.AggregateRoot;
using Epic5Task.Domain.Entities;
using Epic5Task.Domain.Enums;
using Epic5Task.Infrastructure.DataContext;
using Epic5Task.Infrastructure.DataProviders;
using Moq;
using NUnit.Framework;

namespace Epic5Task.Infrastructure.Tests.DataProviders;

[TestFixture]
public class TicketProviderTests
{
    private Mock<IEventProvider> _eventProviderMock;
    private TicketProvider _ticketProvider;

    [SetUp]
    public void SetUp()
    {
        Data.Events.Clear();
        Data.Users.Clear();
        Data.EventCapacities.Clear();
        Data.Tickets.Clear();
        Data.TicketStatusHistory.Clear();

        _eventProviderMock = new Mock<IEventProvider>();
        _ticketProvider = new TicketProvider(_eventProviderMock.Object);
    }

    [Test]
    public void CreateTicket_ShouldAddTicketAndHistory_AndReturnTicketId()
    {
        // Arrange
        int eventId = 1;
        int userId = 10;
        TicketTypesEnum ticketType = TicketTypesEnum.VIP;

        // Act
        var ticketId = _ticketProvider.CreateTicket(eventId, userId, ticketType);

        // Assert
        ticketId.Should().Be(1);
        Data.Tickets.Should().HaveCount(1);
        var ticket = Data.Tickets.Single();
        ticket.TicketId.Should().Be(ticketId);
        ticket.EventId.Should().Be(eventId);
        ticket.UserId.Should().Be(userId);
        ticket.TicketType.Should().Be(ticketType);
        ticket.TicketStatus.Should().Be(TicketStatusesEnum.Pending);

        Data.TicketStatusHistory.Should().HaveCount(1);
        var history = Data.TicketStatusHistory.Single();
        history.TicketId.Should().Be(ticketId);
        history.TicketStatus.Should().Be(TicketStatusesEnum.Pending);
        history.StatusChangedBy.Should().Be(userId);
    }

    [Test]
    public void GetUserTickets_ShouldReturnTicketsForUser_OrderedByCreationDateDescending()
    {
        // Arrange
        int userId = 1;
        Data.AddEvent(new EventEntity { EventId = 1 });
        
        var t1 = new TicketEntity { EventId = 1, UserId = userId, CreatedAt = DateTime.Now.AddMinutes(-10), TicketStatus = TicketStatusesEnum.Pending };
        var t2 = new TicketEntity { EventId = 1, UserId = userId, CreatedAt = DateTime.Now, TicketStatus = TicketStatusesEnum.Pending };
        var t3 = new TicketEntity { EventId = 1, UserId = 2, CreatedAt = DateTime.Now, TicketStatus = TicketStatusesEnum.Pending };

        Data.AddTicket(t1);
        Data.AddTicket(t2);
        Data.AddTicket(t3);

        // Act
        var tickets = _ticketProvider.GetUserTickets(userId);

        // Assert
        tickets.Should().HaveCount(2);
        tickets[0].TicketId.Should().Be(t2.TicketId);
        tickets[1].TicketId.Should().Be(t1.TicketId);
    }

    [Test]
    [TestCase(TicketStatusesEnum.Pending, TicketStatusesEnum.Confirmed, true)]
    [TestCase(TicketStatusesEnum.Confirmed, TicketStatusesEnum.Cancelled, true)]
    [TestCase(TicketStatusesEnum.Pending, TicketStatusesEnum.Cancelled, false)]
    [TestCase(TicketStatusesEnum.Confirmed, TicketStatusesEnum.Expired, false)]
    public void UpdateTicketStatus_ShouldUpdateStatusAndHistory_AndCallEventProviderWhenNeeded(
        TicketStatusesEnum initialStatus, TicketStatusesEnum newStatus, bool expectCapacityUpdate)
    {
        // Arrange
        int eventId = 1;
        int userId = 10;
        Data.AddEvent(new EventEntity { EventId = eventId });
        var ticketEntity = new TicketEntity
        {
            TicketId = 1,
            EventId = eventId,
            UserId = userId,
            TicketType = TicketTypesEnum.Regular,
            TicketStatus = initialStatus
        };
        Data.AddTicket(ticketEntity);
        var ticket = new Ticket(ticketEntity, Data.Events.First());

        // Act
        _ticketProvider.UpdateTicketStatus(ticket, newStatus, userId);

        // Assert
        ticket.TicketStatus.Should().Be(newStatus);
        ticketEntity.TicketStatus.Should().Be(newStatus);
        
        Data.TicketStatusHistory.Should().HaveCount(1);
        var lastHistory = Data.TicketStatusHistory.Last();
        lastHistory.TicketStatus.Should().Be(newStatus);

        if (expectCapacityUpdate)
        {
            _eventProviderMock.Verify(x => x.UpdateEventCapacity(eventId, TicketTypesEnum.Regular, newStatus), Times.Once);
        }
        else
        {
            _eventProviderMock.Verify(x => x.UpdateEventCapacity(It.IsAny<int>(), It.IsAny<TicketTypesEnum>(), It.IsAny<TicketStatusesEnum>()), Times.Never);
        }
    }

    [Test]
    public void GetTicket_WhenExists_ShouldReturnTicket()
    {
        // Arrange
        Data.AddEvent(new EventEntity { EventId = 1 });
        var ticketId = Data.AddTicket(new TicketEntity { EventId = 1, UserId = 1 });

        // Act
        var ticket = _ticketProvider.GetTicket(ticketId);

        // Assert
        ticket.Should().NotBeNull();
        ticket.TicketId.Should().Be(ticketId);
    }

    [Test]
    public void GetTicket_WhenDoesNotExist_ShouldThrowException()
    {
        // Act
        var act = () => _ticketProvider.GetTicket(999);

        // Assert
        act.Should().Throw<Exception>().WithMessage("Ticket not found");
    }

    [Test]
    public void GetTicketHistory_WhenExists_ShouldReturnTicketWithHistory()
    {
        // Arrange
        int ticketId = 1;
        Data.AddTicket(new TicketEntity { TicketId = ticketId, EventId = 1 });
        Data.AddTicketStatusHistory(new TicketStatusHistoryEntity { TicketId = ticketId, TicketStatus = TicketStatusesEnum.Pending, StatusChangedAt = DateTime.Now.AddMinutes(-5) });
        Data.AddTicketStatusHistory(new TicketStatusHistoryEntity { TicketId = ticketId, TicketStatus = TicketStatusesEnum.Confirmed, StatusChangedAt = DateTime.Now });

        // Act
        var result = _ticketProvider.GetTicketHistory(ticketId);

        // Assert
        result.Should().NotBeNull();
        result.TicketId.Should().Be(ticketId);
        result.TicketStatusHistory.Should().HaveCount(2);
        result.TicketStatusHistory[0].TicketStatus.Should().Be(TicketStatusesEnum.Confirmed);
        result.TicketStatusHistory[1].TicketStatus.Should().Be(TicketStatusesEnum.Pending);
    }

    [Test]
    public void GetTicketHistory_WhenDoesNotExist_ShouldThrowException()
    {
        // Act
        var act = () => _ticketProvider.GetTicketHistory(999);

        // Assert
        act.Should().Throw<Exception>().WithMessage("Ticket not found");
    }

    [Test]
    public void SetNoRefund_ShouldUpdatePropertyInBothObjectAndData()
    {
        // Arrange
        var ticketEntity = new TicketEntity { TicketId = 1, NoRefund = false };
        Data.AddTicket(ticketEntity);
        var ticket = new Ticket(ticketEntity, new EventEntity());

        // Act
        _ticketProvider.SetNoRefund(ticket, true);

        // Assert
        ticket.NoRefund.Should().BeTrue();
        ticketEntity.NoRefund.Should().BeTrue();
    }
}
