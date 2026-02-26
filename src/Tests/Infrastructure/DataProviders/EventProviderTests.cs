using AwesomeAssertions;
using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Exceptions;
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
public class EventProviderTests
{
    private Mock<IUserProvider> _userProviderMock;
    private EventProvider _eventProvider;

    [SetUp]
    public void SetUp()
    {
        _userProviderMock = new Mock<IUserProvider>();
        _eventProvider = new EventProvider(_userProviderMock.Object);

        // Clear static data
        Data.Events.Clear();
        Data.Users.Clear();
        Data.EventCapacities.Clear();
        Data.Tickets.Clear();
        Data.TicketStatusHistory.Clear();
    }

    [Test]
    public void CreateEvent_ShouldAddEventAndCapacitiesToData_AndReturnEventId()
    {
        // Arrange
        var owner = "OwnerPhoneNumber";
        var ownerId = 123;
        _userProviderMock.Setup(x => x.GetUserId(owner)).Returns(ownerId);

        var request = new EventRequestModel
        {
            EventTitle = "Test Event",
            EventDate = new DateOnly(2026, 5, 20),
            EventTime = new TimeOnly(18, 0),
            EventType = EventTypesEnum.Concert,
            EventCapacities = new[]
            {
                new EventCapacityRequestModel { TicketType = TicketTypesEnum.Regular, TicketPrice = 50, TicketCapacityLimit = 100 },
                new EventCapacityRequestModel { TicketType = TicketTypesEnum.VIP, TicketPrice = 150, TicketCapacityLimit = 20 }
            }
        };

        // Act
        var resultId = _eventProvider.CreateEvent(request, owner);

        // Assert
        resultId.Should().Be(1);
        Data.Events.Should().HaveCount(1);
        var createdEvent = Data.Events[0];
        createdEvent.EventTitle.Should().Be(request.EventTitle);
        createdEvent.EventOwner.Should().Be(ownerId);

        Data.EventCapacities.Should().HaveCount(2);
        Data.EventCapacities.Should().AllSatisfy(c => c.EventId.Should().Be(resultId));
        Data.EventCapacities.Should().ContainSingle(c => c.TicketType == TicketTypesEnum.Regular && c.TicketPrice == 50);
        Data.EventCapacities.Should().ContainSingle(c => c.TicketType == TicketTypesEnum.VIP && c.TicketPrice == 150);
    }

    [Test]
    public void GetEvents_ShouldReturnEventsInCorrectOrder_FilteredByDate()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var futureTime = TimeOnly.FromDateTime(DateTime.UtcNow.AddHours(1));

        Data.AddEvent(new EventEntity { EventTitle = "Future 2", EventDate = futureDate.AddDays(1), EventTime = new TimeOnly(10, 0), EventId = 1 });
        Data.AddEvent(new EventEntity { EventTitle = "Future 1", EventDate = futureDate, EventTime = new TimeOnly(10, 0), EventId = 2 });
        Data.AddEvent(new EventEntity { EventTitle = "Past", EventDate = pastDate, EventTime = new TimeOnly(10, 0), EventId = 3 });
        
        // Today but future time
        Data.AddEvent(new EventEntity { EventTitle = "Today Future", EventDate = today, EventTime = futureTime, EventId = 4 });

        // Act
        var results = _eventProvider.GetEvents();

