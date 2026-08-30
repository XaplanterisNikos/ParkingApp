namespace ParkingApp.Client.Services.Auth;

/// <summary>
/// Persists the JWT access token in browser storage and reads it back.
/// A single place that owns "where the token lives".
/// </summary>
public interface ITokenStore
{
	/// <summary>Saves the token to browser storage.</summary>
	Task SetTokenAsync(string token);
	/// <summary>Returns the stored token, or null if none is stored.</summary>
	Task<string?> GetTokenAsync();
	/// <summary>Removes the stored token (used on logout).</summary>
	Task ClearTokenAsync();
}
