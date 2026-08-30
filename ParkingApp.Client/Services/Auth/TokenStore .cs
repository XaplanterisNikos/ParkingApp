using Blazored.LocalStorage;

namespace ParkingApp.Client.Services.Auth;

/// <summary>
/// <see cref="ITokenStore"/> implementation backed by the browser's localStorage
/// via Blazored.LocalStorage.
/// </summary>
public class TokenStore : ITokenStore
{

	/// <summary>The localStorage key under which the token is stored.</summary>
	private const string TokenKey = "authToken";

	private readonly ILocalStorageService _localStorage;

	public TokenStore(ILocalStorageService localStorage)
	{
		_localStorage = localStorage;
	}

	/// <inheritdoc />
	public Task ClearTokenAsync() =>
		_localStorage.RemoveItemAsync(TokenKey).AsTask();

	/// <inheritdoc />
	public async Task<string?> GetTokenAsync() =>
		await _localStorage.GetItemAsStringAsync(TokenKey);

	/// <inheritdoc />
	public Task SetTokenAsync(string token) =>
		_localStorage.SetItemAsStringAsync(TokenKey, token).AsTask();
}
