using Microsoft.AspNetCore.Identity;

namespace ParkingApp.Api.Data.Entities;

/// <summary>
/// Application user. Extends the ASP.NET Core Identity user with the two
/// things our domain needs: which company they belong to, and their display name.
/// </summary>
public class ApplicationUser :IdentityUser
{
	/// <summary>Foreign key to the owning <see cref="Company"/>.</summary>
	public Guid CompanyId { get; set; }

	/// <summary>Navigation to the company this user belongs to.</summary>
	public Company Company { get; set; } = null!;

	/// <summary>The user's full display name.</summary>
	public required string FullName { get; set; }
}
