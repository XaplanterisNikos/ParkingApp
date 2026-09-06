using ParkingApp.Shared.Employees;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Employees;

/// <summary>
/// Talks to the API's employee endpoints (current company's employees).
/// </summary>
public interface IEmployeesConsumer
{
	/// <summary>Gets all employees of the current company.</summary>
	Task<ApiResponse<List<EmployeeDto>>?> GetAllAsync();

	/// <summary>Creates a new employee.</summary>
	Task<ApiResponse<EmployeeDto>?> CreateAsync(CreateEmployeeRequest request);
}