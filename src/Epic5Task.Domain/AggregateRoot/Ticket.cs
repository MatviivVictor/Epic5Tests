using Epic5Task.Domain.Entities;

namespace Epic5Task.Domain.AggregateRoot;

public class Ticket: TicketEntity
{
    public EventEntity Event { get; set; }

    public Ticket()
    {
        
    }

    public Ticket(TicketEntity entity, EventEntity @event)
    {
        TicketId = entity.TicketId;
        EventId = entity.EventId;
        UserId = entity.UserId;
        TicketType = entity.TicketType;
        TicketStatus = entity.TicketStatus;
        CreatedAt = entity.CreatedAt;   
        ConfirmedAt = entity.ConfirmedAt;
        NoRefund = entity.NoRefund;
        Event = @event;
    }
}