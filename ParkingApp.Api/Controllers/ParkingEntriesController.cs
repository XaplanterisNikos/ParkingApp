using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Services.Parking;
using ParkingApp.Shared.Parking;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Controllers
{
	[ApiController]
	[Route("api/parking-entries")]
	public class ParkingEntriesController : ControllerBase
	{
		private readonly IParkingEntryService _parkingEntryService;

		public ParkingEntriesController(IParkingEntryService parkingEntryService)
		{
			_parkingEntryService = parkingEntryService;
		}

		[HttpGet]
		public async Task<ActionResult<ApiResponse<List<ParkingEntryDto>>>> GetAll([FromQuery] bool includeDeleted = false)
		{
			var response = await _parkingEntryService.GetAllAsync(includeDeleted);
			return Ok(response);
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<ApiResponse<ParkingEntryDto>>> GetParkingEntryById([FromRoute]int id, [FromQuery] bool includeDeleted = false)
		{
			var response = await _parkingEntryService.GetByIdAsync(id, includeDeleted);
			if (!response.Success) return NotFound(response);
			return Ok(response);
		}

		[HttpPost]
		public async Task<ActionResult<ApiResponse<ParkingEntryDto>>> CreateParkingEntry(
			[FromBody] CreateParkingEntryRequest request)
		{
			var response = await _parkingEntryService.CreateAsync(request);

			if(!response.Success) return BadRequest(response);

			return CreatedAtAction(
				nameof(GetParkingEntryById),
				new { id = response.Value!.Id },
				response);
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<ApiResponse<ParkingEntryDto>>> UpdateParkingEntry(
	   [FromRoute] int id,
	   [FromBody] UpdateParkingEntryRequest request)
		{
			var response = await _parkingEntryService.UpdateAsync(id, request);

			if (!response.Success)
			{
				return NotFound(response);
			}

			return Ok(response);
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult<ApiResponse<bool>>> DeleteParkingEntry([FromRoute] int id)
		{
			var response = await _parkingEntryService.DeleteAsync(id);

			if (!response.Success)
			{
				return NotFound(response);
			}

			return Ok(response);
		}


	}
}
