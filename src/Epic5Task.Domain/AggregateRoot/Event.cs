using Epic5Task.Domain.Entities;

namespace Epic5Task.Domain.AggregateRoot;

public class @Event: EventEntity
{
    public List<EventCapacityEntity> EventCapacities { get; set; } = [];

    public Event()
    {
        
    }
    
    public Event(EventEntity entity)
    {
        EventId = entity.EventId;
        EventTitle = entity.EventTitle;
        EventDate = entity.EventDate;
        EventTime = entity.EventTime;
        EventType = entity.EventType;
        EventOwner = entity.EventOwner;
    }

    public Event(EventEntity entity, List<EventCapacityEntity> capacities): this(entity)
    {
        EventCapacities = capacities;
    }
}