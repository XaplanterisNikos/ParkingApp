using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Tickets;

namespace ParkingApp.Client.Consumers.Tickets;

/// <summary>
/// Calls the ticket endpoints of the employee console. The active branch travels inside
/// the JWT (attached by AuthTokenHandler), so no branch id is ever sent from here.
/// </summary>
public interface ITicketsConsumer
{
	/// <summary>
	/// Registers a vehicle entry in the active branch.
	/// </summary>
	/// <param name="request">The plate, its kind, the vehicle size and the confirmed spot.</param>
	/// <returns>
	/// The server's response. On 400, 404 and 409 it is a failed response with a message
	/// for the employee.
	/// </returns>
	Task<ApiResponse<TicketDto>?> EnterAsync(CreateEntryRequest request);

	/// <summary>Gets the active tickets of the branch, newest first.</summary>
	Task<ApiResponse<List<TicketDto>>?> GetActiveAsync();

	/// <summary>Gets what an exit would cost right now, with every allowed way to charge it.</summary>
	/// <param name="ticketId">The active ticket.</param>
	/// <returns>The server's response; on 404 and 409 it is a failed response with a message.</returns>
	Task<ApiResponse<ExitQuoteDto>?> GetQuoteAsync(Guid ticketId);

	/// <summary>Closes the ticket with the chosen way to charge.</summary>
	/// <param name="ticketId">The active ticket.</param>
	/// <param name="request">The chosen price list version and overstay rule.</param>
	/// <returns>The server's response; on 400, 404 and 409 it is a failed response with a message.</returns>
	Task<ApiResponse<TicketDto>?> ExitAsync(Guid ticketId, ExitTicketRequest request);
}
