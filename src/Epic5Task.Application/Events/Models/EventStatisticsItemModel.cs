using Epic5Task.Application.Events.Queries;
using Epic5Task.Domain.Enums;

namespace Epic5Task.Application.Events.Models;

public class EventStatisticsItemModel: AbstractEventItem
{
    public List<EventCapacityItem> EventCapacities { get; set; } = [];
}

public class EventCapacityItem
{
    public TicketTypesEnum TicketType { get; set; }
    public decimal TicketPrice { get; set; }
    public int TicketCapacityLimit { get; set; }
    public int TicketSold { get; set; }
}