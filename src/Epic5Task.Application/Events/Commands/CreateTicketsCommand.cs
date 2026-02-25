using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Epic5Task.Application.Events.Commands;

public class CreateTicketsCommand : IRequest<List<int>>
{
    public CreateTicketsCommand(int eventId, CreateTicketsRequestModel model)
    {
        EventId = eventId;
        Model = model;
    }

    public int EventId { get; set; }
    public CreateTicketsRequestModel Model { get; set; }
}

public class CreateTicketsCommandValidator : AbstractValidator<CreateTicketsCommand>
{
    public CreateTicketsCommandValidator()
    {
        RuleFor(x => x.EventId).GreaterThan(0);
        RuleFor(x => x.Model).NotNull();
        RuleForEach(x => x.Model.Tickets)
            .ChildRules(c =>
            {
                c.RuleFor(x => x.TicketType).IsInEnum();
                c.RuleFor(x => x.Quantity).GreaterThan(0);
            });
    }
}

public class CreateTicketsCommandHandler : IRequestHandler<CreateTicketsCommand, List<int>>
{
    private readonly IEventProvider _eventProvider;
    private readonly IUserContextProvider _userContextProvider;
    private readonly IUserProvider _userProvider;
    private readonly ITicketProvider _ticketProvider;

    public CreateTicketsCommandHandler(IEventProvider eventProvider, IUserContextProvider userContextProvider,
        IUserProvider userProvider, ITicketProvider ticketProvider)
    {
        _eventProvider = eventProvider;
        _userContextProvider = userContextProvider;
        _userProvider = userProvider;
        _ticketProvider = ticketProvider;
    }

    public Task<List<int>> Handle(CreateTicketsCommand request, CancellationToken cancellationToken)
    {
        var @event = _eventProvider.GetEvent(request.EventId);
        var userId = _userProvider.GetUserId(_userContextProvider.UserPhoneNumber);
        var result = new List<int>();

        foreach (var bookingItem in request.Model.Tickets)
        {
            var capacity = @event.EventCapacities.FirstOrDefault(x => x.TicketType == bookingItem.TicketType);
            if (capacity == null)
            {
                throw new InvalidOperationException(
                    $"Event does not have capacity for ticket type {bookingItem.TicketType}");
            }

            if (capacity.TicketCapacityLimit - capacity.TicketSold < bookingItem.Quantity)
            {
                throw new InvalidOperationException(
                    $"Event does not have enough capacity for ticket type {bookingItem.TicketType}");
            }

            for (int i = 0; i < bookingItem.Quantity; i++)
            {
                var ticketId = _ticketProvider.CreateTicket(request.EventId, userId, bookingItem.TicketType);
                result.Add(ticketId);
            }
        }
        
        return Task.FromResult(result);
    }
}