using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Authorization;
using ParkingApp.Api.Extensions;
using ParkingApp.Api.Services.Tickets;
using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Tickets;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// Ticket endpoints of the employee console: vehicle entry, active tickets, charge preview and exit.
/// Every endpoint requires an employee with a selected work branch.
/// </summary>
[ApiController]
[Route("api/tickets")]
[Authorize(Policy = AuthPolicies.ActiveBranch)]
public class TicketsController : ControllerBase
{
	// Ticket logic, always scoped to the caller's active branch
	private readonly ITicketService _ticketService;

	// Exit logic: active tickets, charge preview and the exit itself
	private readonly ITicketExitService _exitService;

	/// <summary>
	/// Creates the controller.
	/// </summary>
	/// <param name="ticketService">The ticket service (entry).</param>
	/// <param name="exitService">The exit service.</param>
	public TicketsController(ITicketService ticketService, ITicketExitService exitService)
	{
		_ticketService = ticketService;
		_exitService = exitService;
	}

	/// <summary>
	/// Registers a vehicle entry in the active branch and issues a ticket.
	/// </summary>
	/// <param name="request">The plate, its kind, the vehicle size and the confirmed spot.</param>
	/// <returns>
	/// 200 with the ticket; 400 for a rule that is broken; 404 when the spot is not in the branch;
	/// 409 when the spot was just taken or the vehicle is already inside.
	/// </returns>
	[HttpPost("entry")]
	public async Task<ActionResult<ApiResponse<TicketDto>>> Enter([FromBody] CreateEntryRequest request)
	{
		// The employee comes from the token, never from the request body
		var result = await _ticketService.EnterAsync(request, User.GetUserId());

		// One place maps outcomes to HTTP statuses; the service knows nothing about HTTP
		return result.Outcome switch
		{
			EntryOutcome.Created => Ok(ApiResponse<TicketDto>.Ok(result.Ticket!)),
			EntryOutcome.SpotNotFound => NotFound(ApiResponse<TicketDto>.Fail(result.Message!)),
			EntryOutcome.SpotTaken or EntryOutcome.PlateAlreadyInside
				=> Conflict(ApiResponse<TicketDto>.Fail(result.Message!)),
			_ => BadRequest(ApiResponse<TicketDto>.Fail(result.Message!))
		};
	}

	/// <summary>Returns the active tickets of the branch, newest first (for the plate/spot search).</summary>
	[HttpGet("active")]
	public async Task<ActionResult<ApiResponse<List<TicketDto>>>> GetActive()
	{
		var tickets = await _exitService.GetActiveAsync();
		return Ok(ApiResponse<List<TicketDto>>.Ok(tickets));
	}

	/// <summary>
	/// Returns what an exit would cost right now, with every allowed way to charge it.
	/// </summary>
	/// <param name="id">The active ticket.</param>
	/// <returns>200 with the options; 404 when not in the branch; 409 when already closed or no price list.</returns>
	[HttpGet("{id:guid}/quote")]
	public async Task<ActionResult<ApiResponse<ExitQuoteDto>>> GetQuote([FromRoute] Guid id)
	{
		var result = await _exitService.GetQuoteAsync(id);
		return ToActionResult(result);
	}

	/// <summary>
	/// Closes the ticket with the chosen way to charge; the amount is calculated at this moment.
	/// </summary>
	/// <param name="id">The active ticket.</param>
	/// <param name="request">The chosen price list version and overstay rule.</param>
	/// <returns>200 with the closed ticket; 400 for an invalid choice; 404; 409 when already closed.</returns>
	[HttpPost("{id:guid}/exit")]
	public async Task<ActionResult<ApiResponse<TicketDto>>> Exit([FromRoute] Guid id, [FromBody] ExitTicketRequest request)
	{
		// The employee comes from the token, never from the request body
		var result = await _exitService.ExitAsync(id, request, User.GetUserId());
		return ToActionResult(result);
	}

	/// <summary>One place that maps exit outcomes to HTTP statuses.</summary>
	private ActionResult<ApiResponse<T>> ToActionResult<T>(ExitResult<T> result) => result.Outcome switch
	{
		ExitOutcome.Success => Ok(ApiResponse<T>.Ok(result.Value!)),
		ExitOutcome.TicketNotFound => NotFound(ApiResponse<T>.Fail(result.Message!)),
		ExitOutcome.AlreadyExited or ExitOutcome.NoPriceList => Conflict(ApiResponse<T>.Fail(result.Message!)),
		_ => BadRequest(ApiResponse<T>.Fail(result.Message!))
	};
}
