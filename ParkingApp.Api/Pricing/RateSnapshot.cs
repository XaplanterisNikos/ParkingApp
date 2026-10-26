namespace ParkingApp.Api.Pricing;

/// <summary>The prices of one spot size on one kind of day. All amounts include VAT.</summary>
/// <param name="DayStepPrice">The price of a step that starts during the day.</param>
/// <param name="NightStepPrice">The price of a step that starts during the night period.</param>
/// <param name="DailyCap">The most charged for each 24 hours from entry; null = no limit.</param>
public sealed record RateSnapshot(decimal DayStepPrice, decimal NightStepPrice, decimal? DailyCap);
