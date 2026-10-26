namespace ParkingApp.Api.Pricing;

/// <summary>The holiday calendar of a company, as the calculator sees it.</summary>
/// <param name="Dates">The dates the owner marked as holidays (local dates).</param>
/// <param name="SundaysAreHolidays">True when every Sunday is also a holiday.</param>
public sealed record HolidayCalendar(IReadOnlySet<DateOnly> Dates, bool SundaysAreHolidays)
{
	/// <summary>A calendar with no holidays at all.</summary>
	public static HolidayCalendar None { get; } = new(new HashSet<DateOnly>(), false);

	/// <summary>True when the given local date is charged with holiday prices.</summary>
	/// <param name="date">A local (Athens) date.</param>
	public bool IsHoliday(DateOnly date) =>
		Dates.Contains(date) || (SundaysAreHolidays && date.DayOfWeek == DayOfWeek.Sunday);
}
