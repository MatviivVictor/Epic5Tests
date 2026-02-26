using Epic5Task.Application.Exceptions;
using Epic5Task.Application.Interfaces;
using Epic5Task.Application.Tickets.Models;
using Epic5Task.Domain.Enums;
using FluentValidation;
using MediatR;

namespace Epic5Task.Application.Tickets.Commands;

public class ConfirmTicketCommand : IRequest<TicketItemModel>
{
    public ConfirmTicketCommand(int ticketId)
    {
        TicketId = ticketId;
    }

    public int TicketId { get; set; }
}

public class ConfirmTicketCommandValidator : AbstractValidator<ConfirmTicketCommand>
{
    public ConfirmTicketCommandValidator()
    {
        RuleFor(x => x.TicketId).GreaterThan(0);
    }
}

public class ConfirmTicketCommandHandler : IRequestHandler<ConfirmTicketCommand, TicketItemModel>
{
    private readonly ITicketProvider _ticketProvider;
    private readonly IUserProvider _userProvider;
    private readonly IUserContextProvider _userContextProvider;
    private readonly IEventProvider _eventProvider;

    public ConfirmTicketCommandHandler(ITicketProvider ticketProvider, IUserProvider userProvider,
        IUserContextProvider userContextProvider, IEventProvider eventProvider)
    {
        _ticketProvider = ticketProvider;
        _userProvider = userProvider;
        _userContextProvider = userContextProvider;
        _eventProvider = eventProvider;
    }

    public Task<TicketItemModel> Handle(ConfirmTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = _ticketProvider.GetTicket(request.TicketId);
        if (ticket.TicketStatus != TicketStatusesEnum.Pending)
        {
            throw new EntityConflictException("Ticket is not pending");
        }

        var userId = _userProvider.GetUserId(_userContextProvider.UserPhoneNumber);
        if (ticket.UserId != userId)
        {
            throw new AuthZException("User is not authorized to confirm this ticket");
        }

        var @event = _eventProvider.GetEvent(ticket.EventId);
        var capacity = @event.EventCapacities.FirstOrDefault(x => x.TicketType == ticket.TicketType);
        if (capacity == null)
        {
            throw new InvalidOperationException(
                $"Event does not have capacity for ticket type {ticket.TicketType}");
        }

        if (capacity.TicketCapacityLimit - capacity.TicketSold < 1)
        {
            throw new InvalidOperationException(
                $"Event does not have enough capacity for ticket type {ticket.TicketType}");
        }

        if (ticket.CreatedAt.AddMinutes(15) < DateTime.UtcNow)
        {
            _ticketProvider.UpdateTicketStatus(ticket, TicketStatusesEnum.Expired, userId);
            throw new EntityConflictException("Ticket is expired");
        }

        _ticketProvider.UpdateTicketStatus(ticket, TicketStatusesEnum.Confirmed, userId);
        return Task.FromResult(new TicketItemModel(ticket));
    }
}