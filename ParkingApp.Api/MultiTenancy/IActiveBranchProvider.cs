namespace ParkingApp.Api.MultiTenancy;

/// <summary>
/// Provides the branch the current employee is working in, read from the caller's token
/// (the <c>activeBranchId</c> claim added when the employee selects a branch).
/// Parallel to <see cref="ITenantProvider"/>: tenant = which company, active branch = where I work now.
/// </summary>
public interface IActiveBranchProvider
{
	/// <summary>
	/// The current employee's active branch id, or null when there is none
	/// (anonymous request, owner token, or employee who has not selected a branch yet).
	/// </summary>
	Guid? CurrentBranchId { get; }
}