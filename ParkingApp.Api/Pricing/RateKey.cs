using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Api.Pricing;

/// <summary>Identifies one set of prices in a price list: a spot size on a kind of day.</summary>
/// <param name="Size">The spot size.</param>
/// <param name="DayType">Regular day or holiday.</param>
public readonly record struct RateKey(SpotSize Size, DayType DayType);
