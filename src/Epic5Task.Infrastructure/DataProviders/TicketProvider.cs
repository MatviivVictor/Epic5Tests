using Epic5Task.Application.Interfaces;
using Epic5Task.Domain.Entities;
using Epic5Task.Domain.Enums;
using Epic5Task.Infrastructure.DataContext;

namespace Epic5Task.Infrastructure.DataProviders;

public class TicketProvider: ITicketProvider
{
    public int CreateTicket(int eventId, int userId, TicketTypesEnum ticketType)
    {
        var now = DateTime.Now;
        var ticketStatus = TicketStatusesEnum.Pending;
        var ticketId = Data.AddTicket(new TicketEntity
        {
            EventId = eventId,
            UserId = userId,
            TicketType = ticketType,
            TicketStatus = ticketStatus,
            CreatedAt = now,
        });

        Data.AddTicketStatusHistory(new TicketStatusHistoryEntity
        {
            TicketId = ticketId,
            TicketStatus = ticketStatus,
            StatusChangedAt = now,
            StatusChangedBy = userId
        });
        
        var capacity = Data.EventCapacities.FirstOrDefault(x => x.EventId == eventId && x.TicketType == ticketType);
        capacity.TicketSold++;
        
        return ticketId;
    }
}