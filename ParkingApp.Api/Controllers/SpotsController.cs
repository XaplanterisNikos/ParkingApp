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

	/// <summary>Generates a batch of spots of one size on the given floor.</summary>
	[HttpPost("generate")]
	public async Task<ActionResult<ApiResponse<List<SpotDto>>>> Generate(
		[FromRoute] Guid floorId,
		[FromBody] GenerateSpotsRequest request)
	{
		var spots = await _spotService.GenerateAsync(floorId, request);

		if (spots is null)
		{
			return NotFound(ApiResponse<List<SpotDto>>.Fail("Floor not found."));
		}

		return Ok(ApiResponse<List<SpotDto>>.Ok(spots));
	}
}
