using Epic5Task.Application.Interfaces;
using Epic5Task.Domain.AggregateRoot;
using Epic5Task.Domain.Entities;
using Epic5Task.Domain.Enums;
using Epic5Task.Infrastructure.DataContext;

namespace Epic5Task.Infrastructure.DataProviders;

public class TicketProvider : ITicketProvider
{
    private readonly IEventProvider _eventProvider;

    public TicketProvider(IEventProvider eventProvider)
    {
        _eventProvider = eventProvider;
    }

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

        return ticketId;
    }

    public List<Ticket> GetUserTickets(int userId)
    {
        return GetRawTickets()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }

    private static IEnumerable<Ticket> GetRawTickets()
    {
        return Data.Tickets
            .GroupJoin(Data.Events,
                x => x.EventId,
                x => x.EventId,
                (x, y) => new
                {
                    Ticket = x,
                    Event = y.Single()
                })
            .Select(x => new Ticket(x.Ticket, x.Event)).AsEnumerable();
    }

    public void UpdateTicketStatus(Ticket ticket, TicketStatusesEnum status, int userId)
    {
        var currentStatus = ticket.TicketStatus;
        var newStatus = status;

        var shouldUpdateCapacity = (currentStatus, newStatus) switch
        {
            (TicketStatusesEnum.Pending, TicketStatusesEnum.Confirmed) => true,
            (TicketStatusesEnum.Confirmed, TicketStatusesEnum.Cancelled) => true,
            _ => false
        };

        ticket.TicketStatus = status;
        if (shouldUpdateCapacity)
        {
            _eventProvider.UpdateEventCapacity(ticket.EventId, ticket.TicketType, status);
        }

        Data.TicketStatusHistory.Add(new TicketStatusHistoryEntity
        {
            TicketId = ticket.TicketId,
            TicketStatus = status,
            StatusChangedAt = DateTime.Now,
            StatusChangedBy = userId
        });
    }

    public Ticket GetTicket(int ticketId)
    {
        return GetRawTickets().FirstOrDefault(x => x.TicketId == ticketId) ?? throw new Exception("Ticket not found");
    }

    public TicketWithHistory GetTicketHistory(int ticketId)
    {
        return Data.Tickets.Where(x => x.TicketId == ticketId)
                   .GroupJoin(Data.TicketStatusHistory,
                       x => x.TicketId,
                       x => x.TicketId,
                       (x, y) => new
                       {
                           Ticket = x,
                           StatusHistory = y.OrderByDescending(h => h.StatusChangedAt).ToList()
                       })
                   .Select(x => new TicketWithHistory(x.Ticket, x.StatusHistory)).SingleOrDefault() ??
               throw new Exception("Ticket not found");
    }
}