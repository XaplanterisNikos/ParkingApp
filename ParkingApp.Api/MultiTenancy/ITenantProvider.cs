namespace ParkingApp.Api.MultiTenancy;

/// <summary>
/// Provides the company (tenant) id of the current request, read from the caller's token.
/// </summary>
public interface ITenantProvider
{
	/// <summary>
	/// The current tenant's company id, or null when there is no authenticated
	/// tenant context (e.g. anonymous requests, or during startup/seeding).
	/// </summary>
	Guid? CurrentCompanyId { get; }
}
