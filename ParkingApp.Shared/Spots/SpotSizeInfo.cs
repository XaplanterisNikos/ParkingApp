namespace ParkingApp.Shared.Spots;

/// <summary>
/// Maps each <see cref="SpotSize"/> to its code used in spot naming (e.g. "C", "M", "L").
/// </summary>
public static class SpotSizeInfo
{
	/// <summary>The short code used when naming spots.</summary>
	public static string CodeOf(SpotSize size) => size switch
	{
		SpotSize.Motorcycle => "M",
		SpotSize.Car => "C",
		SpotSize.LargeCar => "L",
		_ => "?"
	};

	/// <summary>The human-readable name shown in the UI.</summary>
	public static string NameOf(SpotSize size) => size switch
	{
		SpotSize.Motorcycle => "Μηχανή",
		SpotSize.Car => "Αυτοκίνητο",
		SpotSize.LargeCar => "Μεγάλο όχημα",
		_ => "Άγνωστο"
	};
}
