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

        Assert.That(id1, Is.EqualTo(1));
        Assert.That(id2, Is.EqualTo(2));
        Assert.That(Data.Events, Has.Count.EqualTo(2));
        Assert.That(Data.Events[0].EventId, Is.EqualTo(1));
        Assert.That(Data.Events[1].EventId, Is.EqualTo(2));
    }

    [Test]
    public void AddUser_ShouldAssignSequentialId_AndStoreUser()
    {
        var id1 = Data.AddUser(new UserEntity { PhoneNumber = "+10000000001" });
        var id2 = Data.AddUser(new UserEntity { PhoneNumber = "+10000000002" });

        Assert.That(id1, Is.EqualTo(1));
        Assert.That(id2, Is.EqualTo(2));
        Assert.That(Data.Users, Has.Count.EqualTo(2));
        Assert.That(Data.Users[0].UserId, Is.EqualTo(1));
        Assert.That(Data.Users[1].UserId, Is.EqualTo(2));
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

        Assert.That(() => Data.AddEventCapacity(capacity),
            Throws.TypeOf<EntityNotFoundException>().With.Message.EqualTo("Event not found"));
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

        Assert.That(Data.EventCapacities, Has.Count.EqualTo(2));
        Assert.That(c1.EventCapacityId, Is.EqualTo(1));
        Assert.That(c2.EventCapacityId, Is.EqualTo(2));
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

        Assert.That(c11.EventCapacityId, Is.EqualTo(1));
        Assert.That(c21.EventCapacityId, Is.EqualTo(1));
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

        Assert.That(Data.EventCapacities, Has.Count.EqualTo(1));
        Assert.That(Data.EventCapacities.Single(), Is.SameAs(c2));
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

        Assert.That(id1, Is.EqualTo(1));
        Assert.That(id2, Is.EqualTo(2));
        Assert.That(ticket1.TicketId, Is.EqualTo(1));
        Assert.That(ticket2.TicketId, Is.EqualTo(2));
        Assert.That(Data.Tickets, Has.Count.EqualTo(2));
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

        Assert.That(h1.TicketStatusHistoryId, Is.EqualTo(1));
        Assert.That(h2.TicketStatusHistoryId, Is.EqualTo(2));
        Assert.That(Data.TicketStatusHistory, Has.Count.EqualTo(2));
    }
}