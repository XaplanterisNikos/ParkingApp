using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ParkingApp.Client.Consumers.Console;
using ParkingApp.Client.Consumers.Tickets;
using ParkingApp.Client.Services.Auth;
using ParkingApp.Client.Services.Session;
using ParkingApp.Shared.Auth;
using ParkingApp.Shared.Console;
using ParkingApp.Shared.Floors;
using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Session;
using ParkingApp.Shared.Spots;
using ParkingApp.Shared.Tickets;

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
	private readonly ITicketsConsumer _ticketsConsumer;
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
	/// <param name="ticketsConsumer">Registers entries and exits, and loads active tickets and charge previews.</param>
	/// <param name="authService">Used for sign-out from the picker.</param>
	/// <param name="authStateProvider">Reads the current token's claims.</param>
	/// <param name="navigationManager">Navigation after sign-out.</param>
	public ConsoleViewModel(
		ISessionService sessionService,
		IConsoleConsumer consoleConsumer,
		ITicketsConsumer ticketsConsumer,
		IAuthService authService,
		AuthenticationStateProvider authStateProvider,
		NavigationManager navigationManager)
	{
		_sessionService = sessionService;
		_authService = authService;
		_consoleConsumer = consoleConsumer;
		_ticketsConsumer = ticketsConsumer;
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

	#region Entry state
	/// <summary>The kind of the plate being typed. Greek by default, as most vehicles are local.</summary>
	public PlateKind SelectedPlateKind { get; private set; } = PlateKind.Greek;

	/// <summary>The plate exactly as the employee typed it (not normalized).</summary>
	public string PlateInput { get; private set; } = string.Empty;

	/// <summary>
	/// The spot the ticket will be issued for. After every map load it is the server's
	/// suggestion, unless the employee picked another spot that is still free.
	/// </summary>
	public Guid? SelectedSpotId { get; private set; }

	/// <summary>True while the entry request is in flight (disables the form).</summary>
	public bool IsSubmittingEntry { get; private set; }

	/// <summary>The server's message when the last entry failed; null otherwise.</summary>
	public string? EntryError { get; private set; }

	/// <summary>The ticket of the last successful entry, shown as a confirmation.</summary>
	public TicketDto? LastTicket { get; private set; }

	/// <summary>The selected spot, looked up in the latest <see cref="SpotMap"/>, or null.</summary>
	public SpotStatusDto? SelectedSpot => FindSpot(SelectedSpotId);

	/// <summary>True when the selected spot is the one the server suggested.</summary>
	public bool IsSelectedSpotSuggested =>
		SelectedSpotId is not null && SelectedSpotId == SpotMap.SuggestedSpotId;

	/// <summary>
	/// The plate as it will be stored and shown (Greek letters for a Greek plate),
	/// or null while the input is empty or invalid. Same rules as the server.
	/// </summary>
	public string? PlatePreview =>
		LicensePlateNormalizer.TryNormalize(PlateInput, SelectedPlateKind, out var plate, out _)
			? LicensePlateNormalizer.ToDisplay(plate, SelectedPlateKind)
			: null;

	/// <summary>Why the typed plate is invalid, or null when it is valid or still empty.</summary>
	public string? PlateError
	{
		get
		{
			// An empty field is not an error yet: the employee has not started typing
			if (string.IsNullOrWhiteSpace(PlateInput))
			{
				return null;
			}

			return LicensePlateNormalizer.TryNormalize(PlateInput, SelectedPlateKind, out _, out var error)
				? null
				: error;
		}
	}

	/// <summary>The overnight packages of the branch's current price list (all sizes).</summary>
	public List<OvernightPackageDto> Packages { get; private set; } = new();

	/// <summary>The package the employee chose; it only counts while it fits the selected spot.</summary>
	public Guid? SelectedPackageId { get; private set; }

	/// <summary>The packages that fit the selected spot (packages are priced per spot size).</summary>
	public List<OvernightPackageDto> AvailablePackages =>
		SelectedSpot is { } spot
			? Packages.Where(package => package.Size == spot.Size).ToList()
			: new List<OvernightPackageDto>();

	/// <summary>
	/// The package that will be sent: the chosen one if it still fits the selected spot, else none.
	/// Changing the spot to another size therefore drops the package by itself.
	/// </summary>
	public Guid? EffectivePackageId =>
		AvailablePackages.Any(package => package.Id == SelectedPackageId) ? SelectedPackageId : null;

	/// <summary>True when everything the entry needs is in place and no request is running.</summary>
	public bool CanSubmitEntry =>
		!IsSubmittingEntry
		&& SelectedVehicleSize is not null
		&& CanTake(SelectedSpot)
		&& PlatePreview is not null;
	#endregion

	#region Exit state
	/// <summary>The active tickets of the branch, newest first (for the search and the map).</summary>
	public List<TicketDto> ActiveTickets { get; private set; } = new();

	/// <summary>What the employee typed in the exit search (a plate or a spot code).</summary>
	public string ExitSearchText { get; private set; } = string.Empty;

	/// <summary>
	/// The active tickets that match the search: by spot code, or by plate typed with Greek or
	/// Latin letters (the plate goes through the same normalizer as at entry). At most 8.
	/// </summary>
	public List<TicketDto> ExitSearchResults
	{
		get
		{
			var query = ExitSearchText.Trim();

			if (query.Length == 0)
			{
				return new List<TicketDto>();
			}

			// Capitals, without the separators people type out of habit
			var compact = new string(query.ToUpperInvariant().Where(character => character is not (' ' or '-' or '.')).ToArray());

			// A plate typed in Greek becomes its stored Latin form; anything else stays as typed
			var plateQuery = LicensePlateNormalizer.TryNormalize(query, PlateKind.Greek, out var latin, out _)
				? latin
				: compact;

			return ActiveTickets
				.Where(ticket => ticket.SpotNumber.Contains(compact, StringComparison.Ordinal)
					|| ticket.LicensePlate.Contains(plateQuery, StringComparison.Ordinal)
					|| ticket.LicensePlate.Contains(compact, StringComparison.Ordinal))
				.Take(8)
				.ToList();
		}
	}

	/// <summary>The ticket whose card is open, or null.</summary>
	public Guid? SelectedTicketId { get; private set; }

	/// <summary>The charge preview of the open ticket, or null while loading or closed.</summary>
	public ExitQuoteDto? ExitQuote { get; private set; }

	/// <summary>True while the charge preview is loading.</summary>
	public bool IsLoadingQuote { get; private set; }

	/// <summary>The chosen way to charge (index in the quote's options; the first is the suggestion).</summary>
	public int SelectedOptionIndex { get; private set; }

	/// <summary>The chosen way to charge, or null when there is no quote.</summary>
	public ExitChargeOptionDto? SelectedOption =>
		ExitQuote is { Options.Count: > 0 } quote
			? quote.Options[Math.Clamp(SelectedOptionIndex, 0, quote.Options.Count - 1)]
			: null;

	/// <summary>True after the first press of "close charge": the next press confirms the exit.</summary>
	public bool IsConfirmingExit { get; private set; }

	/// <summary>True while the exit request is in flight.</summary>
	public bool IsExiting { get; private set; }

	/// <summary>The last exit problem (server message or connection), shown in the exit card.</summary>
	public string? ExitError { get; private set; }

	/// <summary>The ticket of the last successful exit, shown as a confirmation.</summary>
	public TicketDto? LastExit { get; private set; }

	/// <summary>The spot of the open ticket; the map shows it larger.</summary>
	public Guid? FocusedSpotId =>
		SelectedTicketId is { } ticketId
			? ActiveTickets.FirstOrDefault(ticket => ticket.Id == ticketId)?.SpotId ?? ExitQuote?.Ticket.SpotId
			: null;
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
		// Every load catches its own errors and reports them separately, so WhenAll never throws here
		await Task.WhenAll(LoadOccupancyAsync(), LoadSpotMapAsync(), LoadPackagesAsync(), LoadActiveTicketsAsync());
	}

	/// <summary>
	/// Loads (or reloads) the active branch's occupancy. Called through
	/// <see cref="RefreshAsync"/>, so the cards and the spot map are always refreshed together.
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
				OccupancyError = result?.Message ?? "Δεν ήταν δυνατή η φόρτωση της πληρότητας.";
				return;
			}

			Occupancy = result.Value;
		}
		catch
		{
			// Non-2xx (e.g. 403/500) or network failure — same handling as the other view models
			OccupancyError = "Δεν υπάρχει σύνδεση με τον server. Δοκιμάστε ξανά σε λίγο.";
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
				SpotMapError = result?.Message ?? "Δεν ήταν δυνατή η φόρτωση του χάρτη θέσεων.";
				return;
			}

			SpotMap = result.Value;
			SelectedFloorType = ChooseFloorToShow(focusSuggestion);
			SelectedSpotId = ChooseSpotToSelect(focusSuggestion);
		}
		catch
		{
			// Non-2xx (e.g. 400/403/500) or network failure — same handling as occupancy
			SpotMapError = "Δεν υπάρχει σύνδεση με τον server. Δοκιμάστε ξανά σε λίγο.";
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


	/// <summary>Stores the plate as typed. A new attempt clears the previous error.</summary>
	/// <param name="value">The text of the plate field.</param>
	public void SetPlateInput(string? value)
	{
		PlateInput = value ?? string.Empty;
		EntryError = null;
	}

	/// <summary>Switches between a Greek and a foreign plate.</summary>
	/// <param name="kind">The new plate kind.</param>
	public void SetPlateKind(PlateKind kind)
	{
		SelectedPlateKind = kind;
		EntryError = null;
	}

	/// <summary>
	/// Picks another spot on the map. Only a free spot that fits the vehicle is accepted;
	/// the server checks the same rule again on entry.
	/// </summary>
	/// <param name="spotId">The spot the employee clicked.</param>
	public void SelectSpot(Guid spotId)
	{
		if (CanTake(FindSpot(spotId)))
		{
			SelectedSpotId = spotId;
			EntryError = null;
		}
	}

	/// <summary>
	/// Sends the entry to the server. On success the confirmation is kept and the plate is
	/// cleared; on failure the server's message is shown. Either way both panels are reloaded,
	/// because the map has changed (our ticket, or another desk's).
	/// </summary>
	public async Task SubmitEntryAsync()
	{
		if (!CanSubmitEntry)
		{
			return;
		}

		IsSubmittingEntry = true;
		EntryError = null;
		LastTicket = null;

		try
		{
			var result = await _ticketsConsumer.EnterAsync(new CreateEntryRequest
			{
				// The plate is sent as typed: the server normalizes it and its result is final
				LicensePlate = PlateInput,
				PlateKind = SelectedPlateKind,
				VehicleSize = SelectedVehicleSize!.Value,
				SpotId = SelectedSpotId!.Value,
				OvernightPackageId = EffectivePackageId
			});

			if (result is { Success: true, Value: not null })
			{
				LastTicket = result.Value;
				PlateInput = string.Empty;
				// The next customer starts with a normal stay
				SelectedPackageId = null;
			}
			else
			{
				EntryError = result?.Message ?? "Η είσοδος δεν καταχωρίστηκε. Δοκιμάστε ξανά.";
			}
		}
		catch
		{
			// Empty body (e.g. 401/403/500) or network failure
			EntryError = "Δεν υπάρχει σύνδεση με τον server. Δοκιμάστε ξανά σε λίγο.";
		}

		// After a success, move to the next suggestion and its floor. After a failure, keep the
		// employee's spot if it is still free (e.g. the plate was the problem), else the suggestion.
		await Task.WhenAll(
			LoadOccupancyAsync(),
			LoadSpotMapAsync(focusSuggestion: LastTicket is not null),
			LoadActiveTicketsAsync());

		IsSubmittingEntry = false;
	}

	/// <summary>Chooses an overnight package for the entry, or null for a normal stay.</summary>
	/// <param name="packageId">The package, or null.</param>
	public void SelectPackage(Guid? packageId)
	{
		SelectedPackageId = packageId;
		EntryError = null;
	}

	/// <summary>Stores what the employee typed in the exit search; a new search closes the open card.</summary>
	/// <param name="value">The search text.</param>
	public void SetExitSearch(string? value)
	{
		ExitSearchText = value ?? string.Empty;
		CloseTicket();
		LastExit = null;
		ExitError = null;
	}

	/// <summary>
	/// Opens a ticket's card: shows its floor on the map and loads what the exit would cost now.
	/// </summary>
	/// <param name="ticketId">The active ticket.</param>
	public async Task SelectTicketAsync(Guid ticketId)
	{
		SelectedTicketId = ticketId;
		ExitQuote = null;
		ExitError = null;
		LastExit = null;
		IsConfirmingExit = false;
		SelectedOptionIndex = 0;

		// Show the floor of the ticket's spot, so the bay is visible
		var ticket = ActiveTickets.FirstOrDefault(item => item.Id == ticketId);
		var floor = ticket is null
			? null
			: SpotMap.Floors.FirstOrDefault(item => item.Spots.Any(spot => spot.Id == ticket.SpotId));

		if (floor is not null)
		{
			SelectedFloorType = floor.Type;
		}

		await LoadQuoteAsync();
	}

	/// <summary>Opens the card of the ticket in an occupied spot that was clicked on the map.</summary>
	/// <param name="spotId">The occupied spot.</param>
	public async Task SelectOccupiedSpotAsync(Guid spotId)
	{
		var ticket = ActiveTickets.FirstOrDefault(item => item.SpotId == spotId);

		// The list may be older than the map (another desk): reload it once before giving up
		if (ticket is null)
		{
			await LoadActiveTicketsAsync();
			ticket = ActiveTickets.FirstOrDefault(item => item.SpotId == spotId);
		}

		if (ticket is null)
		{
			ExitError = "Δεν βρέθηκε ενεργό εισιτήριο για αυτή τη θέση. Πατήστε ανανέωση.";
			return;
		}

		await SelectTicketAsync(ticket.Id);
	}

	/// <summary>Calculates the open ticket's charge again (the amount grows with time).</summary>
	public async Task RefreshQuoteAsync()
	{
		IsConfirmingExit = false;
		await LoadQuoteAsync();
	}

	/// <summary>Chooses one of the quote's ways to charge.</summary>
	/// <param name="index">The option's index in the quote.</param>
	public void SelectOption(int index)
	{
		SelectedOptionIndex = index;
		// A different amount must be confirmed again
		IsConfirmingExit = false;
	}

	/// <summary>First press of "close charge": asks for a confirmation with the amount.</summary>
	public void RequestExit()
	{
		if (SelectedOption is not null)
		{
			IsConfirmingExit = true;
		}
	}

	/// <summary>Cancels the confirmation; the card stays open.</summary>
	public void CancelExit() => IsConfirmingExit = false;

	/// <summary>Closes the card without an exit.</summary>
	public void CloseTicket()
	{
		SelectedTicketId = null;
		ExitQuote = null;
		IsConfirmingExit = false;
		SelectedOptionIndex = 0;
	}

	/// <summary>
	/// Confirms the exit with the chosen way to charge. The server calculates the final amount
	/// at this moment. Either way the panels reload: the spot is free now, or another desk acted first.
	/// </summary>
	public async Task ConfirmExitAsync()
	{
		if (SelectedTicketId is not { } ticketId || SelectedOption is not { } option)
		{
			return;
		}

		IsExiting = true;
		ExitError = null;

		try
		{
			var result = await _ticketsConsumer.ExitAsync(ticketId, new ExitTicketRequest
			{
				// Only the choice is sent; the amount is the server's
				TariffVersionId = option.TariffVersionId,
				Overstay = option.Overstay
			});

			if (result is { Success: true, Value: not null })
			{
				LastExit = result.Value;
				ExitSearchText = string.Empty;
				CloseTicket();
			}
			else
			{
				ExitError = result?.Message ?? "Η έξοδος δεν καταχωρίστηκε. Δοκιμάστε ξανά.";
			}
		}
		catch
		{
			ExitError = "Δεν υπάρχει σύνδεση με τον server. Δοκιμάστε ξανά σε λίγο.";
		}

		IsConfirmingExit = false;

		await Task.WhenAll(LoadOccupancyAsync(), LoadSpotMapAsync(), LoadActiveTicketsAsync());

		IsExiting = false;
	}

	/// <summary>Abandons the choice and logs out — the escape hatch from the picker.</summary>
	public async Task LogoutAsync()
	{
		await _authService.LogoutAsync();
		_navigationManager.NavigateTo("login");
	}
	#endregion

	#region Helpers
	/// <summary>
	/// True when the spot can take the declared vehicle: it exists, it is free, and it is not
	/// smaller than the vehicle (enum values grow with the size).
	/// </summary>
	/// <param name="spot">The spot to check, or null.</param>
	private bool CanTake(SpotStatusDto? spot) =>
		spot is not null
		&& !spot.IsOccupied
		&& SelectedVehicleSize is { } size
		&& spot.Size >= size;

	/// <summary>Finds a spot by id on any floor of the latest map, or returns null.</summary>
	/// <param name="spotId">The spot id, or null.</param>
	private SpotStatusDto? FindSpot(Guid? spotId) =>
		spotId is null
			? null
			: SpotMap.Floors.SelectMany(floor => floor.Spots).FirstOrDefault(spot => spot.Id == spotId);

	/// <summary>
	/// Decides which spot is selected after <see cref="SpotMap"/> was (re)loaded: the employee's
	/// own choice while it can still take the vehicle (only on a plain reload), otherwise the
	/// server's suggestion (which may be null).
	/// </summary>
	/// <param name="focusSuggestion">True to always take the suggestion (new size, new entry).</param>
	private Guid? ChooseSpotToSelect(bool focusSuggestion)
	{
		if (!focusSuggestion && CanTake(FindSpot(SelectedSpotId)))
		{
			return SelectedSpotId;
		}

		return SpotMap.SuggestedSpotId;
	}

	/// <summary>Loads the overnight packages for the entry form. A failure only hides the package row.</summary>
	private async Task LoadPackagesAsync()
	{
		try
		{
			var result = await _consoleConsumer.GetOvernightPackagesAsync();
			Packages = result is { Success: true, Value: not null } ? result.Value : new List<OvernightPackageDto>();
		}
		catch
		{
			// Packages are optional: without them the entry still works with the normal price list
			Packages = new List<OvernightPackageDto>();
		}
	}

	/// <summary>
	/// Loads the active tickets. If the open card's ticket is no longer active (closed by another
	/// desk), the card closes.
	/// </summary>
	private async Task LoadActiveTicketsAsync()
	{
		try
		{
			var result = await _ticketsConsumer.GetActiveAsync();

			if (result is not { Success: true, Value: not null })
			{
				ExitError = result?.Message ?? "Δεν ήταν δυνατή η φόρτωση των ενεργών εισιτηρίων.";
				return;
			}

			ActiveTickets = result.Value;

			if (SelectedTicketId is { } ticketId && ActiveTickets.All(ticket => ticket.Id != ticketId))
			{
				CloseTicket();
			}
		}
		catch
		{
			ExitError = "Δεν υπάρχει σύνδεση με τον server. Δοκιμάστε ξανά σε λίγο.";
		}
	}

	/// <summary>Loads the charge preview of the open ticket.</summary>
	private async Task LoadQuoteAsync()
	{
		if (SelectedTicketId is not { } ticketId)
		{
			return;
		}

		IsLoadingQuote = true;
		ExitError = null;

		try
		{
			var result = await _ticketsConsumer.GetQuoteAsync(ticketId);

			if (result is { Success: true, Value: not null })
			{
				ExitQuote = result.Value;
				// Keep the employee's choice if it still exists, else the suggestion
				if (SelectedOptionIndex >= ExitQuote.Options.Count)
				{
					SelectedOptionIndex = 0;
				}
			}
			else
			{
				ExitQuote = null;
				ExitError = result?.Message ?? "Δεν ήταν δυνατός ο υπολογισμός της χρέωσης.";
			}
		}
		catch
		{
			ExitQuote = null;
			ExitError = "Δεν υπάρχει σύνδεση με τον server. Δοκιμάστε ξανά σε λίγο.";
		}
		finally
		{
			IsLoadingQuote = false;
		}
	}

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