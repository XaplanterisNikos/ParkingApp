namespace ParkingApp.Shared.Auth;

/// <summary>
/// The result of a successful login: the signed JWT plus a little context
/// the client can display without decoding the token itself.
/// </summary>
public class LoginResponse
{
	/// <summary>The signed JWT access token.</summary>
	public required string Token { get; set; }
	/// <summary>The user's full display name.</summary>
	public required string FullName { get; set; }
}
