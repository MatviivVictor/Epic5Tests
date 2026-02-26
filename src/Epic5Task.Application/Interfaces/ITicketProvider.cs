using Epic5Task.Domain.AggregateRoot;
using Epic5Task.Domain.Enums;

namespace Epic5Task.Application.Interfaces;

public interface ITicketProvider
{
    int CreateTicket(int eventId, int userId, TicketTypesEnum ticketType);
    List<Ticket> GetUserTickets(int userId);
    void UpdateTicketStatus(Ticket ticket, TicketStatusesEnum expired, int userId);
    Ticket GetTicket(int ticketId);
    TicketWithHistory GetTicketHistory(int requestTicketId);
}