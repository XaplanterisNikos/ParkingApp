using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ParkingApp.Client.Consumers.Companies;
using ParkingApp.Client.Services.Auth;

namespace ParkingApp.Client.ViewModels;

/// <summary>
/// View model for the main layout's nav bar: exposes the current user's name and
/// company (for display), and handles logout.
/// </summary>
public class MainLayoutViewModel
{
	#region Fields
	private readonly ICompaniesConsumer _companiesConsumer;
	private readonly IAuthService _authService;
	private readonly AuthenticationStateProvider _authStateProvider;
	private readonly NavigationManager _navigationManager;
	#endregion

	#region Constructor
	public MainLayoutViewModel(
		ICompaniesConsumer companiesConsumer, 
		IAuthService authService, 
		AuthenticationStateProvider authStateProvider, 
		NavigationManager navigationManager)
	{
		_companiesConsumer = companiesConsumer;
		_authService = authService;
		_authStateProvider = authStateProvider;
		_navigationManager = navigationManager;
	}
	#endregion

	#region State
	/// <summary>The current user's login name (from the token).</summary>
	public string? UserName { get; private set; }
	/// <summary>The current user's company name (from the API).</summary>
	public string? CompanyName { get; private set; }
	#endregion

	#region Public methods
	/// <summary>Loads the user's name (from claims) and company (from the API).</summary>
	public async Task InitializeAsync()
	{
		var state = await _authStateProvider.GetAuthenticationStateAsync();
		var user = state.User;

		// Not authenticated (e.g. on the login page) — nothing to load.
		if (user.Identity?.IsAuthenticated != true) return;

		UserName = user.Identity.Name;

		var result = await _companiesConsumer.GetMyCompanyAsync();
		if (result is { Success: true, Value: not null })
		{
			CompanyName = result.Value.Name;
		}
	}

	public async Task LogoutAsync()
	{
		await _authService.LogoutAsync();
		_navigationManager.NavigateTo("login");
	}

	#endregion
}
