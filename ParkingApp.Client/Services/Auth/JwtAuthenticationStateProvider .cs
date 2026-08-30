using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ParkingApp.Client.Services.Auth;

/// <summary>
/// Supplies the current authentication state to Blazor by reading the stored JWT,
/// extracting its claims, and exposing them as a <see cref="ClaimsPrincipal"/>.
/// Also notifies the UI when the user logs in or out.
/// </summary>
public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
	private readonly ITokenStore _tokenStore;
	/// <summary>An empty (anonymous) principal, reused when no valid token exists.</summary>
	private static readonly AuthenticationState Anonymous = new(new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity()));

	public JwtAuthenticationStateProvider(ITokenStore tokenStore)
	{
		_tokenStore = tokenStore;
	}

	/// <summary>
	/// Called by Blazor whenever it needs to know who the current user is.
	/// Reads the token, and if present and not expired, builds an authenticated principal.
	/// </summary>
	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		var token = await _tokenStore.GetTokenAsync();

		if (string.IsNullOrWhiteSpace(token))
		{
			return Anonymous;
		}

		var handler = new JwtSecurityTokenHandler();
		// Guard against a malformed token string.
		if (!handler.CanReadToken(token))
		{ return  Anonymous; }

		var jwt = handler.ReadJwtToken(token);

		// If the token has expired, treat the user as anonymous.
		if (jwt.ValidTo < DateTime.UtcNow)
		{
			await _tokenStore.ClearTokenAsync();
			return Anonymous;
		}

		// Build an authenticated identity from the token's claims.
		// The "jwt" authentication type marker is what makes IsAuthenticated true.
		var identity = new ClaimsIdentity(
			jwt.Claims,
			authenticationType: "jwt",
			nameType: "unique_name",
			roleType: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");

		var principal = new ClaimsPrincipal(identity);

		return new AuthenticationState(principal);
	}

	/// <summary>
	/// Call after a successful login: refreshes the state so the UI reflects the new user.
	/// </summary>
	public void NotifyUserAuthentication()
	{
		NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
	}

	/// <summary>
	/// Call on logout: clears the token and refreshes the state to anonymous.
	/// </summary>
	public async Task NotifyUserLogoutAsync()
	{
		await _tokenStore.ClearTokenAsync();
		NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
	}
}
