using Epic5Task.Application.Interfaces;
using Epic5Task.Application.Tickets.Models;
using Epic5Task.Domain.AggregateRoot;
using Epic5Task.Domain.Enums;
using MediatR;

namespace Epic5Task.Application.Tickets.Queries;

public class GetTicketsQuery : IRequest<List<TicketItemModel>>
{
}

public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, List<TicketItemModel>>
{
    private readonly ITicketProvider _ticketProvider;
    private readonly IUserProvider _userProvider;
    private readonly IUserContextProvider _userContextProvider;

    public GetTicketsQueryHandler(ITicketProvider ticketProvider, IUserProvider userProvider,
        IUserContextProvider userContextProvider)
    {
        _ticketProvider = ticketProvider;
        _userProvider = userProvider;
        _userContextProvider = userContextProvider;
    }

    public Task<List<TicketItemModel>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = _userProvider.GetUserId(_userContextProvider.UserPhoneNumber);
        var tickets = _ticketProvider.GetUserTickets(userId);

        UpdateTicketsStatus(tickets, userId);

        var result = tickets.Select(x => new TicketItemModel(x)).ToList();

        return Task.FromResult(result);
    }

    private void UpdateTicketsStatus(List<Ticket> tickets, int userId)
    {
        foreach (var ticket in tickets)
        {
            switch (ticket.TicketStatus)
            {
                case TicketStatusesEnum.Pending:
                    if (ticket.CreatedAt.AddMinutes(15) < DateTime.UtcNow)
                    {
                        _ticketProvider.UpdateTicketStatus(ticket, TicketStatusesEnum.Expired, userId);
                    }
                    break;
                case TicketStatusesEnum.Confirmed:
                    var eventTime = ticket.Event.EventDate.ToDateTime(ticket.Event.EventTime);
                    if (eventTime < DateTime.UtcNow)
                    {
                        _ticketProvider.UpdateTicketStatus(ticket, TicketStatusesEnum.Expired, userId);
                    }

                    break;
                case TicketStatusesEnum.Cancelled:
                    break;
                case TicketStatusesEnum.Expired:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}