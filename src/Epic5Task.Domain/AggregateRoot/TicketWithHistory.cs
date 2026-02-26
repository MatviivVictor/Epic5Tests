using Epic5Task.Domain.Entities;

namespace Epic5Task.Domain.AggregateRoot;

public class TicketWithHistory : TicketEntity
{
    public List<TicketStatusHistoryEntity> TicketStatusHistory { get; set; }

    public TicketWithHistory()
    {
        
    }

    public TicketWithHistory(TicketEntity entity, List<TicketStatusHistoryEntity> statusHistory)
    {
        TicketId = entity.TicketId;
        EventId = entity.EventId;
        UserId = entity.UserId;
        TicketType = entity.TicketType;
        TicketStatus = entity.TicketStatus;
        CreatedAt = entity.CreatedAt;   
        ConfirmedAt = entity.ConfirmedAt;
        NoRefund = entity.NoRefund;
        TicketStatusHistory = statusHistory;
    }
}