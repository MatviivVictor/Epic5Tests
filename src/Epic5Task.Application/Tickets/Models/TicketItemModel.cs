using Epic5Task.Domain.AggregateRoot;
using Epic5Task.Domain.Enums;

namespace Epic5Task.Application.Tickets.Models;

public class TicketItemModel
{
    public int TicketId { get; set; }
    public string EventTitle { get; set; }
    public DateOnly EventDate { get; set; }
    public TimeOnly EventTime { get; set; }
    public TicketTypesEnum TicketType { get; set; }
    public TicketStatusesEnum TicketStatus { get; set; }
    public bool? NoRefund { get; set; }

    public TicketItemModel()
    {
        
    }

    public TicketItemModel(Ticket ticket)
    {
        TicketId = ticket.TicketId;
        EventTitle = ticket.Event.EventTitle;
        EventDate = ticket.Event.EventDate;
        EventTime = ticket.Event.EventTime;
        TicketType = ticket.TicketType;
        TicketStatus = ticket.TicketStatus;
        NoRefund = ticket.NoRefund;
    }
}