namespace ParkingApp.Shared.Floors;

/// <summary>
/// The predefined floor types a branch can have. Each maps to a fixed code (used in
/// spot naming) and a display name. A branch can have each type at most once.
/// </summary>
public enum FloorType
{
	Basement2 = -2,
	Basement1 = -1,
	Ground = 0,
	First = 1,
	Second = 2,
	Third = 3,
	Fourth = 4,
	Fifth = 5,
	Sixth = 6,
	Seventh = 7,
	Eighth = 8,
	Ninth = 9,
	Tenth = 10
}
