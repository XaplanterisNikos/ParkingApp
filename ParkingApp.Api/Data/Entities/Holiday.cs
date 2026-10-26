namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// A day the owner marked as a holiday. Holidays belong to the company, so every
/// branch of the owner uses the same calendar; each branch has its own holiday prices.
/// </summary>
public class Holiday : TenantEntity
{
	/// <summary>The calendar date of the holiday (Athens time).</summary>
	public DateOnly Date { get; set; }

	/// <summary>A name the owner gives to the day (e.g. "28η Οκτωβρίου").</summary>
	public required string Name { get; set; }
}
