using ParkingApp.Shared.Auth;
using ParkingApp.Shared.Responses;
using System.Net.Http.Json;

namespace ParkingApp.Client.Consumers.Auth;

/// <summary>
/// Default <see cref="IAuthConsumer"/>: sends the login request to the API and
/// deserializes the standard <see cref="ApiResponse{T}"/> envelope back.
/// </summary>
public class AuthConsumer : IAuthConsumer
{
	private readonly HttpClient _httpClient;

	public AuthConsumer(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	/// <inheritdoc />
	public async Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request)
	{
		var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

		// The API returns an ApiResponse envelope for BOTH success and failure,
		// so we read it either way and let the caller interpret it.
		return await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
	}
}
