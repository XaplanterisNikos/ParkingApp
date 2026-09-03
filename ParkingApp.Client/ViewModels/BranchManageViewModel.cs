using ParkingApp.Client.Consumers.Branches;
using ParkingApp.Client.Consumers.Floors;
using ParkingApp.Shared.Branches;
using ParkingApp.Shared.Floors;

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

	#endregion

	#region Constructor

	public BranchManageViewModel(
		IBranchesConsumer branchesConsumer,
		IFloorsConsumer floorsConsumer,
		Guid branchId)
	{
		_branchesConsumer = branchesConsumer;
		_floorsConsumer = floorsConsumer;
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

	/// <summary>The name typed into the "new floor" form.</summary>
	public string NewFloorName { get; set; } = "";

	/// <summary>True while a create-floor request is in flight.</summary>
	public bool IsCreatingFloor { get; private set; }

	/// <summary>Error message if creating a floor failed.</summary>
	public string? CreateFloorError { get; private set; }

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
		if (string.IsNullOrWhiteSpace(NewFloorName))
		{
			CreateFloorError = "Please enter a floor name.";
			return;
		}

		IsCreatingFloor = true;
		CreateFloorError = null;

		try
		{
			var request = new CreateFloorRequest { Name = NewFloorName.Trim() };
			var result = await _floorsConsumer.CreateAsync(_branchId, request);

			if (result is { Success: true })
			{
				NewFloorName = "";
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

	#endregion

	#region Helpers

	/// <summary>Fetches the branch's floors into state.</summary>
	private async Task LoadFloorsAsync()
	{
		var result = await _floorsConsumer.GetByBranchAsync(_branchId);

		if (result is { Success: true, Value: not null })
		{
			Floors = result.Value;
		}
		else
		{
			LoadError = result?.Message ?? "Could not load floors.";
		}
	}

	#endregion
}

