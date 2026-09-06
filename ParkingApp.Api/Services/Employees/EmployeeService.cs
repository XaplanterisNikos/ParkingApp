using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.MultiTenancy;
using ParkingApp.Shared.Employees;

namespace ParkingApp.Api.Services.Employees;

/// <summary>
/// Default <see cref="IEmployeeService"/>. Employees are Identity users scoped to the
/// current company; usernames are prefixed with the company code to stay globally unique.
/// </summary>
public class EmployeeService : IEmployeeService
{
	private readonly ParkingDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ITenantProvider _tenantProvider;

	public EmployeeService(
		ParkingDbContext dbContext,
		UserManager<ApplicationUser> userManager,
		ITenantProvider tenantProvider)
	{
		_dbContext = dbContext;
		_userManager = userManager;
		_tenantProvider = tenantProvider;
	}

	/// <inheritdoc />
	public async Task<List<EmployeeDto>> GetAllAsync()
	{
		var companyId = _tenantProvider.CurrentCompanyId
			?? throw new InvalidOperationException("No tenant context for listing employees.");

		// The Users table has no global query filter (it's Identity, not a TenantEntity),
		// so we filter by CompanyId explicitly.
		var users = await _dbContext.Users
			.AsNoTracking()
			.Where(user => user.CompanyId == companyId)
			.ToListAsync();

		var employees = new List<EmployeeDto>();

		foreach (var user in users)
		{
			// Only include users that are actually employees.
			if (!await _userManager.IsInRoleAsync(user, DbSeeder.EmployeeRole))
			{
				continue;
			}

			// The employee's branches (id + name for display). EmployeeBranches is
			// tenant-filtered automatically; join to Branches for the names.
			var branches = await _dbContext.EmployeeBranches
				.AsNoTracking()
				.Where(assignment => assignment.EmployeeId == user.Id)
				.Join(
					_dbContext.Branches,
					assignment => assignment.BranchId,
					branch => branch.Id,
					(assignment, branch) => new EmployeeBranchDto
					{
						Id = branch.Id,
						Name = branch.Name
					})
				.ToListAsync();

			employees.Add(new EmployeeDto
			{
				Id = user.Id,
				LoginUsername = user.UserName!,
				FullName = user.FullName,
				Branches = branches
			});
		}

		return employees;
	}

	/// <inheritdoc />
	public async Task<(bool Success, string? Error, EmployeeDto? Employee)> CreateAsync(
		CreateEmployeeRequest request)
	{
		var companyId = _tenantProvider.CurrentCompanyId
			?? throw new InvalidOperationException("No tenant context for creating an employee.");

		var company = await _dbContext.Companies
			.FirstOrDefaultAsync(c => c.Id == companyId);

		if (company is null)
		{
			return (false, "Company not found.", null);
		}

		// Build the globally-unique username: {companyCode}.{rawUsername}, lowercased.
		var loginUsername = $"{company.Code}.{request.Username}".ToLowerInvariant();

		if (await _userManager.FindByNameAsync(loginUsername) is not null)
		{
			return (false, "This username already exists in your company.", null);
		}

		// Keep only branch ids that belong to this tenant (the global filter hides foreign ones).
		var validBranchIds = await _dbContext.Branches
			.Where(branch => request.BranchIds.Contains(branch.Id))
			.Select(branch => branch.Id)
			.ToListAsync();

		if (validBranchIds.Count == 0)
		{
			return (false, "Select at least one valid branch.", null);
		}

		// All four steps must succeed together.
		await using var transaction = await _dbContext.Database.BeginTransactionAsync();

		try
		{
			var user = new ApplicationUser
			{
				UserName = loginUsername,
				FullName = request.FullName,
				CompanyId = companyId,
				EmailConfirmed = true
			};

			var created = await _userManager.CreateAsync(user, request.Password);
			if (!created.Succeeded)
			{
				var errors = string.Join("; ", created.Errors.Select(error => error.Description));
				return (false, errors, null);
			}

			await _userManager.AddToRoleAsync(user, DbSeeder.EmployeeRole);

			foreach (var branchId in validBranchIds)
			{
				_dbContext.EmployeeBranches.Add(new EmployeeBranch
				{
					EmployeeId = user.Id,
					BranchId = branchId,
					CompanyId = companyId
				});
			}

			await _dbContext.SaveChangesAsync();
			await transaction.CommitAsync();

			var branches = await _dbContext.Branches
				.Where(branch => validBranchIds.Contains(branch.Id))
				.Select(branch => new EmployeeBranchDto { Id = branch.Id, Name = branch.Name })
				.ToListAsync();

			return (true, null, new EmployeeDto
			{
				Id = user.Id,
				LoginUsername = user.UserName!,
				FullName = user.FullName,
				Branches = branches
			});
		}
		catch
		{
			await transaction.RollbackAsync();
			return (false, "Could not create the employee.", null);
		}
	}
}