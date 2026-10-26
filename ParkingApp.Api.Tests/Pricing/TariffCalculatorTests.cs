using ParkingApp.Api.Pricing;
using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Spots;
using Xunit;

namespace ParkingApp.Api.Tests.Pricing;

/// <summary>
/// One test per pricing rule. All dates are in November 2026, when Athens is at UTC+02:00
/// (2026-11-01 is a Sunday, 2026-11-02 a Monday). 2026-10-28 is a Wednesday.
/// </summary>
public class TariffCalculatorTests
{
	// The same zone as production, so night periods and holidays are checked in Athens time
	private static readonly TariffCalculator Calculator =
		new(TimeZoneInfo.FindSystemTimeZoneById("Europe/Athens"));

	// Athens offset in November (winter time)
	private static readonly TimeSpan AthensWinter = TimeSpan.FromHours(2);

	/// <summary>A moment in Athens time, on the given day of November 2026.</summary>
	private static DateTimeOffset Nov(int day, int hour, int minute = 0) =>
		new(2026, 11, day, hour, minute, 0, AthensWinter);

	/// <summary>
	/// A price list for cars: day 2.00 per step, night 1.00 per step, holiday day 3.00 per step,
	/// with the given step, grace, night period and daily cap.
	/// </summary>
	private static TariffSnapshot CarTariff(
		int stepMinutes = 60,
		int graceMinutes = 0,
		TimeOnly? nightStart = null,
		TimeOnly? nightEnd = null,
		decimal? dailyCap = null,
		bool withHolidayPrices = true)
	{
		var rates = new Dictionary<RateKey, RateSnapshot>
		{
			[new RateKey(SpotSize.Car, DayType.Regular)] = new(2.00m, 1.00m, dailyCap)
		};

		if (withHolidayPrices)
		{
			rates[new RateKey(SpotSize.Car, DayType.Holiday)] = new(3.00m, 1.50m, dailyCap);
		}

		return new TariffSnapshot(
			stepMinutes,
			graceMinutes,
			// No night period unless a test asks for one (equal start and end)
			nightStart ?? new TimeOnly(0, 0),
			nightEnd ?? new TimeOnly(0, 0),
			new TimeOnly(12, 0),
			rates);
	}

	[Fact]
	public void A_stay_within_the_grace_minutes_is_free()
	{
		var amount = Calculator.CalculatePriceList(
			CarTariff(graceMinutes: 10), SpotSize.Car, HolidayCalendar.None, Nov(2, 10, 0), Nov(2, 10, 10));

		Assert.Equal(0m, amount);
	}

	[Fact]
	public void After_the_grace_minutes_the_whole_stay_is_charged()
	{
		// 11 minutes with 10 free minutes: one started hour
		var amount = Calculator.CalculatePriceList(
			CarTariff(graceMinutes: 10), SpotSize.Car, HolidayCalendar.None, Nov(2, 10, 0), Nov(2, 10, 11));

		Assert.Equal(2.00m, amount);
	}

	[Fact]
	public void Every_started_step_is_charged_in_full()
	{
		// 1 hour 5 minutes with 60-minute steps = 2 steps
		var amount = Calculator.CalculatePriceList(
			CarTariff(), SpotSize.Car, HolidayCalendar.None, Nov(2, 10, 0), Nov(2, 11, 5));

		Assert.Equal(4.00m, amount);
	}

	[Fact]
	public void A_shorter_step_charges_by_the_quarter_hour()
	{
		// 50 minutes with 15-minute steps = 4 steps of 2.00
		var amount = Calculator.CalculatePriceList(
			CarTariff(stepMinutes: 15), SpotSize.Car, HolidayCalendar.None, Nov(2, 10, 0), Nov(2, 10, 50));

		Assert.Equal(8.00m, amount);
	}

	[Fact]
	public void Each_step_uses_the_price_of_the_moment_it_starts()
	{
		// Night 00:00–07:00, 30-minute steps: 23:30 is a day step, 00:00 a night step
		var tariff = CarTariff(stepMinutes: 30, nightStart: new TimeOnly(0, 0), nightEnd: new TimeOnly(7, 0));

		var amount = Calculator.CalculatePriceList(
			tariff, SpotSize.Car, HolidayCalendar.None, Nov(2, 23, 30), Nov(3, 0, 30));

		Assert.Equal(2.00m + 1.00m, amount);
	}

	[Fact]
	public void A_night_period_that_crosses_midnight_is_handled()
	{
		// Night 22:00–06:00: steps at 21:00 (day), 22:00 and 23:00 (night)
		var tariff = CarTariff(nightStart: new TimeOnly(22, 0), nightEnd: new TimeOnly(6, 0));

		var amount = Calculator.CalculatePriceList(
			tariff, SpotSize.Car, HolidayCalendar.None, Nov(2, 21, 0), Nov(3, 0, 0));

		Assert.Equal(2.00m + 1.00m + 1.00m, amount);
	}

