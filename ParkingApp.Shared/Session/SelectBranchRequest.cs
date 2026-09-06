namespace ParkingApp.Shared.Session;

/// <summary>
/// Sent when an employee chooses the branch they will work in for this session.
/// </summary>
public class SelectBranchRequest
{
	/// <summary>The id of the chosen branch. Must be one the employee is assigned to.</summary>
	public Guid BranchId { get; set; }
}