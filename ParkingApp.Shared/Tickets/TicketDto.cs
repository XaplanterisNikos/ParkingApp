namespace ParkingApp.Shared.Tickets;

/// <summary>
/// A parking ticket as the console sees it: enough to show and print the receipt of an entry.
/// </summary>
public class TicketDto
{
	/// <summary>The ticket's unique identifier.</summary>
	public Guid Id { get; set; }

	/// <summary>The spot the vehicle was assigned to.</summary>
	public Guid SpotId { get; set; }

	/// <summary>The spot's code (e.g. "AC12"), shown on the receipt.</summary>
	public required string SpotNumber { get; set; }

	/// <summary>
	/// The plate as stored (normalized, Latin letters).
	/// Use <see cref="LicensePlateNormalizer.ToDisplay"/> with <see cref="PlateKind"/> to show it.
	/// </summary>
	public required string LicensePlate { get; set; }

	/// <summary>The kind of the plate (Greek or foreign).</summary>
	public PlateKind PlateKind { get; set; }

	/// <summary>When the vehicle entered (UTC-based, with offset).</summary>
	public DateTimeOffset EnteredAt { get; set; }

	/// <summary>The name of the overnight package chosen at entry; null when the price list applies.</summary>
	public string? OvernightPackageName { get; set; }

	/// <summary>When the vehicle exited; null while the ticket is active.</summary>
	public DateTimeOffset? ExitedAt { get; set; }

	/// <summary>The amount charged at exit, VAT included; null while the ticket is active.</summary>
	public decimal? AmountCharged { get; set; }

	/// <summary>The VAT rate (percent) of the charge; null while the ticket is active.</summary>
	public decimal? VatRate { get; set; }
}
