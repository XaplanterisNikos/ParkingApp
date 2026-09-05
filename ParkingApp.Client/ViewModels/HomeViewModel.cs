using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ParkingApp.Client.Consumers.Companies;
using ParkingApp.Client.Services.Auth;
using ParkingApp.Shared.Companies;
using System.Security.Claims;

namespace ParkingApp.Client.ViewModels;

/// <summary>
/// View model for the Home page. Holds the page state and logic, kept out of the
/// component itself. Created manually per visit (not via DI), so its state is fresh
/// each time the page is opened.
/// </summary>
public class HomeViewModel
{
	#region Fields
	private readonly ICompaniesConsumer _companiesConsumer;
	private IAuthService _authService;
	private readonly AuthenticationStateProvider _authStateProvider;
	private readonly NavigationManager _navigationManager;
	#endregion

	#region State
	/// <summary>The current user's login name (from the token).</summary>
	public string? UserName { get; private set; }

	/// <summary>The current user's role (from the token).</summary>
	public string Role { get; private set; } = "(not set)";

	/// <summary>The current user's company, once loaded.</summary>
	public CompanyDto? Company { get; private set; }

	// <summary>True while the company is being fetched.</summary>
	public bool IsLoading { get; private set; } = true;

	/// <summary>Error message if the company could not be loaded.</summary>
	public string? Error { get; private set; }
	#endregion

	#region Constructor
	public HomeViewModel(ICompaniesConsumer companiesConsumer, 
		IAuthService authService, 
		AuthenticationStateProvider authStateProvider, 
		NavigationManager navigationManager)
	{
		_companiesConsumer = companiesConsumer;
		_authService = authService;
		_navigationManager = navigationManager;
		_authStateProvider = authStateProvider;
	}
	#endregion

	#region Public methods

	/// <summary>
	/// Loads the current user's identity (from claims) and their company (from the API).
	/// </summary>
	public async Task InitializeAsync()
	{
		ReadUserFromClaims(await GetUserAsync());
		await LoadCompanyAsync();
	}

	#endregion

	#region Helpers

	/// <summary>Gets the current user's principal from the authentication state.</summary>
	private async Task<ClaimsPrincipal> GetUserAsync()
	{
		var state = await _authStateProvider.GetAuthenticationStateAsync();
		return state.User;
	}

	/// <summary>Reads name and role off the user's claims into state.</summary>
	private void ReadUserFromClaims(ClaimsPrincipal user)
	{
		UserName = user.Identity?.Name;
		Role = GetClaim(user, ClaimTypes.Role);
	}

	/// <summary>Fetches the current user's company from the API and updates state.</summary>
	private async Task LoadCompanyAsync()
	{
		try
		{
			var result = await _companiesConsumer.GetMyCompanyAsync();

			if (result is { Success: true, Value: not null })
			{
				Company = result.Value;
			}
			else
			{
				Error = result?.Message ?? "Could not load your company.";
			}
		}
		catch
		{
			Error = "Could not reach the server.";
		}
		finally
		{
			IsLoading = false;
		}
	}

	/// <summary>Reads a single claim value by type, or a placeholder if missing.</summary>
	private static string GetClaim(ClaimsPrincipal user, string type) =>
		user.FindFirst(type)?.Value ?? "(not set)";

	#endregion
}
