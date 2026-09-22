namespace ParkingApp.Shared.Auth;

/// <summary>
/// Names of the custom claims the application puts in its JWT.
/// Lives in Shared because the Api writes these claims and the Client reads them,
/// so both sides must agree on the exact same names.
/// </summary>
public static class AppClaimTypes
{
	/// <summary>The tenant (company) id the user belongs to.</summary>
	public const string CompanyId = "companyId";

	/// <summary>The branch the employee selected for this work session.</summary>
	public const string ActiveBranchId = "activeBranchId";

	/// <summary>Display name of the active branch, shown in the nav bar.</summary>
	public const string ActiveBranchName = "activeBranchName";
}
