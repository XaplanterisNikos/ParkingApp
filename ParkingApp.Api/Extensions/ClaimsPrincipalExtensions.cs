using System.Security.Claims;

namespace ParkingApp.Api.Extensions;

/// <summary>
/// Extension helpers for reading application-specific claims off the current user.
/// </summary>
public static class ClaimsPrincipalExtensions
{
	/// <summary>
	/// The claim type that carries the tenant (comapny) id in the JWT.
	/// </summary>
	public const string CompanyIdClaim = "companyId";

	/// <summary>
	/// Reads the current user's company id from their JWT claims.
	/// </summary>
	/// <param name="user">The current authenticated principal.</param>
	/// <returns>The company id the user belongs to.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the claim is missing or malformed — this should never happen for a
	/// properly issued token, so it signals a bug or tampering rather than user error.
	/// </exception>
	public static Guid GetComapnyId(this ClaimsPrincipal user)
	{
		var value = user.FindFirst(CompanyIdClaim)?.Value;

		if (Guid.TryParse(value, out var companyId)) return companyId;

		throw new InvalidOperationException(
			"The current user has no valid 'companyId' claim.");
	
	}


}
