namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// Junction entity linking an employee (ApplicationUser) to a branch they work in.
/// A many-to-many relationship: an employee can work in several branches, and a
/// branch has several employees. Tenant-owned so isolation applies automatically.
/// </summary>
public class EmployeeBranch : TenantEntity
{
	/// <summary>The employee (ApplicationUser.Id — a string, as Identity uses string keys).</summary>
	public required string EmployeeId { get; set; }

	/// <summary>The branch the employee is assigned to.</summary>
	public Guid BranchId { get; set; }
}
