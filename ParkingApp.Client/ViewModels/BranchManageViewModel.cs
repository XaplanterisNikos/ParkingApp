using ParkingApp.Client.Consumers.Branches;
using ParkingApp.Client.Consumers.Floors;
using ParkingApp.Client.Consumers.Spots;
using ParkingApp.Shared.Branches;
using ParkingApp.Shared.Floors;
using ParkingApp.Shared.Spots;

namespace ParkingApp.Client.ViewModels;

/// <summary>
/// View model for the branch management page: loads the branch and its floors,
/// and lets the owner add new floors. Created manually per visit.
/// </summary>
public class BranchManageViewModel
{
	#region Fields

	private readonly IBranchesConsumer _branchesConsumer;
	private readonly IFloorsConsumer _floorsConsumer;
	private readonly Guid _branchId;
	private readonly ISpotsConsumer _spotsConsumer;

	#endregion

	#region Constructor

	public BranchManageViewModel(
		IBranchesConsumer branchesConsumer,
		IFloorsConsumer floorsConsumer,
		ISpotsConsumer spotsConsumer,
		Guid branchId)
	{
		_branchesConsumer = branchesConsumer;
		_floorsConsumer = floorsConsumer;
		_spotsConsumer = spotsConsumer;
		_branchId = branchId;
	}

	#endregion

	#region State

	/// <summary>The branch being managed.</summary>
	public BranchDto? Branch { get; private set; }

	/// <summary>The branch's floors.</summary>
	public List<FloorDto> Floors { get; private set; } = new();

	/// <summary>True while the page's data is being loaded.</summary>
	public bool IsLoading { get; private set; } = true;

	/// <summary>Error message if loading failed.</summary>
	public string? LoadError { get; private set; }

	/// <summary>The floor type selected in the "add floor" dropdown.</summary>
	public FloorType? SelectedFloorType { get; set; }

	/// <summary>Floor types not yet used in this branch (available to add).</summary>
	public List<FloorType> AvailableFloorTypes { get; private set; } = new();

	/// <summary>True while a create-floor request is in flight.</summary>
	public bool IsCreatingFloor { get; private set; }

	/// <summary>Error message if creating a floor failed.</summary>
	public string? CreateFloorError { get; private set; }
	/// <summary>The floor whose spot form is currently open; null = none open.</summary>
	public Guid? ActiveFloorId { get; private set; }

	/// <summary>Spots of the currently active floor.</summary>
	public List<SpotDto> ActiveFloorSpots { get; private set; } = new();

	/// <summary>The size chosen for the batch to generate.</summary>
	public SpotSize SelectedSize { get; set; } = SpotSize.Car;

	/// <summary>How many spots to generate.</summary>
	public int SpotCount { get; set; } = 10;

	/// <summary>True while a create-spot request is in flight.</summary>
	public bool IsSavingSpot { get; private set; }

	/// <summary>Error message if creating a spot failed.</summary>
	public string? SpotError { get; private set; }
	/// <summary>Summary of the active floor's spots: how many of each size.</summary>
	public List<(SpotSize Size, int Count)> SpotSummary =>
		ActiveFloorSpots
			.GroupBy(spot => spot.Size)
			.OrderBy(group => group.Key)
			.Select(group => (group.Key, group.Count()))
			.ToList();
	#endregion

	#region Public methods

	/// <summary>Loads the branch and its floors.</summary>
	public async Task InitializeAsync()
	{
		IsLoading = true;
		LoadError = null;

		try
		{
			var branchResult = await _branchesConsumer.GetByIdAsync(_branchId);

			if (branchResult is not { Success: true, Value: not null })
			{
				LoadError = branchResult?.Message ?? "Branch not found.";
				return;
			}

			Branch = branchResult.Value;
			await LoadFloorsAsync();
		}
		catch
		{
			LoadError = "Could not reach the server.";
		}
		finally
		{
			IsLoading = false;
		}
	}

	/// <summary>Creates a floor from <see cref="NewFloorName"/> and reloads the list.</summary>
	public async Task CreateFloorAsync()
	{
		if (SelectedFloorType is null)
		{
			CreateFloorError = "Please select a floor.";
			return;
		}

		IsCreatingFloor = true;
		CreateFloorError = null;

		try
		{
			var request = new CreateFloorRequest { Type = SelectedFloorType.Value };
			var result = await _floorsConsumer.CreateAsync(_branchId, request);

			if (result is { Success: true })
			{
				SelectedFloorType = null;
				await LoadFloorsAsync();
			}
			else
			{
				CreateFloorError = result?.Message ?? "Could not create the floor.";
			}
		}
		catch
		{
			CreateFloorError = "Could not reach the server.";
		}
		finally
		{
			IsCreatingFloor = false;
		}
	}
	/// <summary>
	/// Opens the spot form for a floor (accordion: replaces any previously open one),
	/// clears the form, and loads that floor's spots on demand.
	/// </summary>
	public async Task OpenFloorAsync(Guid floorId)
	{
		ActiveFloorId = floorId;
		ResetSpotForm();

		var result = await _spotsConsumer.GetByFloorAsync(floorId);
		ActiveFloorSpots = result is { Success: true, Value: not null }
			? result.Value
			: new List<SpotDto>();
	}

	/// <summary>Generates a batch of spots on the active floor, then closes the form.</summary>
	public async Task GenerateSpotsAsync()
	{
		if (ActiveFloorId is null) return;

		if (SpotCount < 1)
		{
			SpotError = "Enter a count of at least 1.";
			return;
		}

		IsSavingSpot = true;
		SpotError = null;

		try
		{
			var request = new GenerateSpotsRequest { Size = SelectedSize, Count = SpotCount };
			var result = await _spotsConsumer.GenerateAsync(ActiveFloorId.Value, request);

			if (result is { Success: true })
			{
				CloseSpotForm();   // generate → form closes
			}
			else
			{
				SpotError = result?.Message ?? "Could not generate spots.";
			}
		}
		catch
		{
			SpotError = "Could not reach the server.";
		}
		finally
		{
			IsSavingSpot = false;
		}
	}

	/// <summary>Closes the spot form without saving.</summary>
	public void CancelSpotForm() => CloseSpotForm();
	#endregion

	#region Helpers

	/// <summary>Fetches the branch's floors into state.</summary>
	private async Task LoadFloorsAsync()
	{
		var result = await _floorsConsumer.GetByBranchAsync(_branchId);

		if (result is { Success: true, Value: not null })
		{
			Floors = result.Value;
			RecomputeAvailableFloorTypes();
		}
		else
		{
			LoadError = result?.Message ?? "Could not load floors.";
		}
	}

	/// <summary>Clears the spot form fields (keeps the floor open).</summary>
	private void ResetSpotForm()
	{
		SelectedSize = SpotSize.Car;
		SpotCount = 10;
		SpotError = null;
	}

	/// <summary>Closes the spot form entirely (no floor active).</summary>
	private void CloseSpotForm()
	{
		ActiveFloorId = null;
		ActiveFloorSpots = new List<SpotDto>();
		ResetSpotForm();
	}

	/// <summary>Available types = all defined types minus the ones already used.</summary>
	private void RecomputeAvailableFloorTypes()
	{
		var used = Floors.Select(floor => floor.Type).ToHashSet();

		AvailableFloorTypes = Enum.GetValues<FloorType>()
			.Where(type => !used.Contains(type))
			.ToList();
	}
	#endregion
}

