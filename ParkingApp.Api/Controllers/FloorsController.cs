using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Services.Floors;
using ParkingApp.Shared.Floors;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// Endpoints for the floors within a branch. The branch is identified by the route,
/// and access is implicitly tenant-scoped (a branch that isn't the caller's simply
/// yields no data).
/// </summary>
[ApiController]
[Route("api/branches/{branchId:guid}/floors")]
[Authorize]
public class FloorsController : ControllerBase
{
	private readonly IFloorService _floorService;

	public FloorsController(IFloorService floorService)
	{
		_floorService = floorService;
	}

	/// <summary>Returns all floors of the given branch.</summary>
	[HttpGet]
	public async Task<ActionResult<ApiResponse<List<FloorDto>>>> GetByBranch(
		[FromRoute] Guid branchId)
	{
		var floors = await _floorService.GetByBranchAsync(branchId);
		return Ok(ApiResponse<List<FloorDto>>.Ok(floors));
	}

	/// <summary>Creates a new floor within the given branch.</summary>
	[HttpPost]
	public async Task<ActionResult<ApiResponse<FloorDto>>> Create(
		[FromRoute] Guid branchId,
		[FromBody] CreateFloorRequest request)
	{
		var floor = await _floorService.CreateAsync(branchId, request);

		if (floor is null)
		{
			// Service returned null → that floor type already exists in this branch.
			return Conflict(ApiResponse<FloorDto>.Fail("This floor already exists in the branch."));
		}

		return Ok(ApiResponse<FloorDto>.Ok(floor));
	}
}
