using ParkingApp.Shared.Tickets;

namespace ParkingApp.Api.Services.Tickets;

/// <summary>
/// Write-side of the employee console: issuing tickets (and, in a later slice, closing them).
/// Always scoped to the active branch from the caller's token.
/// </summary>
public interface ITicketService
{
	/// <summary>
	/// Registers a vehicle entry in the active branch and issues a ticket.
	/// </summary>
	/// <param name="request">The plate, its kind, the vehicle size and the confirmed spot.</param>
	/// <param name="employeeId">The employee who registers the entry (from the token).</param>
	/// <returns>The outcome; on success it carries the new ticket.</returns>
	/// <exception cref="InvalidOperationException">
	/// The caller's token has no company or no active branch. The endpoint must be
	/// protected by the <c>ActiveBranch</c> policy.
	/// </exception>
	Task<EntryResult> EnterAsync(CreateEntryRequest request, string employeeId);
}
