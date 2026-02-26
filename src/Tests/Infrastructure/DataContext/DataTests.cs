using AwesomeAssertions;
using Epic5Task.Application.Exceptions;
using Epic5Task.Domain.Entities;
using Epic5Task.Domain.Enums;
using Epic5Task.Infrastructure.DataContext;
using NUnit.Framework;

namespace Epic5Task.Infrastructure.Tests.DataContext;

[TestFixture]
public class DataTests
{
    [SetUp]
    public void SetUp()
    {
        // Data — статичний in-memory стан, тому ОБОВʼЯЗКОВО чистимо між тестами
        Data.Events.Clear();
        Data.Users.Clear();
        Data.EventCapacities.Clear();
        Data.Tickets.Clear();
        Data.TicketStatusHistory.Clear();
    }

    [Test]
    public void AddEvent_ShouldAssignSequentialId_AndStoreEvent()
    {
        var id1 = Data.AddEvent(new EventEntity
        {
            EventTitle = "E1",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(10, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        var id2 = Data.AddEvent(new EventEntity
        {
            EventTitle = "E2",
            EventDate = new DateOnly(2026, 1, 2),
            EventTime = new TimeOnly(11, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        id1.Should().Be(1);
        id2.Should().Be(2);

        Data.Events.Should().HaveCount(2);
        Data.Events[0].EventId.Should().Be(1);
        Data.Events[1].EventId.Should().Be(2);
    }

    [Test]
    public void AddUser_ShouldAssignSequentialId_AndStoreUser()
    {
        var id1 = Data.AddUser(new UserEntity { PhoneNumber = "+10000000001" });
        var id2 = Data.AddUser(new UserEntity { PhoneNumber = "+10000000002" });

        id1.Should().Be(1);
        id2.Should().Be(2);

        Data.Users.Should().HaveCount(2);
        Data.Users[0].UserId.Should().Be(1);
        Data.Users[1].UserId.Should().Be(2);
    }

    [Test]
    public void AddEventCapacity_WhenEventDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        var capacity = new EventCapacityEntity
        {
            EventId = 999,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 10,
            TicketCapacityLimit = 100,
            TicketSold = 0
        };

        var act = () => Data.AddEventCapacity(capacity);

        act.Should()
            .Throw<EntityNotFoundException>()
            .WithMessage("Event not found");
    }

    [Test]
    public void AddEventCapacity_WhenEventExists_ShouldAssignIdPerEvent_AndStoreCapacity()
    {
        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "E",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(10, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        var c1 = new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 10,
            TicketCapacityLimit = 100
        };

        var c2 = new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.VIP,
            TicketPrice = 20,
            TicketCapacityLimit = 50
        };

        Data.AddEventCapacity(c1);
        Data.AddEventCapacity(c2);

        Data.EventCapacities.Should().HaveCount(2);
        c1.EventCapacityId.Should().Be(1);
        c2.EventCapacityId.Should().Be(2);
    }

    [Test]
    public void AddEventCapacity_ShouldAssignIdIndependentlyForDifferentEvents()
    {
        var eventId1 = Data.AddEvent(new EventEntity
        {
            EventTitle = "E1",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(10, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        var eventId2 = Data.AddEvent(new EventEntity
        {
            EventTitle = "E2",
            EventDate = new DateOnly(2026, 1, 2),
            EventTime = new TimeOnly(11, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        var c11 = new EventCapacityEntity
        {
            EventId = eventId1,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 10,
            TicketCapacityLimit = 100
        };

        var c21 = new EventCapacityEntity
        {
            EventId = eventId2,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 10,
            TicketCapacityLimit = 100
        };

        Data.AddEventCapacity(c11);
        Data.AddEventCapacity(c21);

        c11.EventCapacityId.Should().Be(1);
        c21.EventCapacityId.Should().Be(1);
    }

    [Test]
    public void RemoveEventCapacity_ShouldRemoveProvidedCapacities()
    {
        var eventId = Data.AddEvent(new EventEntity
        {
            EventTitle = "E",
            EventDate = new DateOnly(2026, 1, 1),
            EventTime = new TimeOnly(10, 0),
            EventType = EventTypesEnum.Concert,
            EventOwner = 1
        });

        var c1 = new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.Regular,
            TicketPrice = 10,
            TicketCapacityLimit = 100
        };
        var c2 = new EventCapacityEntity
        {
            EventId = eventId,
            TicketType = TicketTypesEnum.VIP,
            TicketPrice = 20,
            TicketCapacityLimit = 50
        };

        Data.AddEventCapacity(c1);
        Data.AddEventCapacity(c2);

        Data.RemoveEventCapacity(c1);

        Data.EventCapacities.Should().HaveCount(1);
        Data.EventCapacities.Single().Should().BeSameAs(c2);
    }

    [Test]
    public void AddTicket_ShouldAssignSequentialId_AndStoreTicket()
    {
        var ticket1 = new TicketEntity
        {
            EventId = 1,
            UserId = 1,
            TicketType = TicketTypesEnum.Regular,
            TicketStatus = TicketStatusesEnum.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var ticket2 = new TicketEntity
        {
            EventId = 1,
            UserId = 2,
            TicketType = TicketTypesEnum.VIP,
            TicketStatus = TicketStatusesEnum.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var id1 = Data.AddTicket(ticket1);
        var id2 = Data.AddTicket(ticket2);

        id1.Should().Be(1);
        id2.Should().Be(2);

        ticket1.TicketId.Should().Be(1);
        ticket2.TicketId.Should().Be(2);

        Data.Tickets.Should().HaveCount(2);
    }

    [Test]
    public void AddTicketStatusHistory_ShouldAssignSequentialId_AndStoreHistory()
    {
        var h1 = new TicketStatusHistoryEntity
        {
            TicketId = 1,
            TicketStatus = TicketStatusesEnum.Pending,
            StatusChangedAt = DateTime.UtcNow,
            StatusChangedBy = 1
        };

        var h2 = new TicketStatusHistoryEntity
        {
            TicketId = 1,
            TicketStatus = TicketStatusesEnum.Confirmed,
            StatusChangedAt = DateTime.UtcNow,
            StatusChangedBy = 1
        };

        Data.AddTicketStatusHistory(h1);
        Data.AddTicketStatusHistory(h2);

        h1.TicketStatusHistoryId.Should().Be(1);
        h2.TicketStatusHistoryId.Should().Be(2);

        Data.TicketStatusHistory.Should().HaveCount(2);
    }
}