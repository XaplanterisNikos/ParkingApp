namespace ParkingApp.Shared.Pricing;

/// <summary>Changes the calendar-wide holiday settings of the company.</summary>
public class HolidaySettingsRequest
{
	/// <summary>True when every Sunday is charged with holiday prices.</summary>
	public bool SundaysAreHolidays { get; set; }
}
