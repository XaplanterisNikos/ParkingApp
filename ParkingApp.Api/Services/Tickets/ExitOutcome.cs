namespace ParkingApp.Api.Services.Tickets;

/// <summary>
/// The possible results of an exit quote or an exit. The controller maps each one to an HTTP status.
/// </summary>
public enum ExitOutcome
{
	/// <summary>The quote was made, or the vehicle exited (200).</summary>
	Success,

	/// <summary>The ticket does not exist in the active branch (404).</summary>
	TicketNotFound,

	/// <summary>The ticket was already closed, e.g. by another desk (409).</summary>
	AlreadyExited,

	/// <summary>No price list can charge this ticket (409): none saved, or no prices for the spot size.</summary>
	NoPriceList,

	/// <summary>The chosen way to charge is not one of the allowed ones (400).</summary>
	InvalidChoice
}
