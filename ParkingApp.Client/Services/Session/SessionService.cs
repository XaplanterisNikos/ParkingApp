using ParkingApp.Client.Consumers.Session;
using ParkingApp.Client.Services.Auth;
using ParkingApp.Shared.Session;

namespace ParkingApp.Client.Services.Session;

/// <summary>
/// Default <see cref="ISessionService"/>. Mirrors AuthService: after a successful
/// branch selection it persists the re-issued token and refreshes the auth state.
/// </summary>
public class SessionService : ISessionService
{
	private readonly ISessionConsumer _sessionConsumer;
	private readonly ITokenStore _tokenStore;
	private readonly JwtAuthenticationStateProvider _authStateProvider;

	public SessionService(
		ISessionConsumer sessionConsumer,
		ITokenStore tokenStore,
		JwtAuthenticationStateProvider authStateProvider)
	{
		_sessionConsumer = sessionConsumer;
		_tokenStore = tokenStore;
		_authStateProvider = authStateProvider;
	}

	/// <inheritdoc />
	public async Task<(bool Success, string? Error, List<BranchOptionDto> Branches)> GetMyBranchesAsync()
	{
		var result = await _sessionConsumer.GetMyBranchesAsync();

		if (result is null || !result.Success || result.Value is null)
		{
			var message = result?.Message ?? "Could not load your branches.";
			return (false, message, new List<BranchOptionDto>());
		}

		return (true, null, result.Value);
	}

	/// <inheritdoc />
	public async Task<(bool Success, string? Error)> SelectBranchAsync(Guid branchId)
	{
		var result = await _sessionConsumer.SelectBranchAsync(
			new SelectBranchRequest { BranchId = branchId });

		if (result is null || !result.Success || result.Value is null)
		{
			var message = result?.Message ?? "Could not select the branch.";
			return (false, message);
		}

		// Same two steps as login: store the re-issued token, then refresh the
		// auth state so the new activeBranchId claim is what later requests carry.
		await _tokenStore.SetTokenAsync(result.Value.Token);
		_authStateProvider.NotifyUserAuthentication();

		return (true, null);
	}
}