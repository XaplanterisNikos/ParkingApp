using ParkingApp.Shared.Pricing;

namespace ParkingApp.Api.Services.Pricing;

/// <summary>
/// The owner's side of price lists: read the current version of a branch and save a new one.
/// Tenant scoping is automatic; the branch must belong to the caller's company.
/// </summary>
public interface ITariffService
{
	/// <summary>
	/// Returns the current (newest) price list of a branch, or suggested defaults with no
	/// version id when the branch has none yet.
	/// </summary>
	/// <param name="branchId">The branch.</param>
	/// <returns>The price list, or null when the branch does not exist for this company.</returns>
	Task<TariffDto?> GetCurrentAsync(Guid branchId);

	/// <summary>
	/// Checks the price list and saves it as a NEW version. Older versions stay unchanged,
	/// so tickets that entered under them can still be charged with them.
	/// </summary>
	/// <param name="branchId">The branch.</param>
	/// <param name="request">The complete price list.</param>
	Task<TariffSaveResult> SaveAsync(Guid branchId, SaveTariffRequest request);
}
