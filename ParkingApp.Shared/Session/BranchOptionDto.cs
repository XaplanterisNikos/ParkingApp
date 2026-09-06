namespace ParkingApp.Shared.Session;

/// <summary>
/// A branch the current employee may work in, offered as a choice when they
/// have more than one assignment. Carries just what the picker needs to show.
/// </summary>
public class BranchOptionDto
{
	/// <summary>The branch id, sent back when the employee picks this branch.</summary>
	public Guid Id { get; set; }

	/// <summary>The branch's display name.</summary>
	public string Name { get; set; } = string.Empty;
}
