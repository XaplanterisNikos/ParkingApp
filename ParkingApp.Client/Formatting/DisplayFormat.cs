using System.Globalization;

namespace ParkingApp.Client.Formatting;

/// <summary>
/// How amounts, times and durations are shown in the UI. Kept in one place, and written
/// without the browser's culture data, so every screen shows them the same way.
/// </summary>
public static class DisplayFormat
{
	/// <summary>An amount in euros, Greek style: "12,50 €".</summary>
	/// <param name="amount">The amount.</param>
	public static string Money(decimal amount) =>
		amount.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',') + " €";

	/// <summary>
	/// A UTC-based time as hours and minutes in Athens time. If the browser has no time zone
	/// data, it falls back to the browser's local time instead of failing.
	/// </summary>
	/// <param name="time">The time to show.</param>
	/// <param name="format">The format; hours and minutes by default.</param>
	public static string AthensTime(DateTimeOffset time, string format = "HH:mm")
	{
		try
		{
			var athens = TimeZoneInfo.FindSystemTimeZoneById("Europe/Athens");
			return TimeZoneInfo.ConvertTime(time, athens).ToString(format, CultureInfo.InvariantCulture);
		}
		catch (TimeZoneNotFoundException)
		{
			return time.ToLocalTime().ToString(format, CultureInfo.InvariantCulture);
		}
	}

	/// <summary>A stay's length in words: "45 λεπ.", "2 ώρ. 15 λεπ.", "1 ημ. 3 ώρ.".</summary>
	/// <param name="duration">The length of the stay.</param>
	public static string Duration(TimeSpan duration)
	{
		if (duration < TimeSpan.Zero)
		{
			duration = TimeSpan.Zero;
		}

		if (duration.TotalDays >= 1)
		{
			return $"{(int)duration.TotalDays} ημ. {duration.Hours} ώρ.";
		}

		return duration.TotalHours >= 1
			? $"{(int)duration.TotalHours} ώρ. {duration.Minutes} λεπ."
			: $"{duration.Minutes} λεπ.";
	}
}
