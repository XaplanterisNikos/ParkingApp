namespace ParkingApp.Shared.Auth;

/// <summary>
/// Credentials sent by the client to obtain an access token.
/// </summary>
public class LoginRequest
{
	/// <summary>The user's login name (email for owners, username for employees).</summary>
	public required string UserName { get; set;  }
	/// <summary>The user's plain-text password (sent over HTTPS, never stored).</summary>
	public required string Password { get; set; }
}
