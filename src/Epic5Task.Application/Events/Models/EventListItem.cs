using Epic5Task.Domain.Enums;

namespace Epic5Task.Application.Events.Queries;

public class EventListItem : AbstractEventItem
{
    public List<TicketTypesEnum> TicketTypes { get; set; }
}