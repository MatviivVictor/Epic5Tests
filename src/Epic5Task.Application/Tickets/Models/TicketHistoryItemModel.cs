using Epic5Task.Domain.Enums;

namespace Epic5Task.Application.Tickets.Models;

public class TicketHistoryItemModel
{
    public DateTime StatusChangedAt { get; set; }
    public TicketStatusesEnum TicketStatus { get; set; }
}