using System.ComponentModel.DataAnnotations;

namespace ParkingApp.Shared.Employees;

/// <summary>
/// Request to create a new employee under the current owner's company.
/// The caller sends the raw username (e.g. "giannis"); the server prefixes
/// it with the company code to make it globally unique (e.g. "athens.giannis").
/// </summary>
/// 
public class CreateEmployeeRequest
{
	/// <summary>Raw login name, without the company prefix (e.g. "giannis").</summary>
	[Required, MaxLength(50)]
	public string Username { get; set; } = string.Empty;
	/// <summary>The employee's full display name.</summary>
	[Required, MaxLength(100)]
	public string FullName { get; set; } = string.Empty;
	/// <summary>Initial password for the employee account.</summary>
	[Required, MinLength(6)]
	public string Password {  get; set; } = string.Empty;
	/// <summary>The branches this employee is assigned to (at least one).</summary>
	[Required, MinLength(1)]
	public List<Guid> BranchIds { get; set; } = new();
}
