using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ParkingApp.Client.Services.Auth;
using ParkingApp.Client.Services.Session;
using ParkingApp.Shared.Session;

namespace ParkingApp.Client.ViewModels;

/// <summary>
/// View model for the employee console. Acts as a gate: if the token already carries
/// an active branch the console is ready; otherwise it loads the employee's branches
/// and either auto-selects the only one or opens the picker.
/// </summary>
public class ConsoleViewModel
{
	#region Fields
	private readonly ISessionService _sessionService;
	private readonly IAuthService _authService;
	private readonly AuthenticationStateProvider _authStateProvider;
	private readonly NavigationManager _navigationManager;

	/// <summary>The claim type carrying the chosen work branch id.</summary>
	private const string ActiveBranchIdClaim = "activeBranchId";
	#endregion

	#region Constructor
	public ConsoleViewModel(
		ISessionService sessionService,
		IAuthService authService,
		AuthenticationStateProvider authStateProvider,
		NavigationManager navigationManager)
	{
		_sessionService = sessionService;
		_authService = authService;
		_authStateProvider = authStateProvider;
		_navigationManager = navigationManager;
	}
	#endregion

	#region State
	/// <summary>True while the gate is deciding (checking the claim / loading branches).</summary>
	public bool IsLoading { get; private set; } = true;

	/// <summary>When true, the branch picker modal should be shown.</summary>
	public bool ShowBranchPicker { get; private set; }

	/// <summary>The branches offered in the picker (only when the employee has several).</summary>
	public List<BranchOptionDto> Branches { get; private set; } = new();

	/// <summary>True while a selection request is in flight (disables the picker).</summary>
	public bool IsSelecting { get; private set; }

	/// <summary>Error message if loading branches or selecting one failed.</summary>
	public string? Error { get; private set; }
	#endregion

	#region Public methods
	/// <summary>
	/// The console gate. If the token already carries an active branch, the console is
	/// ready immediately (survives refresh). Otherwise loads the employee's branches:
	/// one is auto-selected, several open the picker.
	/// </summary>
	public async Task InitializeAsync()
	{
		IsLoading = true;
		Error = null;

		// Already chose a branch — the claim lives in the token, so this survives F5.
		if (await HasActiveBranchAsync())
		{
			IsLoading = false;
			return;
		}

		var (success, error, branches) = await _sessionService.GetMyBranchesAsync();

		if (!success)
		{
			Error = error;
			IsLoading = false;
			return;
		}

		// One branch → no choice to make, select it automatically.
		if (branches.Count == 1)
		{
			await SelectBranchAsync(branches[0].Id);
			IsLoading = false;
			return;
		}

		// Several branches → let the employee pick.
		Branches = branches;
		ShowBranchPicker = true;
		IsLoading = false;
	}

	/// <summary>Selects the active branch (the service re-issues and stores the token).</summary>
	public async Task SelectBranchAsync(Guid branchId)
	{
		IsSelecting = true;
		Error = null;

		var (success, error) = await _sessionService.SelectBranchAsync(branchId);

		if (!success)
		{
			Error = error;
			IsSelecting = false;
			return;
		}

		// The token now carries the branch; close the picker. The notify inside the
		// service makes the nav pick up the branch name on its own.
		ShowBranchPicker = false;
		IsSelecting = false;
	}

	/// <summary>Abandons the choice and logs out — the escape hatch from the picker.</summary>
	public async Task LogoutAsync()
	{
		await _authService.LogoutAsync();
		_navigationManager.NavigateTo("login");
	}
	#endregion

	#region Helpers
	/// <summary>Reads the current token's claims to see if a branch is already active.</summary>
	private async Task<bool> HasActiveBranchAsync()
	{
		var state = await _authStateProvider.GetAuthenticationStateAsync();
		return state.User.FindFirst(ActiveBranchIdClaim) is not null;
	}
	#endregion
}