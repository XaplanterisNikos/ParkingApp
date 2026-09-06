namespace ParkingApp.Shared.Employees;

/// <summary>An employee of the current company, with the branches they belong to.</summary>
public class EmployeeDto
{
	/// <summary>Identity user id (string).</summary>
	public string Id { get; set; } = string.Empty;
	/// <summary>Full stored login username, including the company prefix (e.g. "athens.giannis").</summary>
	public string LoginUsername { get; set; } = string.Empty;
	/// <summary>The employee's full display name.</summary>
	public string FullName { get; set; } = string.Empty;
	/// <summary>The branches this employee is assigned to.</summary>
	public List<EmployeeBranchDto> Branches { get; set; } = new();
}
