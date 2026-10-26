using ParkingApp.Shared.Tickets;

namespace ParkingApp.Api.Services.Tickets;

/// <summary>
/// The exit side of the console: active tickets, what an exit would cost, and the exit itself.
/// Always scoped to the active branch from the caller's token.
/// </summary>
public interface ITicketExitService
{
	/// <summary>Returns the active tickets of the branch, newest first (for the plate/spot search).</summary>
	Task<List<TicketDto>> GetActiveAsync();

	/// <summary>
	/// Calculates every allowed way to charge an active ticket right now: the entry version and
	/// the current version of the price list (when they differ), and for an exceeded overnight
	/// package both "price list" and "renew package".
	/// </summary>
	/// <param name="ticketId">The ticket.</param>
	Task<ExitResult<ExitQuoteDto>> GetQuoteAsync(Guid ticketId);

	/// <summary>
	/// Closes an active ticket with the chosen way to charge. The amount is calculated again at
	/// the moment of exit; the update only succeeds if the ticket is still active.
	/// </summary>
	/// <param name="ticketId">The ticket.</param>
	/// <param name="request">The chosen price list version and overstay rule.</param>
	/// <param name="employeeId">The employee who registers the exit (from the token).</param>
	Task<ExitResult<TicketDto>> ExitAsync(Guid ticketId, ExitTicketRequest request, string employeeId);
}
