using ParkingApp.Client.Consumers.Auth;
using ParkingApp.Shared.Auth;
using ParkingApp.Shared.Responses;
using System.Net.Http.Json;

namespace ParkingApp.Client.Services.Auth;

/// <summary>
/// Default <see cref="IAuthService"/>: talks to the API's auth endpoint,
/// persists the token, and notifies the authentication state provider.
/// </summary>
public class AuthService : IAuthService
{
	private readonly IAuthConsumer _authConsumer;
	private readonly ITokenStore _tokenStore;
	private readonly JwtAuthenticationStateProvider _authStateProvider;

	public AuthService(IAuthConsumer authConsumer, ITokenStore tokenStore, JwtAuthenticationStateProvider authStateProvider)
	{
		_authConsumer = authConsumer;
		_tokenStore = tokenStore;
		// We registered our provider as the base AuthenticationStateProvider, but we need
		// our concrete type to call NotifyUserAuthentication/NotifyUserLogoutAsync.
		_authStateProvider = authStateProvider;
	}

	/// <inheritdoc />
	public async Task<(bool Success, string? Error)> LoginAsync(LoginRequest request)
	{
		// Delegate the HTTP call to the consumer.
		var result = await _authConsumer.LoginAsync(request);

		// Interpret the envelope: fail if the call failed or the payload is missing.
		if (result is null || !result.Success || result.Value is null)
		{
			var message = result?.Message ?? "Login failed. Please try again.";
			return (false, message);
		}

		// Success: store the token and refresh the auth state so the UI updates.
		await _tokenStore.SetTokenAsync(result.Value.Token);
		_authStateProvider.NotifyUserAuthentication();

		return (true, null);
	}

	/// <inheritdoc />
	public async Task LogoutAsync()
	{
		await _authStateProvider.NotifyUserLogoutAsync();
	}
}
