using ParkingApp.Shared.Auth;

namespace ParkingApp.Client.Services.Auth;

/// <summary>
/// Coordinates the client-side login/logout flow: calls the API, stores the
/// token, and refreshes the authentication state so the UI updates.
/// </summary>
public interface IAuthService
{
	/// <summary>
	/// Attempts to log in with the given credentials.
	/// </summary>
	/// <param name="request">The username and password.</param>
	/// <returns>
	/// A tuple: <c>Success</c> indicates whether login succeeded, and
	/// <c>Error</c> carries a message to show the user when it did not.
	/// </returns>
	Task<(bool Success, string? Error)> LoginAsync(LoginRequest request);

	/// <summary>Logs the current user out and clears their token.</summary>
	Task LogoutAsync();
}