	[Fact]
	public void The_daily_cap_limits_each_24_hours_from_entry()
	{
		// 30 hours, cap 20: first 24 hours = min(48, 20); next 6 hours = 12
		var amount = Calculator.CalculatePriceList(
			CarTariff(dailyCap: 20.00m), SpotSize.Car, HolidayCalendar.None, Nov(2, 8, 0), Nov(3, 14, 0));

		Assert.Equal(20.00m + 12.00m, amount);
	}

	[Fact]
	public void A_marked_holiday_uses_the_holiday_prices()
	{
		var holidays = new HolidayCalendar(new HashSet<DateOnly> { new(2026, 10, 28) }, SundaysAreHolidays: false);
		var at = new DateTimeOffset(2026, 10, 28, 10, 0, 0, AthensWinter);

		var amount = Calculator.CalculatePriceList(CarTariff(), SpotSize.Car, holidays, at, at.AddHours(1));

		Assert.Equal(3.00m, amount);
	}

	[Theory]
	// Attributes cannot hold decimals, so the expected amount is given in whole euros
	[InlineData(true, 6)]
	[InlineData(false, 4)]
	public void Sundays_are_holidays_only_when_the_owner_chose_so(bool sundaysAreHolidays, int expectedEuros)
	{
		var holidays = new HolidayCalendar(new HashSet<DateOnly>(), sundaysAreHolidays);

		// Sunday 1 November, 2 hours
		var amount = Calculator.CalculatePriceList(CarTariff(), SpotSize.Car, holidays, Nov(1, 10, 0), Nov(1, 12, 0));

		Assert.Equal(expectedEuros, amount);
	}

	[Fact]
	public void A_holiday_without_its_own_prices_uses_the_regular_prices()
	{
		var holidays = new HolidayCalendar(new HashSet<DateOnly>(), SundaysAreHolidays: true);

		var amount = Calculator.CalculatePriceList(
			CarTariff(withHolidayPrices: false), SpotSize.Car, holidays, Nov(1, 10, 0), Nov(1, 11, 0));

		Assert.Equal(2.00m, amount);
	}

	[Fact]
	public void A_spot_size_without_prices_is_an_error()
	{
		Assert.Throws<InvalidOperationException>(() => Calculator.CalculatePriceList(
			CarTariff(), SpotSize.LargeCar, HolidayCalendar.None, Nov(2, 10, 0), Nov(2, 11, 0)));
	}

	[Fact]
	public void A_package_covers_the_stay_until_checkout_on_its_last_day()
	{
		// 1 night from Monday 18:00 ends Tuesday 12:00; exit at 11:00 → only the package
		var amount = Calculator.CalculatePackage(
			CarTariff(), SpotSize.Car, HolidayCalendar.None, new PackageSnapshot(1, 15.00m),
			PackageOverstay.PriceList, Nov(2, 18, 0), Nov(3, 11, 0));

		Assert.Equal(15.00m, amount);
	}

	[Fact]
	public void Time_after_the_package_is_charged_with_the_price_list()
	{
		// Package ends Tuesday 12:00; exit 14:00 → 2 extra hours at 2.00
		var amount = Calculator.CalculatePackage(
			CarTariff(), SpotSize.Car, HolidayCalendar.None, new PackageSnapshot(1, 15.00m),
			PackageOverstay.PriceList, Nov(2, 18, 0), Nov(3, 14, 0));

		Assert.Equal(15.00m + 4.00m, amount);
	}

	[Fact]
	public void A_renewed_package_is_charged_again_for_each_new_period()
	{
		// Package ends Tuesday 12:00; exit 14:00 starts a second period
		var amount = Calculator.CalculatePackage(
			CarTariff(), SpotSize.Car, HolidayCalendar.None, new PackageSnapshot(1, 15.00m),
			PackageOverstay.RenewPackage, Nov(2, 18, 0), Nov(3, 14, 0));

		Assert.Equal(30.00m, amount);
	}

	[Fact]
	public void The_grace_minutes_also_apply_after_the_package()
	{
		// Exit 5 minutes after checkout, with 10 free minutes → no renewal
		var amount = Calculator.CalculatePackage(
			CarTariff(graceMinutes: 10), SpotSize.Car, HolidayCalendar.None, new PackageSnapshot(1, 15.00m),
			PackageOverstay.RenewPackage, Nov(2, 18, 0), Nov(3, 12, 5));

		Assert.Equal(15.00m, amount);
	}
}
