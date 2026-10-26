using ParkingApp.Shared.Pricing;

namespace ParkingApp.Api.Services.Pricing;

/// <summary>
/// The owner's holiday calendar. It belongs to the company, so every branch uses it.
/// </summary>
public interface IHolidayService
{
	/// <summary>Returns the Sunday setting and the marked dates, in date order.</summary>
	Task<HolidayCalendarDto> GetCalendarAsync();

	/// <summary>Adds a date to the calendar.</summary>
	/// <param name="request">The date and its name (the name is already checked).</param>
	/// <returns>The new holiday, or null when the date is already in the calendar.</returns>
	Task<HolidayDto?> AddAsync(CreateHolidayRequest request);

	/// <summary>Removes a date from the calendar.</summary>
	/// <param name="holidayId">The holiday to remove.</param>
	/// <returns>False when the holiday does not exist for this company.</returns>
	Task<bool> DeleteAsync(Guid holidayId);

	/// <summary>Changes whether every Sunday counts as a holiday.</summary>
	/// <param name="sundaysAreHolidays">The new setting.</param>
	Task SetSundaysAreHolidaysAsync(bool sundaysAreHolidays);
}
