namespace ParkingApp.Shared.Pricing;

/// <summary>
/// The kind of day a price applies to. A holiday uses its own set of prices.
/// Stored as int; explicit values keep them stable if new kinds are added later.
/// </summary>
public enum DayType
{
	/// <summary>A normal working day.</summary>
	Regular = 1,

	/// <summary>A day the owner marked as a holiday (or a Sunday, when the owner chose so).</summary>
	Holiday = 2
}
