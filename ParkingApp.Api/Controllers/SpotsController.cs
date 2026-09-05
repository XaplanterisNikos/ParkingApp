using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Services.Spots;
using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// Endpoints for the parking spots within a floor. The floor is identified by the route;
/// access is implicitly tenant-scoped (a floor that isn't the caller's yields no data).
/// </summary>
[ApiController]
[Route("api/floors/{floorId:guid}/spots")]
[Authorize]
public class SpotsController : ControllerBase
{
	private readonly ISpotService _spotService;

	public SpotsController(ISpotService spotService)
	{
		_spotService = spotService;
	}

	/// <summary>Returns all spots of the given floor.</summary>
	[HttpGet]
	public async Task<ActionResult<ApiResponse<List<SpotDto>>>> GetByFloor(
		[FromRoute] Guid floorId)
	{
		var spots = await _spotService.GetByFloorAsync(floorId);
		return Ok(ApiResponse<List<SpotDto>>.Ok(spots));
	}

	/// <summary>Creates a new spot within the given floor.</summary>
	[HttpPost]
	public async Task<ActionResult<ApiResponse<SpotDto>>> Create(
		[FromRoute] Guid floorId,
		[FromBody] CreateSpotRequest request)
	{
		var spot = await _spotService.CreateAsync(floorId, request);
		return Ok(ApiResponse<SpotDto>.Ok(spot));
	}
}
