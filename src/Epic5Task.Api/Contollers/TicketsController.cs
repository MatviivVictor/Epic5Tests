using Epic5Task.Application.Tickets.Commands;
using Epic5Task.Application.Tickets.Models;
using Epic5Task.Application.Tickets.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Epic5Task.Api.Contollers;

[Route("api/v1/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves a list of tickets.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the list of tickets.</returns>
    [HttpGet("list")]
    [ProducesResponseType(typeof(List<TicketItemModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTickets(CancellationToken cancellationToken)
    {
        var query = new GetTicketsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Confirms a specific ticket by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ticket to be confirmed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult indicating the result of the confirmation.</returns>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(TicketItemModel), StatusCodes.Status200OK)]
    public async Task<IActionResult> ConfirmTicket([FromRoute] int id, CancellationToken cancellationToken)
    {
        var command = new ConfirmTicketCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Cancels a specific ticket by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ticket to be canceled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult indicating the result of the cancellation.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelTicket([FromRoute] int id, CancellationToken cancellationToken)
    {
        var command = new CancelTicketCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Retrieves the history of a specific ticket by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the ticket whose history is being retrieved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IActionResult with the ticket's history.</returns>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(List<TicketHistoryItemModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTicketHistory([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = new GetTicketHistoryQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}