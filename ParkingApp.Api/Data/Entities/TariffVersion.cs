namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// One version of a branch's price list. A version is never changed after it is created:
/// every change by the owner creates a new version, so a ticket can always be charged with
/// the version that was valid when the vehicle entered. The newest version is the current one.
/// </summary>
public class TariffVersion : TenantEntity
{
	/// <summary>The branch this price list belongs to.</summary>
	public Guid BranchId { get; set; }

	/// <summary>When the owner saved this version. The newest one is the current price list.</summary>
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>The VAT rate in percent (e.g. 24). All prices include VAT.</summary>
	public decimal VatRate { get; set; }

	/// <summary>The charging step in minutes: 15, 30 or 60. Every started step is charged in full.</summary>
	public int StepMinutes { get; set; }

	/// <summary>A stay of up to this many minutes is free (0 = no free time).</summary>
	public int GraceMinutes { get; set; }

	/// <summary>Start of the night period (Athens time), e.g. 00:00.</summary>
	public TimeOnly NightStart { get; set; }

	/// <summary>End of the night period (Athens time), e.g. 07:00. Equal to the start = no night period.</summary>
	public TimeOnly NightEnd { get; set; }

	/// <summary>The time an overnight package ends on its last day (Athens time), e.g. 12:00.</summary>
	public TimeOnly OvernightCheckoutTime { get; set; }

	/// <summary>The prices of this version, one row per spot size and day type.</summary>
	public ICollection<TariffRate> Rates { get; set; } = new List<TariffRate>();

	/// <summary>The overnight packages of this version.</summary>
	public ICollection<OvernightPackage> Packages { get; set; } = new List<OvernightPackage>();
}
