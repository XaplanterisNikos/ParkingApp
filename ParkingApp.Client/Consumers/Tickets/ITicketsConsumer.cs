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
}
