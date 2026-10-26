using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Pricing;

/// <summary>Calls the owner's price list endpoints of a branch.</summary>
public interface ITariffsConsumer
{
	/// <summary>Gets the current price list of the branch (or suggested defaults when none is saved).</summary>
	/// <param name="branchId">The branch.</param>
	Task<ApiResponse<TariffDto>?> GetCurrentAsync(Guid branchId);

	/// <summary>Saves the price list as a new version.</summary>
	/// <param name="branchId">The branch.</param>
	/// <param name="request">The complete price list.</param>
	/// <returns>The server's response; on 400 it lists the broken rules in <c>Errors</c>.</returns>
	Task<ApiResponse<TariffDto>?> SaveAsync(Guid branchId, SaveTariffRequest request);
}
