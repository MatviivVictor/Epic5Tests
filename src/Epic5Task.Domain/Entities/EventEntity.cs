using Epic5Task.Domain.Enums;

namespace Epic5Task.Domain.Entities;

public class EventEntity
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public DateOnly EventDate { get; set; }
    public TimeOnly EventTime { get; set; }
    public EventTypesEnum EventType { get; set; }
    public int EventOwner { get; set; }
}