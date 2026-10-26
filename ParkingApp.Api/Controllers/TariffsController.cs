using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Data;
using ParkingApp.Api.Services.Pricing;
using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// The price list of a branch, for the owner. Saving always creates a new version.
/// </summary>
[ApiController]
[Route("api/branches/{branchId:guid}/tariff")]
[Authorize(Roles = DbSeeder.OwnerRole)]
public class TariffsController : ControllerBase
{
	// Price list logic; tenant scoping is automatic
	private readonly ITariffService _tariffService;

	/// <summary>
	/// Creates the controller.
	/// </summary>
	/// <param name="tariffService">The price list service.</param>
	public TariffsController(ITariffService tariffService)
	{
		_tariffService = tariffService;
	}

	/// <summary>
	/// Returns the current price list of the branch, or suggested defaults (no version id)
	/// when the branch has none yet.
	/// </summary>
	/// <param name="branchId">The branch.</param>
	[HttpGet]
	public async Task<ActionResult<ApiResponse<TariffDto>>> GetCurrent([FromRoute] Guid branchId)
	{
		var tariff = await _tariffService.GetCurrentAsync(branchId);

		if (tariff is null)
		{
			return NotFound(ApiResponse<TariffDto>.Fail("Το υποκατάστημα δεν βρέθηκε."));
		}

		return Ok(ApiResponse<TariffDto>.Ok(tariff));
	}

	/// <summary>
	/// Saves the price list as a new version, which becomes the current one.
	/// </summary>
	/// <param name="branchId">The branch.</param>
	/// <param name="request">The complete price list.</param>
	/// <returns>200 with the new version; 400 with the broken rules; 404 when the branch is missing.</returns>
	[HttpPost]
	public async Task<ActionResult<ApiResponse<TariffDto>>> Save(
		[FromRoute] Guid branchId,
		[FromBody] SaveTariffRequest request)
	{
		var result = await _tariffService.SaveAsync(branchId, request);

		return result.Outcome switch
		{
			TariffSaveOutcome.Saved => Ok(ApiResponse<TariffDto>.Ok(result.Tariff!, "Ο τιμοκατάλογος αποθηκεύτηκε.")),
			TariffSaveOutcome.BranchNotFound => NotFound(ApiResponse<TariffDto>.Fail("Το υποκατάστημα δεν βρέθηκε.")),
			_ => BadRequest(ApiResponse<TariffDto>.Fail("Ο τιμοκατάλογος δεν αποθηκεύτηκε.", result.Errors))
		};
	}
}
