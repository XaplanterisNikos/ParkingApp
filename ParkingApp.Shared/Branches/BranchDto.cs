namespace ParkingApp.Shared.Branches;

/// <summary>
/// A branch as exposed to the client. No CompanyId here — the client never needs it;
/// tenant scoping happens entirely server-side.
/// </summary>
public class BranchDto
{
	/// <summary>The branch's unique identifier.</summary>
	public Guid Id { get; set; }
	/// <summary>The branch's display name.</summary>
	public required string Name { get; set; }
}
