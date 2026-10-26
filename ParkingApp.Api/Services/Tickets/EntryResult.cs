using ParkingApp.Shared.Tickets;

namespace ParkingApp.Api.Services.Tickets;

/// <summary>
/// The result of a vehicle entry: what happened, the new ticket on success,
/// and a message for the employee on failure.
/// </summary>
/// <param name="Outcome">What happened.</param>
/// <param name="Ticket">The issued ticket; set only when <paramref name="Outcome"/> is <see cref="EntryOutcome.Created"/>.</param>
/// <param name="Message">A message for the employee; set only on failure.</param>
public record EntryResult(EntryOutcome Outcome, TicketDto? Ticket = null, string? Message = null)
{
	/// <summary>Builds a successful result.</summary>
	public static EntryResult Success(TicketDto ticket) => new(EntryOutcome.Created, ticket);

	/// <summary>Builds a failed result with a message for the employee.</summary>
	public static EntryResult Failure(EntryOutcome outcome, string message) => new(outcome, null, message);
}
