namespace ParkingApp.Shared.Floors;

/// <summary>
/// Maps each <see cref="FloorType"/> to its fixed code (for spot naming) and display name.
/// Single source of truth for floor labels/codes.
/// </summary>
public static class FloorTypeInfo
{
	/// <summary>The short code used when naming spots (e.g. "A", "L", "BA").</summary>
	public static string CodeOf(FloorType type) => type switch
	{
		FloorType.Basement2 => "BB",
		FloorType.Basement1 => "BA",
		FloorType.Ground => "L",
		FloorType.First => "A",
		FloorType.Second => "B",
		FloorType.Third => "C",
		FloorType.Fourth => "D",
		FloorType.Fifth => "E",
		FloorType.Sixth => "F",
		FloorType.Seventh => "G",   
		FloorType.Eighth => "H",
		FloorType.Ninth => "I",
		FloorType.Tenth => "J",
		_ => "?"
	};

	/// <summary>The human-readable name shown in the UI.</summary>
	public static string NameOf(FloorType type) => type switch
	{
		FloorType.Basement2 => "Υπόγειο 2",
		FloorType.Basement1 => "Υπόγειο 1",
		FloorType.Ground => "Ισόγειο",
		FloorType.First => "1ος όροφος",
		FloorType.Second => "2ος όροφος",
		FloorType.Third => "3ος όροφος",
		FloorType.Fourth => "4ος όροφος",
		FloorType.Fifth => "5ος όροφος",
		FloorType.Sixth => "6ος όροφος",
		FloorType.Seventh => "7ος όροφος",
		FloorType.Eighth => "8ος όροφος",
		FloorType.Ninth => "9ος όροφος",
		FloorType.Tenth => "10ος όροφος",
		_ => "Άγνωστος"
	};
}