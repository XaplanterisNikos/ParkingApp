using ParkingApp.Shared.Auth;
using System.Security.Claims;

namespace ParkingApp.Api.Extensions;

/// <summary>
/// Extension helpers for reading application-specific claims off the current user.
/// </summary>
public static class ClaimsPrincipalExtensions
{
	/// <summary>
	/// Reads the current user's company id from their JWT claims.
	/// </summary>
	/// <param name="user">The current authenticated principal.</param>
	/// <returns>The company id the user belongs to.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the claim is missing or malformed — this should never happen for a
	/// properly issued token, so it signals a bug or tampering rather than user error.
	/// </exception>
	public static Guid GetCompanyId(this ClaimsPrincipal user)
	{
		// Claim name comes from the shared constants (single source of truth)
		var value = user.FindFirst(AppClaimTypes.CompanyId)?.Value;

		if (Guid.TryParse(value, out var companyId)) return companyId;

		throw new InvalidOperationException(
			"The current user has no valid 'companyId' claim.");
	
	}

	/// <summary>
	/// Reads the current user's Identity id from their JWT claims.
	/// </summary>
	/// <param name="user">The current authenticated principal.</param>
	/// <returns>The user's Identity id.</returns>
	/// <exception cref="InvalidOperationException">
	/// Thrown if the id claim is missing — signals a malformed or tampered token.
	/// </exception>
	public static string GetUserId(this ClaimsPrincipal user)
	{
		var value = user.FindFirstValue(ClaimTypes.NameIdentifier);

		if (!string.IsNullOrEmpty(value)) return value;

		throw new InvalidOperationException(
			"The current user has no valid NameIdentifier claim.");
	}

	/// <summary>
	/// Reads the branch the employee selected for this work session.
	/// </summary>
	/// <param name="user">The current authenticated principal.</param>
	/// <returns>
	/// The active branch id, or <c>null</c> if no branch has been selected yet.
	/// </returns>
	/// <remarks>
	/// Unlike <see cref="GetCompanyId"/>, a missing claim is NOT an error here:
	/// the first login token is intentionally branch-free, and the claim only
	/// appears after the employee selects a branch (re-issued token).
	/// </remarks>
	public static Guid? GetActiveBranchId(this ClaimsPrincipal user)
	{
		var value = user.FindFirst(AppClaimTypes.ActiveBranchId)?.Value;

		// Missing or malformed claim → no active branch (null), never an exception
		return Guid.TryParse(value, out var branchId) ? branchId : null;
	}
}
