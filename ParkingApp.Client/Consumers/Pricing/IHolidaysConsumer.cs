using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Pricing;

/// <summary>Calls the owner's holiday calendar endpoints.</summary>
public interface IHolidaysConsumer
{
	/// <summary>Gets the Sunday setting and the marked dates.</summary>
	Task<ApiResponse<HolidayCalendarDto>?> GetCalendarAsync();

	/// <summary>Adds a date to the calendar.</summary>
	/// <param name="request">The date and its name.</param>
	Task<ApiResponse<HolidayDto>?> AddAsync(CreateHolidayRequest request);

	/// <summary>Removes a date from the calendar.</summary>
	/// <param name="holidayId">The holiday to remove.</param>
	Task<ApiResponse<bool>?> DeleteAsync(Guid holidayId);

	/// <summary>Changes whether every Sunday counts as a holiday.</summary>
	/// <param name="request">The new setting.</param>
	Task<ApiResponse<bool>?> SetSettingsAsync(HolidaySettingsRequest request);
}
