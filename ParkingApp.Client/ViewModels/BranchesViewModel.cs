using ParkingApp.Client.Consumers.Branches;
using ParkingApp.Shared.Branches;

namespace ParkingApp.Client.ViewModels;

/// <summary>
/// View model for the Branches page: loads the tenant's branches (data) and lets the
/// owner create a new one (input). Created manually per visit for fresh state.
/// </summary>
public class BranchesViewModel
{
	#region Fields
	private readonly IBranchesConsumer _consumer;
	#endregion

	#region Constructor
	public BranchesViewModel(IBranchesConsumer consumer)
	{
		_consumer = consumer;
	}
	#endregion

	#region State

	/// <summary>The tenant's branches, once loaded.</summary>
	public List<BranchDto> Branches { get; private set; } = new();

	/// <summary>True while the list is being fetched.</summary>
	public bool IsLoading { get; private set; } = true;

	/// <summary>Error message if the list could not be loaded.</summary>
	public string? LoadError { get; private set; }

	/// <summary>The name typed into the "new branch" form.</summary>
	public string NewBranchName { get; set; } = "";

	/// <summary>True while a create request is in flight.</summary>
	public bool IsCreating { get; private set; }

	/// <summary>Error message if creating a branch failed.</summary>
	public string? CreateError { get; private set; }

	#endregion

	#region Public methods

	/// <summary>Loads the tenant's branches from the API.</summary>
	public async Task InitializeAsync()
	{
		IsLoading = true;
		LoadError = null;

		try
		{
			var result = await _consumer.GetAllAsync();

			if (result is { Success: true, Value: not null })
			{
				Branches = result.Value;
			}
			else
			{
				LoadError = result?.Message ?? "Could not load branches.";
			}
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

	/// <summary>
	/// Creates a new branch from <see cref="NewBranchName"/>. On success, clears the
	/// input and reloads the list so the new branch appears immediately.
	/// </summary>
	public async Task CreateAsync()
	{
		// Guard: don't submit an empty name.
		if (string.IsNullOrWhiteSpace(NewBranchName))
		{
			CreateError = "Please enter a branch name.";
			return;
		}

		IsCreating = true;
		CreateError = null;

		try
		{
			var request = new CreateBranchRequest { Name = NewBranchName.Trim() };
			var result = await _consumer.CreateAsync(request);

			if (result is { Success: true })
			{
				NewBranchName = "";            // clear the form
				await InitializeAsync();       // reload so the new branch shows up
			}
			else
			{
				CreateError = result?.Message ?? "Could not create the branch.";
			}
		}
		catch
		{
			CreateError = "Could not reach the server.";
		}
		finally
		{
			IsCreating = false;
		}
	}

	#endregion
}
