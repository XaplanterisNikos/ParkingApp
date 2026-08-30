using ParkingApp.Shared.Auth;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Auth;

/// <summary>
/// Talks to the API's authentication endpoints over HTTP.
/// Pure communication: no token storage, no state — just request/response.
/// </summary>
public interface IAuthConsumer
{
	/// <summary>
	/// Calls the login endpoint with the given credentials.
	/// </summary>
	/// <param name="request">The username and password.</param>
	/// <returns>The API's standard response envelope containing the token on success.</returns>
	Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request);
}
