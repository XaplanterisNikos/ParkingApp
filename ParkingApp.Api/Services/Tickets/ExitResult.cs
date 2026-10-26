namespace ParkingApp.Api.Services.Tickets;

/// <summary>The result of an exit quote or an exit.</summary>
/// <typeparam name="T">The value returned on success.</typeparam>
/// <param name="Outcome">What happened.</param>
/// <param name="Value">The quote or the closed ticket; set only on success.</param>
/// <param name="Message">A message for the employee; set only on failure.</param>
public record ExitResult<T>(ExitOutcome Outcome, T? Value = default, string? Message = null)
{
	/// <summary>Builds a successful result.</summary>
	public static ExitResult<T> Success(T value) => new(ExitOutcome.Success, value);

	/// <summary>Builds a failed result with a message for the employee.</summary>
	public static ExitResult<T> Failure(ExitOutcome outcome, string message) => new(outcome, default, message);
}
