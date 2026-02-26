using Epic5Task.Application.Events.Commands;
using Epic5Task.Application.Events.Models;
using Epic5Task.Application.Events.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Epic5Task.Api.Contollers;

[Route("api/v1/[controller]")]
[ApiController]
public class EventsController: ControllerBase
{
    private readonly IMediator _mediator;

    public EventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of events.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response with a list of events.</returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<EventListItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListEvents(CancellationToken cancellationToken)
    {
        var query = new GetListEventsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new event.
    /// </summary>
    /// <param name="model">The model containing the details of the event to be created.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response with the created event details.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EventListItem), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateEvent([FromBody] EventRequestModel model,
        CancellationToken cancellationToken)
    {
        var command = new CreateEventCommand(model);
        var eventId = await _mediator.Send(command, cancellationToken);
        var result = await _mediator.Send(new GetEventByIdQuery(eventId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing event with the specified details.
    /// </summary>
    /// <param name="id">The unique identifier of the event to update.</param>
    /// <param name="model">An object containing the updated event details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated event details.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EventListItem), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEvent([FromRoute] int id, [FromBody] EventRequestModel model,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEventCommand(id, model);
        var eventId = await _mediator.Send(command, cancellationToken);
        var result = await _mediator.Send(new GetEventByIdQuery(eventId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the details of a specific event by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response with the event details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventListItem), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventById([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = new GetEventByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves statistical information about a specific event.
    /// </summary>
    /// <param name="id">The unique identifier of the event.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response with the event statistics.</returns>
    [HttpGet("{id}/statistics")]
    [ProducesResponseType(typeof(EventStatisticsItemModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEventStatistics([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = new GetEventStatisticsQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates tickets for a specified event.
    /// </summary>
    /// <param name="id">The identifier of the event for which tickets are to be created.</param>
    /// <param name="model">The data model containing details about the tickets to be created.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the list of created ticket IDs.</returns>
    [HttpPost("{id}/tickets")]
    [ProducesResponseType(typeof(List<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateTickets([FromRoute] int id, [FromBody] CreateTicketsRequestModel model,
        CancellationToken cancellationToken)
    {
        var command = new CreateTicketsCommand(id, model);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}