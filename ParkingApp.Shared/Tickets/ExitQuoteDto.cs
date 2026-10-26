namespace ParkingApp.Shared.Tickets;

/// <summary>
/// What an exit would cost right now: the ticket and every allowed way to charge it.
/// The amounts are only a preview: the server calculates again at the moment of exit.
/// </summary>
public class ExitQuoteDto
{
	/// <summary>The active ticket.</summary>
	public required TicketDto Ticket { get; set; }

	/// <summary>The moment the amounts were calculated.</summary>
	public DateTimeOffset QuotedAt { get; set; }

	/// <summary>The allowed charges; the first one is the suggested choice.</summary>
	public List<ExitChargeOptionDto> Options { get; set; } = new();
}
