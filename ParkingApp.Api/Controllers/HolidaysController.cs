using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Data;
using ParkingApp.Api.Services.Pricing;
using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// The company's holiday calendar, for the owner. Every branch of the company uses it.
/// </summary>
[ApiController]
[Route("api/holidays")]
[Authorize(Roles = DbSeeder.OwnerRole)]
public class HolidaysController : ControllerBase
{
	// Holiday calendar logic; tenant scoping is automatic
	private readonly IHolidayService _holidayService;

	/// <summary>
	/// Creates the controller.
	/// </summary>
	/// <param name="holidayService">The holiday calendar service.</param>
	public HolidaysController(IHolidayService holidayService)
	{
		_holidayService = holidayService;
	}

	/// <summary>Returns the Sunday setting and the marked dates.</summary>
	[HttpGet]
	public async Task<ActionResult<ApiResponse<HolidayCalendarDto>>> GetCalendar()
	{
		var calendar = await _holidayService.GetCalendarAsync();
		return Ok(ApiResponse<HolidayCalendarDto>.Ok(calendar));
	}

	/// <summary>Adds a date to the calendar.</summary>
	/// <returns>200 with the holiday; 400 without a name; 409 when the date is already there.</returns>
	[HttpPost]
	public async Task<ActionResult<ApiResponse<HolidayDto>>> Add([FromBody] CreateHolidayRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100)
		{
			return BadRequest(ApiResponse<HolidayDto>.Fail("Δώστε όνομα στην αργία (έως 100 χαρακτήρες)."));
		}

		var holiday = await _holidayService.AddAsync(request);

		if (holiday is null)
		{
			return Conflict(ApiResponse<HolidayDto>.Fail("Η ημερομηνία υπάρχει ήδη στις αργίες."));
		}

		return Ok(ApiResponse<HolidayDto>.Ok(holiday));
	}

	/// <summary>Removes a date from the calendar.</summary>
	/// <param name="id">The holiday to remove.</param>
	[HttpDelete("{id:guid}")]
	public async Task<ActionResult<ApiResponse<bool>>> Delete([FromRoute] Guid id)
	{
		if (!await _holidayService.DeleteAsync(id))
		{
			return NotFound(ApiResponse<bool>.Fail("Η αργία δεν βρέθηκε."));
		}

		return Ok(ApiResponse<bool>.Ok(true));
	}

	/// <summary>Changes whether every Sunday counts as a holiday.</summary>
	[HttpPut("settings")]
	public async Task<ActionResult<ApiResponse<bool>>> SetSettings([FromBody] HolidaySettingsRequest request)
	{
		await _holidayService.SetSundaysAreHolidaysAsync(request.SundaysAreHolidays);
		return Ok(ApiResponse<bool>.Ok(request.SundaysAreHolidays));
	}
}
