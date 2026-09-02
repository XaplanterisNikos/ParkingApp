namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// Base class for every entity that belongs to a single company (tenant).
/// Carrying <see cref="CompanyId"/> here — rather than on each entity — lets tenant
/// isolation be applied uniformly (via a global query filter) and never forgotten.
/// </summary>
public abstract class TenantEntity
{
	/// <summary>Primary key.</summary>
	public Guid Id { get; set; }

	/// <summary>The company (tenant) this entity belongs to.</summary>
	public Guid CompanyId { get; set; }
}
