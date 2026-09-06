using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Data;
using ParkingApp.Api.Services.Employees;
using ParkingApp.Shared.Employees;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// Endpoints for the current company's employees. Owner-only.
/// </summary>
[ApiController]
[Route("api/employees")]
[Authorize(Roles = DbSeeder.OwnerRole)]
public class EmployeesController : ControllerBase
{
	private readonly IEmployeeService _employeeService;

	public EmployeesController(IEmployeeService employeeService)
	{
		_employeeService = employeeService;
	}

	/// <summary>Returns all employees of the current company.</summary>
	[HttpGet]
	public async Task<ActionResult<ApiResponse<List<EmployeeDto>>>> GetAll()
	{
		var employees = await _employeeService.GetAllAsync();
		return Ok(ApiResponse<List<EmployeeDto>>.Ok(employees));
	}

	/// <summary>Creates a new employee for the current company.</summary>
	[HttpPost]
	public async Task<ActionResult<ApiResponse<EmployeeDto>>> Create(
		[FromBody] CreateEmployeeRequest request)
	{
		var (success, error, employee) = await _employeeService.CreateAsync(request);

		if (!success || employee is null)
		{
			return BadRequest(ApiResponse<EmployeeDto>.Fail(error ?? "Could not create the employee."));
		}

		return Ok(ApiResponse<EmployeeDto>.Ok(employee));
	}
}