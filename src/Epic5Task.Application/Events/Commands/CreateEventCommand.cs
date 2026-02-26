using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Events.Validators;
using Epic5Task.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Epic5Task.Application.Events.Commands;

public class CreateEventCommand: IRequest<int>
{
    public CreateEventCommand(EventRequestModel model)
    {
        Model = model;
    }

    public EventRequestModel Model { get; set; } = null!;
}

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Model)
            .NotNull()
            .SetValidator(new EventRequestModelValidator());
    }
}


public class CreateEventCommandHandler: IRequestHandler<CreateEventCommand, int>
{
    private readonly IUserContextProvider _userContextProvider;
    private readonly IEventProvider _eventProvider;

    public CreateEventCommandHandler(IUserContextProvider userContextProvider, IEventProvider eventProvider)
    {
        _userContextProvider = userContextProvider;
        _eventProvider = eventProvider;
    }

    public Task<int> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var eventId = _eventProvider.CreateEvent(request.Model, _userContextProvider.UserPhoneNumber);
        
        return Task.FromResult(eventId); 
    }
}