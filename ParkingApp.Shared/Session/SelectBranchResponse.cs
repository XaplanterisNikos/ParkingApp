namespace ParkingApp.Shared.Session;

/// <summary>
/// Returned after a successful branch selection: a fresh JWT that additionally
/// carries the chosen branch, so subsequent requests are scoped to it.
/// </summary>
public class SelectBranchResponse
{
	/// <summary>The re-issued access token, now including the active branch claim.</summary>
	public string Token { get; set; } = string.Empty;
}