using Epic5Task.Domain.Enums;

namespace Epic5Task.Domain.Entities;

public class EventCapacityEntity
{
    public int EventCapacityId { get; set; }
    public int EventId { get; set; }
    public TicketTypesEnum TicketType { get; set; }
    public decimal TicketPrice { get; set; }
    public int TicketCapacityLimit { get; set; }
    public int TicketSold { get; set; }
}