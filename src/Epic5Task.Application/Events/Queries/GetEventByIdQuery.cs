using Epic5Task.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Epic5Task.Application.Events.Queries;

public class GetEventByIdQuery: IRequest<EventListItem>
{
    public GetEventByIdQuery(int eventId)
    {
        EventId = eventId;
    }

    public int EventId { get; set; }
}

public class GetEventByIdQueryValidator: AbstractValidator<GetEventByIdQuery>
{
    public GetEventByIdQueryValidator()
    {
        RuleFor(x => x.EventId).GreaterThan(0);
    }
}

public class GetEventByIdQueryHandler: IRequestHandler<GetEventByIdQuery, EventListItem>
{
    private readonly IEventProvider _eventProvider;

    public GetEventByIdQueryHandler(IEventProvider eventProvider)
    {
        _eventProvider = eventProvider;
    }

    public Task<EventListItem> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var @event = _eventProvider.GetEvent(request.EventId);

        return Task.FromResult(Mappers.Mapper.Map(@event));
    }
}