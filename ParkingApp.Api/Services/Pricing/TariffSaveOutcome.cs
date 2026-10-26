namespace ParkingApp.Api.Services.Pricing;

/// <summary>The possible results of saving a price list. The controller maps each one to an HTTP status.</summary>
public enum TariffSaveOutcome
{
	/// <summary>A new version was created (200).</summary>
	Saved,

	/// <summary>The branch does not exist for this company (404).</summary>
	BranchNotFound,

	/// <summary>The price list breaks one or more rules (400).</summary>
	Invalid
}
