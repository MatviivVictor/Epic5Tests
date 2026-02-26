using Epic5Task.Application.Exceptions;
using Epic5Task.Application.Interfaces;
using Epic5Task.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Epic5Task.Application.Tickets.Commands;

public class CancelTicketCommand : IRequest<Unit>
{
    public CancelTicketCommand(int ticketId)
    {
        TicketId = ticketId;
    }

    public int TicketId { get; set; }
}

public class CancelTicketCommandValidator : AbstractValidator<CancelTicketCommand>
{
    public CancelTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
    }
}

public class CancelTicketCommandHandler : IRequestHandler<CancelTicketCommand, Unit>
{
    private readonly ITicketProvider _ticketProvider;
    private readonly IUserProvider _userProvider;
    private readonly IUserContextProvider _userContextProvider;

    public CancelTicketCommandHandler(ITicketProvider ticketProvider, IUserProvider userProvider,
        IUserContextProvider userContextProvider)
    {
        _ticketProvider = ticketProvider;
        _userProvider = userProvider;
        _userContextProvider = userContextProvider;
    }

    public Task<Unit> Handle(CancelTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = _ticketProvider.GetTicket(request.TicketId);
        if ((new[] { TicketStatusesEnum.Cancelled, TicketStatusesEnum.Expired }).Contains(ticket.TicketStatus))
        {
            throw new EntityConflictException("Ticket is not yet available for cancellation"); 
        }

        var userId = _userProvider.GetUserId(_userContextProvider.UserPhoneNumber);
        if (ticket.UserId != userId)
        {
            throw new AuthZException("User is not authorized to confirm this ticket");
        }

        var eventDate = ticket.Event.EventDate.ToDateTime(ticket.Event.EventTime);
        var now = DateTime.UtcNow;

        if (eventDate > now)
        {
            throw new EntityConflictException("Ticket is not yet available for cancellation"); 
        }

        if (ticket.TicketStatus == TicketStatusesEnum.Confirmed)
        {
            ticket.NoRefund = now - eventDate > TimeSpan.FromDays(1);
        }

        _ticketProvider.UpdateTicketStatus(ticket, TicketStatusesEnum.Cancelled, userId);
        return Task.FromResult(Unit.Value);
    }
}