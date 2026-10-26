namespace ParkingApp.Shared.Pricing;

/// <summary>Adds a date to the company's holiday calendar.</summary>
public class CreateHolidayRequest
{
	/// <summary>The date of the holiday (Athens date).</summary>
	public DateOnly Date { get; set; }

	/// <summary>A name for the day (e.g. "28η Οκτωβρίου").</summary>
	public string Name { get; set; } = string.Empty;
}
