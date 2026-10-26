using ParkingApp.Shared.Pricing;

namespace ParkingApp.Shared.Tickets;

/// <summary>
/// One way to charge a ticket at exit. There is more than one when the price list changed
/// while the vehicle was inside, or when an overnight package was exceeded.
/// </summary>
public class ExitChargeOptionDto
{
	/// <summary>The price list version used for this amount; sent back to confirm the exit.</summary>
	public Guid TariffVersionId { get; set; }

	/// <summary>When that version was saved by the owner.</summary>
	public DateTimeOffset VersionCreatedAt { get; set; }

	/// <summary>True when this is the version that was current at entry.</summary>
	public bool IsEntryVersion { get; set; }

	/// <summary>True when this is the branch's current version.</summary>
	public bool IsCurrentVersion { get; set; }

	/// <summary>
	/// How the time after an overnight package is charged; null when there is no package
	/// or nothing after it to charge. Sent back to confirm the exit.
	/// </summary>
	public PackageOverstay? Overstay { get; set; }

	/// <summary>The amount, VAT included, at the time of the quote.</summary>
	public decimal Amount { get; set; }

	/// <summary>The VAT rate (percent) of this version.</summary>
	public decimal VatRate { get; set; }
}
