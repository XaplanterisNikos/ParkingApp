using ParkingApp.Shared.Employees;

namespace ParkingApp.Api.Services.Employees;

/// <summary>
/// Operations on employees within the current owner's company.
/// </summary>
public interface IEmployeeService
{
	/// <summary>Returns all employees of the current company, with their branches.</summary>
	Task<List<EmployeeDto>> GetAllAsync();

	/// <summary>
	/// Creates an employee account (Identity user + Employee role + branch assignments).
	/// </summary>
	/// <returns>
	/// Success flag, an error message when it failed, and the created employee on success.
	/// </returns>
	Task<(bool Success, string? Error, EmployeeDto? Employee)> CreateAsync(CreateEmployeeRequest request);
}