        // Assert
        results.Should().HaveCount(3);
        results[0].EventTitle.Should().Be("Today Future");
        results[1].EventTitle.Should().Be("Future 1");
        results[2].EventTitle.Should().Be("Future 2");
    }

    [Test]
    public void GetEvent_WhenExists_ShouldReturnEventWithCapacities()
    {
        // Arrange
        var eventId = Data.AddEvent(new EventEntity 
        { 
            EventTitle = "Test", 
            EventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            EventTime = new TimeOnly(12, 0)
        });
        Data.AddEventCapacity(new EventCapacityEntity { EventId = eventId, TicketType = TicketTypesEnum.Regular });

        // Act
        var result = _eventProvider.GetEvent(eventId);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(eventId);
        result.EventCapacities.Should().HaveCount(1);
    }

    [Test]
    public void GetEvent_WhenDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Act
        var act = () => _eventProvider.GetEvent(999);

        // Assert
        act.Should().Throw<EntityNotFoundException>().WithMessage("Event not found");
    }

    [Test]
    public void UpdateEvent_WhenDoesNotExist_ShouldThrowEntityNotFoundException()
    {
        // Act
        var act = () => _eventProvider.UpdateEvent(999, new EventRequestModel(), 1);

        // Assert
        act.Should().Throw<EntityNotFoundException>().WithMessage("Event not found");
    }

    [Test]
    public void UpdateEvent_ShouldUpdatePropertiesAndHandleCapacities()
    {
        // Arrange
        var eventId = Data.AddEvent(new EventEntity 
        { 
            EventTitle = "Old Title", 
            EventDate = new DateOnly(2026, 1, 1), 
            EventTime = new TimeOnly(10, 0),
            EventType = EventTypesEnum.Concert
        });
        
        // Existing capacity
        Data.AddEventCapacity(new EventCapacityEntity 
        { 
            EventId = eventId, 
            TicketType = TicketTypesEnum.Regular, 
            TicketPrice = 10, 
            TicketCapacityLimit = 100, 
            TicketSold = 5 
        });
        
        // Capacity to be removed
        Data.AddEventCapacity(new EventCapacityEntity 
        { 
            EventId = eventId, 
            TicketType = TicketTypesEnum.VIP, 
            TicketPrice = 50, 
            TicketCapacityLimit = 10 
        });

        var request = new EventRequestModel
        {
            EventTitle = "New Title",
            EventDate = new DateOnly(2026, 2, 2),
            EventTime = new TimeOnly(11, 0),
            EventType = EventTypesEnum.Conference,
            EventCapacities = new[]
            {
                // Update existing
                new EventCapacityRequestModel { TicketType = TicketTypesEnum.Regular, TicketPrice = 15, TicketCapacityLimit = 150 },
                // Add new
                new EventCapacityRequestModel { TicketType = TicketTypesEnum.Student, TicketPrice = 5, TicketCapacityLimit = 1000 }
                // VIP is missing, should be removed
            }
        };

        // Act
        _eventProvider.UpdateEvent(eventId, request, 1);

        // Assert
        var updatedEvent = Data.Events.First(x => x.EventId == eventId);
        updatedEvent.EventTitle.Should().Be("New Title");
        updatedEvent.EventType.Should().Be(EventTypesEnum.Conference);

        var capacities = Data.EventCapacities.Where(x => x.EventId == eventId).ToList();
        capacities.Should().HaveCount(2);
        
        var regular = capacities.First(x => x.TicketType == TicketTypesEnum.Regular);
        regular.TicketPrice.Should().Be(15);
        regular.TicketCapacityLimit.Should().Be(150);
        regular.TicketSold.Should().Be(5); // Should remain same

        var student = capacities.First(x => x.TicketType == TicketTypesEnum.Student);
        student.TicketPrice.Should().Be(5);
        student.TicketCapacityLimit.Should().Be(1000);

        capacities.Should().NotContain(x => x.TicketType == TicketTypesEnum.VIP);
    }

    [Test]
    public void UpdateEvent_WhenUpdatingCapacityLimitBelowSold_ShouldThrowEntityConflictException()
    {
        // Arrange
        var eventId = Data.AddEvent(new EventEntity { EventTitle = "E" });
        Data.AddEventCapacity(new EventCapacityEntity 
        { 
            EventId = eventId, 
            TicketType = TicketTypesEnum.Regular, 
            TicketSold = 10, 
            TicketCapacityLimit = 20 
        });

        var request = new EventRequestModel
        {
            EventTitle = "E",
            EventCapacities = new[]
            {
                new EventCapacityRequestModel { TicketType = TicketTypesEnum.Regular, TicketCapacityLimit = 5 } // 5 < 10
            }
        };

        // Act
        var act = () => _eventProvider.UpdateEvent(eventId, request, 1);

        // Assert
        act.Should().Throw<EntityConflictException>().WithMessage("Ticket sold limit reached");
    }

    [Test]
    public void UpdateEventCapacity_ShouldCorrectlyUpdateSoldTickets()
    {
        // Arrange
        var eventId = Data.AddEvent(new EventEntity { EventTitle = "E" });
        Data.AddEventCapacity(new EventCapacityEntity 
        { 
            EventId = eventId, 
            TicketType = TicketTypesEnum.Regular, 
            TicketSold = 10 
        });

        // Act & Assert
        
        // Confirmed -> Increment
        _eventProvider.UpdateEventCapacity(eventId, TicketTypesEnum.Regular, TicketStatusesEnum.Confirmed);
        Data.EventCapacities.First().TicketSold.Should().Be(11);

        // Cancelled -> Decrement
        _eventProvider.UpdateEventCapacity(eventId, TicketTypesEnum.Regular, TicketStatusesEnum.Cancelled);
        Data.EventCapacities.First().TicketSold.Should().Be(10);

        // Pending -> No change
        _eventProvider.UpdateEventCapacity(eventId, TicketTypesEnum.Regular, TicketStatusesEnum.Pending);
        Data.EventCapacities.First().TicketSold.Should().Be(10);

        // Expired -> No change
        _eventProvider.UpdateEventCapacity(eventId, TicketTypesEnum.Regular, TicketStatusesEnum.Expired);
        Data.EventCapacities.First().TicketSold.Should().Be(10);
    }

    [Test]
    public void UpdateEventCapacity_WhenCapacityNotFound_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        var eventId = Data.AddEvent(new EventEntity { EventTitle = "E" });

        // Act
        var act = () => _eventProvider.UpdateEventCapacity(eventId, TicketTypesEnum.Regular, TicketStatusesEnum.Confirmed);

        // Assert
        act.Should().Throw<EntityNotFoundException>().WithMessage("Event or Event's capacity not found");
    }
}
