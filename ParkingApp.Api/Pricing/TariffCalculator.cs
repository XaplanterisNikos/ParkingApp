using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Pricing;

/// <summary>
/// Calculates the charge of a stay from a price list. It has no database access and no
/// clock: everything comes in as arguments, so the same input always gives the same amount.
/// All prices include VAT; the result is rounded to cents.
/// </summary>
/// <remarks>
/// Times are passed as <see cref="DateTimeOffset"/> (UTC-based). Night periods, holidays and the
/// package checkout time are local rules, so every check converts to the calculator's time zone.
/// Steps are counted in real elapsed time, so daylight saving changes are handled correctly.
/// </remarks>
public class TariffCalculator
{
	// The local time zone of the parking (Europe/Athens in production)
	private readonly TimeZoneInfo _timeZone;

	/// <summary>
	/// Creates the calculator.
	/// </summary>
	/// <param name="timeZone">The time zone used for night periods, holidays and checkout times.</param>
	public TariffCalculator(TimeZoneInfo timeZone)
	{
		_timeZone = timeZone;
	}

	/// <summary>
	/// Charges a stay with the normal price list.
	/// </summary>
	/// <param name="tariff">The price list version.</param>
	/// <param name="size">The size of the spot (charging depends on the spot, not the vehicle).</param>
	/// <param name="holidays">The company's holiday calendar.</param>
	/// <param name="from">When the charged time starts.</param>
	/// <param name="to">When the charged time ends.</param>
	/// <returns>The amount, VAT included, rounded to cents.</returns>
	/// <remarks>
	/// Rules: a stay within the grace minutes is free; otherwise every started step is charged
	/// in full, at the price of the moment the step starts (day or night, regular day or holiday);
	/// for each 24 hours from <paramref name="from"/>, the total is limited by the daily cap of
	/// the day on which those 24 hours start.
	/// </remarks>
	/// <exception cref="ArgumentException">The step is not greater than zero.</exception>
	public decimal CalculatePriceList(
		TariffSnapshot tariff,
		SpotSize size,
		HolidayCalendar holidays,
		DateTimeOffset from,
		DateTimeOffset to)
	{
		if (tariff.StepMinutes <= 0)
		{
			// A zero step would never move forward
			throw new ArgumentException("The charging step must be greater than zero.", nameof(tariff));
		}

		// Nothing to charge, or a stay short enough to be free
		if (to <= from || to - from <= TimeSpan.FromMinutes(tariff.GraceMinutes))
		{
			return 0m;
		}

		var step = TimeSpan.FromMinutes(tariff.StepMinutes);
		var total = 0m;

		// Walk the stay in blocks of 24 hours from the start; each block has its own cap
		for (var blockStart = from; blockStart < to; blockStart = blockStart.AddHours(24))
		{
			var blockEnd = blockStart.AddHours(24) < to ? blockStart.AddHours(24) : to;
			var blockTotal = 0m;

			// Every step that STARTS inside the block is charged in full
			for (var stepStart = blockStart; stepStart < blockEnd; stepStart += step)
			{
				blockTotal += StepPrice(tariff, size, holidays, stepStart);
			}

			// The cap of the day on which the block starts
			var cap = RateAt(tariff, size, holidays, blockStart).DailyCap;
			total += cap is { } limit ? Math.Min(blockTotal, limit) : blockTotal;
		}

		return Round(total);
	}

	/// <summary>
	/// Charges a stay that has an overnight package. The package replaces the price list until
	/// it ends; the time after its end is charged as the employee chose.
	/// </summary>
	/// <param name="tariff">The price list version (for the checkout time and any extra time).</param>
	/// <param name="size">The size of the spot.</param>
	/// <param name="holidays">The company's holiday calendar.</param>
	/// <param name="package">The overnight package chosen at entry.</param>
	/// <param name="overstay">How the time after the package is charged.</param>
	/// <param name="entered">When the vehicle entered.</param>
	/// <param name="exited">When the vehicle exits.</param>
	/// <returns>The amount, VAT included, rounded to cents.</returns>
	/// <remarks>
	/// The package ends on the local date of entry plus <see cref="PackageSnapshot.Nights"/> days,
	/// at the checkout time. The grace minutes also apply after the end of the package.
	/// </remarks>
	/// <exception cref="ArgumentException">The package covers less than one night.</exception>
	public decimal CalculatePackage(
		TariffSnapshot tariff,
		SpotSize size,
		HolidayCalendar holidays,
		PackageSnapshot package,
		PackageOverstay overstay,
		DateTimeOffset entered,
		DateTimeOffset exited)
	{
		if (package.Nights < 1)
		{
			throw new ArgumentException("A package must cover at least one night.", nameof(package));
		}

		var grace = TimeSpan.FromMinutes(tariff.GraceMinutes);
		var nights = TimeSpan.FromDays(package.Nights);

		// End of the package, in local time: entry date + N days, at the checkout time
		var localEnd = ToLocal(entered).Date.AddDays(package.Nights) + tariff.OvernightCheckoutTime.ToTimeSpan();
		var packageEnd = FromLocal(localEnd);

		var total = package.Price;

		if (overstay == PackageOverstay.RenewPackage)
		{
			// Each new period that starts (beyond the free minutes) costs the package again
			while (exited - packageEnd > grace)
			{
				total += package.Price;
				localEnd += nights;
				packageEnd = FromLocal(localEnd);
			}
		}
		else
		{
			// The extra time is charged with the normal price list (its own grace check included)
			total += CalculatePriceList(tariff, size, holidays, packageEnd, exited);
		}

		return Round(total);
	}

	/// <summary>The price of one step, from the moment it starts.</summary>
	private decimal StepPrice(TariffSnapshot tariff, SpotSize size, HolidayCalendar holidays, DateTimeOffset stepStart)
	{
		var rate = RateAt(tariff, size, holidays, stepStart);
		var localTime = TimeOnly.FromDateTime(ToLocal(stepStart));

		// IsBetween handles a night period that crosses midnight (e.g. 22:00–06:00);
		// equal start and end means there is no night period
		var isNight = localTime.IsBetween(tariff.NightStart, tariff.NightEnd);

		return isNight ? rate.NightStepPrice : rate.DayStepPrice;
	}

	/// <summary>The prices that apply at a moment: regular or holiday, by the local date.</summary>
	private RateSnapshot RateAt(TariffSnapshot tariff, SpotSize size, HolidayCalendar holidays, DateTimeOffset moment)
	{
		var localDate = DateOnly.FromDateTime(ToLocal(moment));
		var dayType = holidays.IsHoliday(localDate) ? DayType.Holiday : DayType.Regular;
		return tariff.RateFor(size, dayType);
	}

	/// <summary>Converts a moment to local wall-clock time.</summary>
	private DateTime ToLocal(DateTimeOffset moment) =>
		TimeZoneInfo.ConvertTime(moment, _timeZone).DateTime;

	/// <summary>Converts a local wall-clock time to a moment, with the offset valid at that time.</summary>
	private DateTimeOffset FromLocal(DateTime localTime) =>
		new(localTime, _timeZone.GetUtcOffset(localTime));

	/// <summary>Rounds an amount to cents, with halves rounded up (0.005 → 0.01).</summary>
	private static decimal Round(decimal amount) =>
		Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
