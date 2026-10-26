namespace ParkingApp.Shared.Pricing;

/// <summary>The company's holiday calendar: the Sunday setting and the marked dates.</summary>
public class HolidayCalendarDto
{
	/// <summary>True when every Sunday is charged with holiday prices.</summary>
	public bool SundaysAreHolidays { get; set; }

	/// <summary>The marked dates, in date order.</summary>
	public List<HolidayDto> Holidays { get; set; } = new();
}
