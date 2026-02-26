using Epic5Task.Application.Events.Queries;
using Epic5Task.Domain.AggregateRoot;

namespace Epic5Task.Application.Events.Mappers;

public static class Mapper
{
   public static Func<Event, EventListItem> Map = x => new EventListItem
    {
        EventId = x.EventId,
        EventTitle = x.EventTitle,
        EventDate = x.EventDate,
        EventTime = x.EventTime,
        EventType = x.EventType,
        TicketTypes = x.EventCapacities.Where(c => c.TicketSold< c.TicketCapacityLimit)
            .Select(c => c.TicketType).ToList()
    };
}