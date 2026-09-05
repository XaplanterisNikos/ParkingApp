namespace ParkingApp.Shared.Spots;

/// <summary>
/// The size category of a parking spot, determining which vehicles fit.
/// Stored as int; explicit values keep them stable if new sizes are added later.
/// </summary>
public enum SpotSize
{
	Motorcycle = 1,
	Car = 2,
	LargeCar =3
}
