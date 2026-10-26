namespace ParkingApp.Shared.Pricing;

/// <summary>A day in the company's holiday calendar.</summary>
public class HolidayDto
{
	/// <summary>The holiday's id (used to remove it).</summary>
	public Guid Id { get; set; }

	/// <summary>The date of the holiday (Athens date).</summary>
	public DateOnly Date { get; set; }

	/// <summary>The name the owner gave to the day.</summary>
	public string Name { get; set; } = string.Empty;
}
