using Microsoft.AspNetCore.Identity;
using ParkingApp.Api.Data.Entities;

namespace ParkingApp.Api.Data;

/// <summary>
/// Seeds the database with the baseline data the application needs to run:
/// the two roles and one or more demo owner accounts (with their companies).
/// Registration is seed-only, so without this no one could ever log in.
/// </summary>
public class DbSeeder
{
	/// <summary>The application's fixed set of roles.</summary>
	public const string OwnerRole = "Owner";
	public const string EmployeeRole = "Employee";

	/// <summary>
	/// Runs the full seed. Idempotent: safe to call on every startup — each step
	/// checks for existence before creating, so nothing is duplicated.
	/// </summary>
	/// <param name="services">A scoped service provider.</param>
	public static async Task SeedAsync(IServiceProvider services)
	{
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
		var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
		var dbContext = services.GetRequiredService<ParkingDbContext>();

		// Roles must exist before any user can be assigned to them
		await EnsureRoleAsync(roleManager, OwnerRole);
		await EnsureRoleAsync(roleManager,EmployeeRole);

		// A demo company must exist before its owner (the owner need its CompanyId)
		var company = dbContext.Companies.FirstOrDefault(company => company.Name == "Athens Parking");
		if(company is null)
		{
			company = new Company { Name = "Athens Parking" };
			dbContext.Companies.Add(company);
			await dbContext.SaveChangesAsync(); // saves + assigns the generated Guid Id
		}

		await EnsureOwnerAsync(userManager, company,
			email: "owner@athens.test",
			fullName: "Athens Owner",
			password: "Owner123!");
	}

	/// <summary>Creates a role if it does not already exist.</summary>
	private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
	{
		if (!await roleManager.RoleExistsAsync(role))
		{
			await roleManager.CreateAsync(new IdentityRole(role));
		}
	}

	/// <summary>Creates an owner user (if missing) and assigns the Owner role.</summary>
	private static async Task EnsureOwnerAsync(
		UserManager<ApplicationUser> userManager,
		Company company,
		string email,
		string fullName,
		string password)
	{
		if ( await userManager.FindByEmailAsync(email) is not null)
		{
			return; // already seeded
		}

		var owner = new ApplicationUser
		{ UserName = email, Email = email, FullName = fullName, CompanyId = company.Id , EmailConfirmed = true};

		var result = await userManager.CreateAsync(owner, password);
		if(result.Succeeded)
		{
			await userManager.AddToRoleAsync(owner, OwnerRole);
		}
		else
		{
			var errors = string.Join("; ", result.Errors.Select(error => error.Description));
			throw new InvalidOperationException($"Failed to seed owner '{email}': {errors}");
		}
	}
}
