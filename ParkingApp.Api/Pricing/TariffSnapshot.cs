using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Pricing;

/// <summary>
/// What the calculator needs to know about one price list version. It is a plain copy of the
/// stored version, so the calculator does not depend on the database and is easy to test.
/// </summary>
/// <param name="StepMinutes">The charging step in minutes (must be greater than zero).</param>
/// <param name="GraceMinutes">A stay of up to this many minutes is free.</param>
/// <param name="NightStart">Start of the night period (local time).</param>
/// <param name="NightEnd">End of the night period (local time). Equal to the start = no night period.</param>
/// <param name="OvernightCheckoutTime">The time an overnight package ends on its last day (local time).</param>
/// <param name="Rates">The prices, by spot size and day type.</param>
public sealed record TariffSnapshot(
	int StepMinutes,
	int GraceMinutes,
	TimeOnly NightStart,
	TimeOnly NightEnd,
	TimeOnly OvernightCheckoutTime,
	IReadOnlyDictionary<RateKey, RateSnapshot> Rates)
{
	/// <summary>
	/// Returns the prices for a spot size on a kind of day. When a holiday has no prices of
	/// its own, the regular prices are used.
	/// </summary>
	/// <exception cref="InvalidOperationException">The price list has no prices for this spot size.</exception>
	public RateSnapshot RateFor(SpotSize size, DayType dayType)
	{
		if (Rates.TryGetValue(new RateKey(size, dayType), out var rate))
		{
			return rate;
		}

		if (dayType != DayType.Regular && Rates.TryGetValue(new RateKey(size, DayType.Regular), out var regular))
		{
			return regular;
		}

		throw new InvalidOperationException($"The price list has no prices for spot size {size}.");
	}
}
