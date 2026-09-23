using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ParkingApp.Client.Consumers.Console;
using ParkingApp.Client.Services.Auth;
using ParkingApp.Client.Services.Session;
using ParkingApp.Shared.Auth;
using ParkingApp.Shared.Console;
using ParkingApp.Shared.Floors;
using ParkingApp.Shared.Session;
using ParkingApp.Shared.Spots;

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
	private readonly IConsoleConsumer _consoleConsumer;
	private readonly IAuthService _authService;
	private readonly AuthenticationStateProvider _authStateProvider;
	private readonly NavigationManager _navigationManager;
	#endregion

	#region Constructor
	/// <summary>
	/// Creates the view model. Instantiated manually by the page (not via DI).
	/// </summary>
	/// <param name="sessionService">Loads the employee's branches and selects the active one.</param>
	/// <param name="consoleConsumer">Loads the active branch's occupancy.</param>
	/// <param name="authService">Used for sign-out from the picker.</param>
	/// <param name="authStateProvider">Reads the current token's claims.</param>
	/// <param name="navigationManager">Navigation after sign-out.</param>
	public ConsoleViewModel(
		ISessionService sessionService,
		IConsoleConsumer consoleConsumer,
		IAuthService authService,
		AuthenticationStateProvider authStateProvider,
		NavigationManager navigationManager)
	{
		_sessionService = sessionService;
		_authService = authService;
		_consoleConsumer = consoleConsumer;
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

	/// <summary>Occupancy per spot size for the active branch (empty until loaded).</summary>
	public List<SpotSizeOccupancyDto> Occupancy { get; private set; } = new();

	/// <summary>True while occupancy is being (re)loaded — disables the refresh button.</summary>
	public bool IsLoadingOccupancy { get; private set; }

	/// <summary>
	/// Error message if loading occupancy failed. Kept separate from <see cref="Error"/>
	/// so a failed occupancy load doesn't take down the whole console.
	/// </summary>
	public string? OccupancyError { get; private set; }

	/// <summary>All spots in the branch (derived from <see cref="Occupancy"/>).</summary>
	public int TotalSpots => Occupancy.Sum(item => item.Total);

	/// <summary>Spots with an active ticket (derived from <see cref="Occupancy"/>).</summary>
	public int OccupiedSpots => Occupancy.Sum(item => item.Occupied);

	/// <summary>Spots available right now (derived — never stored).</summary>
	public int FreeSpots => TotalSpots - OccupiedSpots;

	/// <summary>The active branch's spot map: floors, spots and live occupancy (empty until loaded).</summary>
	public SpotMapDto SpotMap { get; private set; } = new();

	/// <summary>True while the spot map is being (re)loaded.</summary>
	public bool IsLoadingSpotMap { get; private set; }

	/// <summary>
	/// Error message if loading the spot map failed. Separate from <see cref="OccupancyError"/>
	/// so each panel fails on its own without hiding the other.
	/// </summary>
	public string? SpotMapError { get; private set; }

	/// <summary>
	/// The vehicle size the employee declared, or null before choosing one.
	/// Sent with every map request so the server can suggest a spot of exactly that size.
	/// </summary>
	public SpotSize? SelectedVehicleSize { get; private set; }

	/// <summary>
	/// The floor currently shown, stored as its type (the floor's key within a branch),
	/// or null when the branch has no floors.
	/// </summary>
	public FloorType? SelectedFloorType { get; private set; }

	/// <summary>
	/// The floor currently shown, looked up in the latest <see cref="SpotMap"/> (derived — never stored),
	/// or null when nothing is selected.
	/// </summary>
	public FloorMapDto? SelectedFloor =>
		SpotMap.Floors.FirstOrDefault(floor => floor.Type == SelectedFloorType);
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
			// Ready path #1: the token already carries the branch — load cards and map together
			await RefreshAsync();
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

		// Ready path #2: the new token (with the branch claim) is already stored,
		// so both requests pass the ActiveBranch policy.
		await RefreshAsync();
	}

	/// <summary>
	/// Reloads everything the console shows about the branch — occupancy cards and spot map —
	/// in parallel. Used by the gate and by the refresh button: the console is a snapshot and
	/// does not update live, and refreshing only one panel would let the two disagree.
	/// </summary>
	public async Task RefreshAsync()
	{
		// Both loads catch their own errors and report them separately, so WhenAll never throws here
		await Task.WhenAll(LoadOccupancyAsync(), LoadSpotMapAsync());
	}

	/// <summary>
	/// Loads (or reloads) the active branch's occupancy. Also used by the refresh
	/// button, since the console is a snapshot and does not update live.
	/// </summary>
	public async Task LoadOccupancyAsync()
	{
		IsLoadingOccupancy = true;
		OccupancyError = null;

		try
		{
			var result = await _consoleConsumer.GetOccupancyAsync();

			if (result is not { Success: true, Value: not null })
			{
				OccupancyError = result?.Message ?? "Could not load occupancy.";
				return;
			}

			Occupancy = result.Value;
		}
		catch
		{
			// Non-2xx (e.g. 403/500) or network failure — same handling as the other view models
			OccupancyError = "Could not reach the server.";
		}
		finally
		{
			// Runs on every exit path: success, early return, or exception
			IsLoadingOccupancy = false;
		}
	}

	/// <summary>
	/// Loads (or reloads) the spot map for the declared vehicle size, then decides which floor to show.
	/// </summary>
	/// <param name="focusSuggestion">
	/// True to jump to the suggested spot's floor (after a size change); false to keep the
	/// floor the employee is looking at (plain refresh).
	/// </param>
	public async Task LoadSpotMapAsync(bool focusSuggestion = false)
	{
		IsLoadingSpotMap = true;
		SpotMapError = null;

		try
		{
			var result = await _consoleConsumer.GetSpotMapAsync(SelectedVehicleSize);

			if (result is not { Success: true, Value: not null })
			{
				// The previous map stays in memory, but the page shows the error instead of it
				SpotMapError = result?.Message ?? "Could not load the spot map.";
				return;
			}

			SpotMap = result.Value;
			SelectedFloorType = ChooseFloorToShow(focusSuggestion);
		}
		catch
		{
			// Non-2xx (e.g. 400/403/500) or network failure — same handling as occupancy
			SpotMapError = "Could not reach the server.";
		}
		finally
		{
			// Runs on every exit path: success, early return, or exception
			IsLoadingSpotMap = false;
		}
	}

	/// <summary>
	/// Declares the vehicle size (or clears it with null) and reloads the map, so the suggestion
	/// always matches both the current size and the current occupancy.
	/// </summary>
	public async Task SelectVehicleSizeAsync(SpotSize? size)
	{
		SelectedVehicleSize = size;
		// A new size means a new suggestion — show its floor
		await LoadSpotMapAsync(focusSuggestion: true);
	}

	/// <summary>Shows another floor. No request: the map already holds every floor of the branch.</summary>
	public void SelectFloor(FloorType floorType)
	{
		SelectedFloorType = floorType;
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
		// Claim name from the shared constants — the same one TokenService writes
		return state.User.FindFirst(AppClaimTypes.ActiveBranchId) is not null;
	}

	/// <summary>
	/// Decides which floor to show after <see cref="SpotMap"/> was (re)loaded, in priority order:
	/// the suggested spot's floor (only when <paramref name="focusSuggestion"/> is true),
	/// the floor already shown (if it still exists), the ground floor, the first floor.
	/// Returns null when the branch has no floors.
	/// </summary>
	/// <param name="focusSuggestion">True to prefer the suggested spot's floor.</param>
	private FloorType? ChooseFloorToShow(bool focusSuggestion)
	{
		var floors = SpotMap.Floors;

		// 1. The suggestion's floor — the employee sees the proposed box without searching
		if (focusSuggestion && SpotMap.SuggestedSpotId is { } suggestedId)
		{
			var suggestedFloor = floors.FirstOrDefault(floor =>
				floor.Spots.Any(spot => spot.Id == suggestedId));

			if (suggestedFloor is not null)
			{
				return suggestedFloor.Type;
			}
		}

		// 2. Keep the floor already shown, if it still exists after the reload
		if (floors.Any(floor => floor.Type == SelectedFloorType))
		{
			return SelectedFloorType;
		}

		// 3. The ground floor, if the branch has one
		if (floors.Any(floor => floor.Type == FloorType.Ground))
		{
			return FloorType.Ground;
		}

		// 4. The first floor in physical order; ?.Type yields null (not Ground) when there are none
		return floors.FirstOrDefault()?.Type;
	}
	#endregion
}