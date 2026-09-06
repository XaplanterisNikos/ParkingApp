using ParkingApp.Client.Consumers.Branches;
using ParkingApp.Client.Consumers.Employees;
using ParkingApp.Shared.Branches;
using ParkingApp.Shared.Employees;

namespace ParkingApp.Client.ViewModels;

/// <summary>
/// View model for the Employees page: lists the company's employees and lets the owner
/// create a new one (choosing which branches they work in). Created manually per visit.
/// </summary>
public class EmployeesViewModel
{
	#region Fields

	private readonly IEmployeesConsumer _employeesConsumer;
	private readonly IBranchesConsumer _branchesConsumer;

	#endregion

	#region Constructor

	public EmployeesViewModel(
		IEmployeesConsumer employeesConsumer,
		IBranchesConsumer branchesConsumer)
	{
		_employeesConsumer = employeesConsumer;
		_branchesConsumer = branchesConsumer;
	}

	#endregion

	#region State

	/// <summary>The company's employees.</summary>
	public List<EmployeeDto> Employees { get; private set; } = new();

	/// <summary>All branches of the company (options for the "add employee" form).</summary>
	public List<BranchDto> Branches { get; private set; } = new();

	/// <summary>True while the page's data is being loaded.</summary>
	public bool IsLoading { get; private set; } = true;

	/// <summary>Error message if loading failed.</summary>
	public string? LoadError { get; private set; }

	// --- create form ---

	/// <summary>Raw username typed in the form (without the company prefix).</summary>
	public string NewUsername { get; set; } = "";

	/// <summary>Full name typed in the form.</summary>
	public string NewFullName { get; set; } = "";

	/// <summary>Password typed in the form.</summary>
	public string NewPassword { get; set; } = "";

	/// <summary>Ids of the branches ticked for the new employee.</summary>
	public HashSet<Guid> SelectedBranchIds { get; } = new();

	/// <summary>True while a create request is in flight.</summary>
	public bool IsCreating { get; private set; }

	/// <summary>Error message if creating failed.</summary>
	public string? CreateError { get; private set; }

	#endregion

	#region Public methods

	/// <summary>Loads the employees and the branch options.</summary>
	public async Task InitializeAsync()
	{
		IsLoading = true;
		LoadError = null;

		try
		{
			var branchesResult = await _branchesConsumer.GetAllAsync();
			if (branchesResult is { Success: true, Value: not null })
			{
				Branches = branchesResult.Value;
			}

			await LoadEmployeesAsync();
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

	/// <summary>Toggles a branch selection for the new employee.</summary>
	public void ToggleBranch(Guid branchId)
	{
		if (!SelectedBranchIds.Add(branchId))
		{
			SelectedBranchIds.Remove(branchId);
		}
	}

	/// <summary>Creates the employee from the form, then reloads the list.</summary>
	public async Task CreateAsync()
	{
		if (string.IsNullOrWhiteSpace(NewUsername) ||
			string.IsNullOrWhiteSpace(NewFullName) ||
			string.IsNullOrWhiteSpace(NewPassword))
		{
			CreateError = "Please fill in username, full name and password.";
			return;
		}

		if (SelectedBranchIds.Count == 0)
		{
			CreateError = "Select at least one branch.";
			return;
		}

		IsCreating = true;
		CreateError = null;

		try
		{
			var request = new CreateEmployeeRequest
			{
				Username = NewUsername.Trim(),
				FullName = NewFullName.Trim(),
				Password = NewPassword,
				BranchIds = SelectedBranchIds.ToList()
			};

			var result = await _employeesConsumer.CreateAsync(request);

			if (result is { Success: true })
			{
				ResetForm();
				await LoadEmployeesAsync();
			}
			else
			{
				CreateError = result?.Message ?? "Could not create the employee.";
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

	#region Helpers

	/// <summary>Fetches the employees into state.</summary>
	private async Task LoadEmployeesAsync()
	{
		var result = await _employeesConsumer.GetAllAsync();

		if (result is { Success: true, Value: not null })
		{
			Employees = result.Value;
		}
		else
		{
			LoadError = result?.Message ?? "Could not load employees.";
		}
	}

	/// <summary>Clears the create form.</summary>
	private void ResetForm()
	{
		NewUsername = "";
		NewFullName = "";
		NewPassword = "";
		SelectedBranchIds.Clear();
		CreateError = null;
	}

	#endregion
}