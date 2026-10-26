namespace ParkingApp.Shared.Pricing;

/// <summary>
/// A complete price list as the owner saves it. Saving never changes an existing version:
/// it always creates a new one, which becomes the current price list of the branch.
/// </summary>
public class SaveTariffRequest
{
	/// <summary>The VAT rate in percent (0–100). All prices include VAT.</summary>
	public decimal VatRate { get; set; }

	/// <summary>The charging step in minutes: 15, 30 or 60.</summary>
	public int StepMinutes { get; set; }

	/// <summary>A stay of up to this many minutes is free (0–120).</summary>
	public int GraceMinutes { get; set; }

	/// <summary>Start of the night period (Athens time).</summary>
	public TimeOnly NightStart { get; set; }

	/// <summary>End of the night period (Athens time). Equal to the start = no night period.</summary>
	public TimeOnly NightEnd { get; set; }

	/// <summary>The time an overnight package ends on its last day (Athens time).</summary>
	public TimeOnly OvernightCheckoutTime { get; set; }

	/// <summary>
	/// The prices. Regular prices are required for every spot size that the branch has;
	/// holiday prices are optional (without them, holidays use the regular prices).
	/// </summary>
	public List<TariffRateDto> Rates { get; set; } = new();

	/// <summary>The overnight packages (may be empty).</summary>
	public List<OvernightPackageDto> Packages { get; set; } = new();
}
