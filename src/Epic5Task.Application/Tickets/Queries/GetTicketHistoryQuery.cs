using Epic5Task.Application.Exceptions;
using Epic5Task.Application.Interfaces;
using Epic5Task.Application.Tickets.Models;
using FluentValidation;
using MediatR;

namespace Epic5Task.Application.Tickets.Queries;

public class GetTicketHistoryQuery : IRequest<List<TicketHistoryItemModel>>
{
    public GetTicketHistoryQuery(int ticketId)
    {
        TicketId = ticketId;
    }

    public int TicketId { get; set; }
}

public class GetTicketHistoryQueryValidator : AbstractValidator<GetTicketHistoryQuery>
{
    public GetTicketHistoryQueryValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
    }
}

public class GetTicketHistoryQueryHandler : IRequestHandler<GetTicketHistoryQuery, List<TicketHistoryItemModel>>
{
    private readonly ITicketProvider _ticketProvider;
    private readonly IUserProvider _userProvider;
    private readonly IUserContextProvider _userContextProvider;

    public GetTicketHistoryQueryHandler(ITicketProvider ticketProvider, IUserProvider userProvider,
        IUserContextProvider userContextProvider)
    {
        _ticketProvider = ticketProvider;
        _userProvider = userProvider;
        _userContextProvider = userContextProvider;
    }

    public Task<List<TicketHistoryItemModel>> Handle(GetTicketHistoryQuery request, CancellationToken cancellationToken)
    {
        var ticket = _ticketProvider.GetTicketHistory(request.TicketId);
        var userId = _userProvider.GetUserId(_userContextProvider.UserPhoneNumber);
        if (ticket.UserId != userId)
        {
            throw new AuthZException("User is not authorized to confirm this ticket");
        }

        var result = ticket.TicketStatusHistory.Select(x => new TicketHistoryItemModel()
        {
            TicketStatus = x.TicketStatus, StatusChangedAt = x.StatusChangedAt
        }).ToList();
        
        return Task.FromResult(result);
    }
}