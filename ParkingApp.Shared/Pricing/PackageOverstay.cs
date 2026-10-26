namespace ParkingApp.Shared.Pricing;

/// <summary>
/// How the time after the end of an overnight package is charged.
/// The employee chooses it at exit.
/// </summary>
public enum PackageOverstay
{
	/// <summary>The extra time is charged with the normal price list.</summary>
	PriceList = 1,

	/// <summary>The package is charged again for every new period it starts.</summary>
	RenewPackage = 2
}
