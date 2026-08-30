namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// A tenant company. Owns one or more parking branches and all the users
/// (owner and employees) that operate under it. Every user belongs to
/// exactly one company — this is the root of the multi-tenant model.
/// </summary>
public class Company
{
	/// <summary>Primary key (non-sequential GUID, safe to expose in tokens/URLs).</summary>
	public Guid Id { get; set; }

	/// <summary>The company's display name (e.g. "Acme Parking Ltd").</summary>
	public required string Name { get; set; }

	/// <summary>The users that belong to this company.</summary>
	public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
