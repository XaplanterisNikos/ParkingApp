namespace ParkingApp.Shared.Tickets;

/// <summary>
/// The kind of a licence plate. It decides which characters are allowed
/// and how the stored plate is shown back to people.
/// Stored as int; explicit values keep them stable if new kinds are added later.
/// </summary>
public enum PlateKind
{
	/// <summary>
	/// A Greek plate. Only the 14 letters that look the same in Greek and Latin are allowed.
	/// It is stored with Latin letters and shown with Greek letters.
	/// </summary>
	Greek = 1,

	/// <summary>A foreign plate. Latin letters A-Z and digits are allowed.</summary>
	Foreign = 2
}
