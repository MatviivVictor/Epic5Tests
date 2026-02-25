using Epic5Task.Domain.Enums;

namespace Epic5Task.Application.Interfaces;

public interface ITicketProvider
{
    int CreateTicket(int eventId, int userId, TicketTypesEnum ticketType);
}