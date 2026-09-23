using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Authorization;
using ParkingApp.Api.Services.Console;
using ParkingApp.Shared.Console;
using ParkingApp.Shared.Responses;

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
}