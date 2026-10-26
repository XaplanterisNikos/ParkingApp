using ParkingApp.Shared.Spots;

namespace ParkingApp.Shared.Pricing;

/// <summary>The prices of one spot size on one kind of day. All amounts include VAT.</summary>
public class TariffRateDto
{
	/// <summary>The spot size these prices apply to.</summary>
	public SpotSize Size { get; set; }

	/// <summary>Regular day or holiday.</summary>
	public DayType DayType { get; set; }

	/// <summary>The price of one step that starts during the day.</summary>
	public decimal DayStepPrice { get; set; }

	/// <summary>The price of one step that starts during the night period.</summary>
	public decimal NightStepPrice { get; set; }

	/// <summary>The most charged for each 24 hours from entry; null = no limit.</summary>
	public decimal? DailyCap { get; set; }
}
