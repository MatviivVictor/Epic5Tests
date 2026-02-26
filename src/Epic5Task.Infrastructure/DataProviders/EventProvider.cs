using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Exceptions;
using Epic5Task.Application.Interfaces;
using Epic5Task.Domain.AggregateRoot;
using Epic5Task.Domain.Entities;
using Epic5Task.Domain.Enums;

namespace Epic5Task.Infrastructure.DataProviders;

public class EventProvider : IEventProvider
{
    private readonly IUserProvider _userProvider;

    public EventProvider(IUserProvider userProvider)
    {
        _userProvider = userProvider;
    }

    public int CreateEvent(EventRequestModel model, string owner)
    {
        var ownerId = _userProvider.GetUserId(owner);
        var @event = new EventEntity
        {
            EventId = 0,
            EventTitle = model.EventTitle,
            EventDate = model.EventDate,
            EventTime = model.EventTime,
            EventType = model.EventType,
            EventOwner = ownerId
        };

        DataContext.Data.AddEvent(@event);

        foreach (var capacity in model.EventCapacities)
        {
            DataContext.Data.AddEventCapacity(new EventCapacityEntity
            {
                EventId = @event.EventId,
                TicketType = capacity.TicketType,
                TicketPrice = capacity.TicketPrice,
                TicketCapacityLimit = capacity.TicketCapacityLimit,
                TicketSold = 0
            });
        }

        return @event.EventId;
    }

    public List<@Event> GetEvents()
    {
        var events = GetRawEvents()
            .OrderBy(x => x.EventDate).ThenBy(x => x.EventTime)
            .ToList();

        return events;
    }

    private static IEnumerable<Event> GetRawEvents()
    {
        return DataContext.Data.Events.Where(x => x.EventDate.ToDateTime(x.EventTime) >= DateTime.UtcNow)
            .GroupJoin(DataContext.Data.EventCapacities,
                x => x.EventId,
                x => x.EventId, (x, y) => new
                {
                    Event = x,
                    Capacities = y.ToList()
                })
            .Select(x => new Event(x.Event, x.Capacities)).AsEnumerable();
    }

    public void UpdateEvent(int requestEventId, EventRequestModel requestModel, int userId)
    {
        var entityEvent = DataContext.Data.Events.FirstOrDefault(x => x.EventId == requestEventId);
        if (entityEvent == null)
        {
            throw new EntityNotFoundException("Event not found");
        }

        entityEvent.EventTitle = requestModel.EventTitle;
        entityEvent.EventDate = requestModel.EventDate;
        entityEvent.EventTime = requestModel.EventTime;
        entityEvent.EventType = requestModel.EventType;

        var capacities = DataContext.Data.EventCapacities.Where(x => x.EventId == requestEventId).ToList();

        var capacitiesToRemove = capacities
            .Where(x => !requestModel.EventCapacities.Any(m => m.TicketType == x.TicketType)).ToList();
        foreach (var capacityToRemove in capacitiesToRemove)
        {
            DataContext.Data.RemoveEventCapacity(capacityToRemove);
        }
        
        var capacitiesModelForUpdate = requestModel.EventCapacities
            .Where(x => capacities.Any(c => c.TicketType == x.TicketType)).ToList();

        var index = 1;
        foreach (var capacityModel in capacitiesModelForUpdate)
        {
            var capacityToUpdate = capacities.First(x => x.TicketType == capacityModel.TicketType);
            if (capacityToUpdate.TicketSold > capacityModel.TicketCapacityLimit)
            {
                throw new EntityConflictException("Ticket sold limit reached");
            }

            capacityToUpdate.TicketPrice = capacityModel.TicketPrice;
            capacityToUpdate.TicketCapacityLimit = capacityModel.TicketCapacityLimit;
            capacityToUpdate.EventCapacityId = index++;
        }

        var newCapacities = requestModel.EventCapacities.Where(x => !capacities.Any(c => c.TicketType == x.TicketType)).ToList();
        foreach (var capacity in newCapacities)
        {
            DataContext.Data.AddEventCapacity(new EventCapacityEntity
                {
                    EventId = entityEvent.EventId,
                    TicketType = capacity.TicketType,
                    TicketPrice = capacity.TicketPrice,
                    TicketCapacityLimit = capacity.TicketCapacityLimit,
                    TicketSold = 0
                }
            );
        }
        
    }

    public Event GetEvent(int eventId)
    {
        var @event = GetRawEvents().FirstOrDefault(x => x.EventId  == eventId);
        return @event ?? throw new EntityNotFoundException("Event not found");  
    }

    public void UpdateEventCapacity(int eventId, TicketTypesEnum ticketType, TicketStatusesEnum ticketStatus)
    {
        var eventCapacity = DataContext.Data.EventCapacities
            .FirstOrDefault(x => x.EventId == eventId && x.TicketType == ticketType);
        
        if (eventCapacity == null)
        {
            throw new EntityNotFoundException("Event or Event's capacity not found");
        }

        switch (ticketStatus)
        {
            case TicketStatusesEnum.Pending:
                break;
            case TicketStatusesEnum.Confirmed:
                eventCapacity.TicketSold++;
                break;
            case TicketStatusesEnum.Cancelled:
                eventCapacity.TicketSold--;
                break;
            case TicketStatusesEnum.Expired:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ticketStatus), ticketStatus, null);
        }
    }
}