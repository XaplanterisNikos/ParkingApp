using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Authorization;
using ParkingApp.Api.Services.Console;
using ParkingApp.Shared.Console;
using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// Read endpoints of the employee console (the cash desk's view of its branch).
/// Every endpoint requires an employee with a selected work branch.
/// </summary>
[ApiController]
[Route("api/console")]
[Authorize(Policy = AuthPolicies.ActiveBranch)]
public class ConsoleController : ControllerBase
{
	// Console read logic, always scoped to the caller's active branch
	private readonly IConsoleService _consoleService;

	/// <summary>
	/// Creates the controller.
	/// </summary>
	/// <param name="consoleService">The console read service.</param>
	public ConsoleController(IConsoleService consoleService)
	{
		_consoleService = consoleService;
	}

	/// <summary>
	/// Returns total and occupied spots per size category for the active branch.
	/// </summary>
	[HttpGet("occupancy")]
	public async Task<ActionResult<ApiResponse<List<SpotSizeOccupancyDto>>>> GetOccupancy()
	{
		var occupancy = await _consoleService.GetOccupancyAsync();
		return Ok(ApiResponse<List<SpotSizeOccupancyDto>>.Ok(occupancy));
	}

	/// <summary>
	/// Returns the spot map of the active branch (floors, spots, live occupancy) and,
	/// when a vehicle size is given, a suggested free spot of exactly that size.
	/// </summary>
	/// <param name="size">
	/// Optional vehicle size category, by name or value (<c>?size=Car</c> or <c>?size=2</c>).
	/// Omit it to get the map without a suggestion.
	/// </param>
	[HttpGet("spot-map")]
	public async Task<ActionResult<ApiResponse<SpotMapDto>>> GetSpotMap([FromQuery] SpotSize? size)
	{
		// Thin action: binding in, envelope out — all logic lives in the service
		var spotMap = await _consoleService.GetSpotMapAsync(size);
		return Ok(ApiResponse<SpotMapDto>.Ok(spotMap));
	}

	/// <summary>
	/// Returns the overnight packages the customer can ask for at entry
	/// (from the active branch's current price list).
	/// </summary>
	[HttpGet("overnight-packages")]
	public async Task<ActionResult<ApiResponse<List<OvernightPackageDto>>>> GetOvernightPackages()
	{
		var packages = await _consoleService.GetOvernightPackagesAsync();
		return Ok(ApiResponse<List<OvernightPackageDto>>.Ok(packages));
	}
}