using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Authorization;
using ParkingApp.Api.Extensions;
using ParkingApp.Api.Services.Tickets;
using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Tickets;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// Write endpoints of the employee console: vehicle entry (and, in a later slice, exit).
/// Every endpoint requires an employee with a selected work branch.
/// </summary>
[ApiController]
[Route("api/tickets")]
[Authorize(Policy = AuthPolicies.ActiveBranch)]
public class TicketsController : ControllerBase
{
	// Ticket logic, always scoped to the caller's active branch
	private readonly ITicketService _ticketService;

	/// <summary>
	/// Creates the controller.
	/// </summary>
	/// <param name="ticketService">The ticket service.</param>
	public TicketsController(ITicketService ticketService)
	{
		_ticketService = ticketService;
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
}
