using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Exceptions;
using Epic5Task.Application.Interfaces;
using FluentValidation;
using MediatR;

namespace Epic5Task.Application.Events.Queries;

public class GetEventStatisticsQuery : IRequest<EventStatisticsItemModel>
{
    public GetEventStatisticsQuery(int eventId)
    {
        EventId = eventId;
    }

    public int EventId { get; set; }
}

public class GetEventStatisticsQueryValidator : AbstractValidator<GetEventStatisticsQuery>
{
    public GetEventStatisticsQueryValidator()
    {
        RuleFor(x => x.EventId).GreaterThan(0);
    }
}

public class GetEventStatisticsQueryHandler : IRequestHandler<GetEventStatisticsQuery, EventStatisticsItemModel>
{
    private readonly IEventProvider _eventProvider;
    private readonly IUserContextProvider _userContextProvider;
    private readonly IUserProvider _userProvider;

    public GetEventStatisticsQueryHandler(IEventProvider eventProvider, IUserContextProvider userContextProvider,
        IUserProvider userProvider)
    {
        _eventProvider = eventProvider;
        _userContextProvider = userContextProvider;
        _userProvider = userProvider;
    }

    public Task<EventStatisticsItemModel> Handle(GetEventStatisticsQuery request, CancellationToken cancellationToken)
    {
        var @event = _eventProvider.GetEvent(request.EventId);
        var userId = _userProvider.GetUserId(_userContextProvider.UserPhoneNumber);

        if (userId != @event.EventOwner)
        {
            throw new AuthZException("User is not the event owner");
        }

        return Task.FromResult(new EventStatisticsItemModel
        {
            EventId = @event.EventId,
            EventTitle = @event.EventTitle,
            EventDate = @event.EventDate,
            EventTime = @event.EventTime,
            EventType = @event.EventType,
            EventCapacities = @event.EventCapacities.Select(c => new EventCapacityItem
            {
                TicketType = c.TicketType,
                TicketPrice = c.TicketPrice,
                TicketCapacityLimit = c.TicketCapacityLimit,
                TicketSold = c.TicketSold
            }).ToList()
        });
    }
}