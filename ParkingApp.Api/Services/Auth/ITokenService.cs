using ParkingApp.Api.Data.Entities;

namespace ParkingApp.Api.Services.Auth;

/// <summary>
/// Creates signed JWT access tokens for authenticated users.
/// </summary>
public interface ITokenService
{
	/// <summary>
	/// Builds a signed JWT for the given user, embedding their id, roles and company id as claims.
	/// </summary>
	/// <param name="user">The authenticated user.</param>
	/// <param name="roles">The roles assigned to the user.</param>
	/// <returns>The serialized JWT string.</returns>
	string CreateToken(ApplicationUser user, IEnumerable<string> roles);
}
