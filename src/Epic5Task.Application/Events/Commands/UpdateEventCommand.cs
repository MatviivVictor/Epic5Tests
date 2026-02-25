using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Events.Validators;
using Epic5Task.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Epic5Task.Application.Events.Commands;

public class UpdateEventCommand: IRequest<int>
{
    public UpdateEventCommand(int eventId, EventRequestModel model)
    {
        EventId = eventId;
        Model = model;
    }

    public int EventId { get; set; }
    public EventRequestModel Model { get; set; } = null!;
}

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(x => x.Model)
            .NotNull()
            .SetValidator(new EventRequestModelValidator());
    }
}

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, int>
{
    private readonly IUserContextProvider _userContextProvider;
    private readonly IEventProvider _eventProvider;
    private readonly IUserProvider _userProvider;

    public UpdateEventCommandHandler(IUserContextProvider userContextProvider, IEventProvider eventProvider, IUserProvider userProvider)
    {
        _userContextProvider = userContextProvider;
        _eventProvider = eventProvider;
        _userProvider = userProvider;
    }

    public Task<int> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var userId = _userProvider.GetUserId(_userContextProvider.UserPhoneNumber);
        _eventProvider.UpdateEvent(request.EventId, request.Model, userId);
        return Task.FromResult(request.EventId);
    }
}