namespace ParkingApp.Shared.Employees;

/// <summary>A branch an employee is assigned to (id + name for display).</summary>
public class EmployeeBranchDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
}
