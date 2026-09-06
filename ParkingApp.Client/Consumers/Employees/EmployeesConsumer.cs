using System.Net.Http.Json;
using ParkingApp.Shared.Employees;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Client.Consumers.Employees;

/// <summary>
/// Default <see cref="IEmployeesConsumer"/>: calls the protected employee endpoints.
/// The JWT is attached automatically by AuthTokenHandler.
/// </summary>
public class EmployeesConsumer : IEmployeesConsumer
{
	private readonly HttpClient _httpClient;

	public EmployeesConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<List<EmployeeDto>>?> GetAllAsync()
	{
		return await _httpClient
			.GetFromJsonAsync<ApiResponse<List<EmployeeDto>>>("api/employees");
	}

	/// <inheritdoc />
	public async Task<ApiResponse<EmployeeDto>?> CreateAsync(CreateEmployeeRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/employees", request);
		return await response.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
	}
}