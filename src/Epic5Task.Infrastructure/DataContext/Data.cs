using Epic5Task.Application.Exceptions;
using Epic5Task.Domain.Entities;

namespace Epic5Task.Infrastructure.DataContext;

public static class Data
{
    private static List<EventCapacityEntity> _eventCapacities = [];
    private static List<EventEntity> _events = [];
    private static List<UserEntity> _users = [];
    private static List<TicketEntity> _tickets = [];
    private static List<TicketStatusHistoryEntity> _ticketStatusHistory = [];

    public static List<EventEntity> Events
    {
        get => _events;
    }

    public static int AddEvent(EventEntity eventEntity)
    {
        eventEntity.EventId = _events.Count + 1;
        _events.Add(eventEntity);
        return eventEntity.EventId;
    }

    public static List<UserEntity> Users
    {
        get => _users;
    }

    public static int AddUser(UserEntity userEntity)
    {
        userEntity.UserId = _users.Count + 1;
        _users.Add(userEntity);
        return userEntity.UserId;
    }

    public static List<EventCapacityEntity> EventCapacities
    {
        get => _eventCapacities;
    }

    public static void AddEventCapacity(EventCapacityEntity eventCapacityEntity)
    {
        if (!_events.Any(e => e.EventId == eventCapacityEntity.EventId))
        {
            throw new EntityNotFoundException("Event not found");
        }

        eventCapacityEntity.EventCapacityId = _eventCapacities.Count(x => x.EventId == eventCapacityEntity.EventId) + 1;
        _eventCapacities.Add(eventCapacityEntity);
    }

    public static void RemoveEventCapacity(params EventCapacityEntity[] capacityToRemove)
    {
        foreach (var capacity in capacityToRemove)
            _eventCapacities.Remove(capacity);
    }

    public static List<TicketEntity> Tickets
    {
        get => _tickets;
    }
    
    public static int AddTicket(TicketEntity ticketEntity)
    {
        ticketEntity.TicketId = _tickets.Count + 1;
        _tickets.Add(ticketEntity);
        return ticketEntity.TicketId;
    }

    public static List<TicketStatusHistoryEntity> TicketStatusHistory
    {
        get => _ticketStatusHistory;
    }
    
    public static void AddTicketStatusHistory(TicketStatusHistoryEntity ticketStatusHistoryEntity)
    {
        ticketStatusHistoryEntity.TicketStatusHistoryId = _ticketStatusHistory.Count + 1;
        _ticketStatusHistory.Add(ticketStatusHistoryEntity);
    }
}