using Epic5Task.Domain.Enums;

namespace Epic5Task.Application.Events.Models;

public class EventRequestModel
{
    public string EventTitle { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly EventTime { get; set; }
    public EventTypesEnum EventType { get; set; }
    public EventCapacityRequestModel[] EventCapacities { get; set; } = [];
}

public class EventCapacityRequestModel
{
    public TicketTypesEnum TicketType { get; set; }
    public decimal TicketPrice { get; set; }
    public int TicketCapacityLimit { get; set; }
}

