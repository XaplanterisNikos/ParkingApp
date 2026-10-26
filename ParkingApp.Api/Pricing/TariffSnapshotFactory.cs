using ParkingApp.Api.Data.Entities;

namespace ParkingApp.Api.Pricing;

/// <summary>
/// Copies a stored price list version into the plain snapshot the calculator works with.
/// Keeping this in one place means the calculator never sees EF entities.
/// </summary>
public static class TariffSnapshotFactory
{
	/// <summary>Builds the calculator's view of a version. Its rates must be loaded.</summary>
	/// <param name="version">A version loaded with its <see cref="TariffVersion.Rates"/>.</param>
	public static TariffSnapshot From(TariffVersion version) => new(
		version.StepMinutes,
		version.GraceMinutes,
		version.NightStart,
		version.NightEnd,
		version.OvernightCheckoutTime,
		version.Rates.ToDictionary(
			rate => new RateKey(rate.Size, rate.DayType),
			rate => new RateSnapshot(rate.DayStepPrice, rate.NightStepPrice, rate.DailyCap)));
}
