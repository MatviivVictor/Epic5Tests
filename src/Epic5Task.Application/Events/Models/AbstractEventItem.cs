using Epic5Task.Domain.Enums;

namespace Epic5Task.Application.Events.Queries;

public abstract class AbstractEventItem
{
    public int EventId { get; set; }
    public string EventTitle { get; set; } = null!;
    public DateOnly EventDate { get; set; }
    public TimeOnly EventTime { get; set; }
    public EventTypesEnum EventType { get; set; }
}