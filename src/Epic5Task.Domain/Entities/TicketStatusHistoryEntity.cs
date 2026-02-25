using Epic5Task.Domain.Enums;

namespace Epic5Task.Domain.Entities;

public class TicketStatusHistoryEntity
{
    public int TicketStatusHistoryId { get; set; }
    public int TicketId { get; set; }
    public TicketStatusesEnum TicketStatus { get; set; }
    public DateTime StatusChangedAt { get; set; }
    public int StatusChangedBy { get; set; }
}