namespace ParkingApp.Shared.Companies;

/// <summary>
/// A company as exposed to the client: only the fields the UI needs,
/// never the full entity (which carries navigation data like its users).
/// </summary>
public class CompanyDto
{
	/// <summary>The company's unique identifier.</summary>
	public Guid Id { get; set; }

	/// <summary>The company's display name.</summary>
	public required string Name { get; set; }
}
