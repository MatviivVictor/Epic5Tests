using Epic5Task.Domain.Enums;

namespace Epic5Task.Application.Events.Models;

public class CreateTicketsRequestModel
{
    public BookingTicketsModel[] Tickets { get; set; } = [];
}

public class BookingTicketsModel
{
    public TicketTypesEnum TicketType { get; set; }
    public int Quantity { get; set; }
}