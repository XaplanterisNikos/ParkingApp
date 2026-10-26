using ParkingApp.Shared.Pricing;

namespace ParkingApp.Api.Services.Pricing;

/// <summary>The result of saving a price list.</summary>
/// <param name="Outcome">What happened.</param>
/// <param name="Tariff">The new current price list; set only when saved.</param>
/// <param name="Errors">The broken rules, in words for the owner; empty unless invalid.</param>
public record TariffSaveResult(TariffSaveOutcome Outcome, TariffDto? Tariff, List<string> Errors);
