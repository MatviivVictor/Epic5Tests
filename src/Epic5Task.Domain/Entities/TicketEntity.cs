using Epic5Task.Domain.Enums;

namespace Epic5Task.Domain.Entities;

public class TicketEntity
{
    public int TicketId { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public TicketTypesEnum TicketType { get; set; }
    public TicketStatusesEnum TicketStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}