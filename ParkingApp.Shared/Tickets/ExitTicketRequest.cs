using ParkingApp.Shared.Pricing;

namespace ParkingApp.Shared.Tickets;

/// <summary>
/// The employee's confirmation of an exit: which of the quoted ways to charge.
/// No amount is sent: the server calculates it at the moment of exit.
/// </summary>
public class ExitTicketRequest
{
	/// <summary>The price list version chosen (the entry version or the current one).</summary>
	public Guid TariffVersionId { get; set; }

	/// <summary>How to charge time after an overnight package; null = with the price list.</summary>
	public PackageOverstay? Overstay { get; set; }
}
