namespace ParkingApp.Api.Authorization;

/// <summary>
/// Names of the application's authorization policies.
/// Registered in Program.cs and referenced by [Authorize(Policy = ...)] on controllers.
/// </summary>
public static class AuthPolicies
{
	/// <summary>
	/// An employee who has selected a work branch for this session
	/// (Employee role AND an activeBranchId claim in the token).
	/// Required by every console endpoint.
	/// </summary>
	public const string ActiveBranch = "ActiveBranch";
}