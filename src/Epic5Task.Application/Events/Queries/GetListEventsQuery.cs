using Epic5Task.Application.Interfaces;
using MediatR;

namespace Epic5Task.Application.Events.Queries;

public class GetListEventsQuery : IRequest<List<EventListItem>>
{
}

public class GetListEventsQueryHandler : IRequestHandler<GetListEventsQuery, List<EventListItem>>
{
    private readonly IEventProvider _eventProvider;

    public GetListEventsQueryHandler(IEventProvider eventProvider)
    {
        _eventProvider = eventProvider;
    }

    public Task<List<EventListItem>> Handle(GetListEventsQuery request, CancellationToken cancellationToken)
    {

        var events = _eventProvider.GetEvents().Select(Mappers.Mapper.Map).ToList();
        
        return Task.FromResult(events);
    }
}