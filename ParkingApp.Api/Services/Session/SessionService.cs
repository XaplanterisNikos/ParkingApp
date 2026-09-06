using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ParkingApp.Api.Data;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.Services.Auth;
using ParkingApp.Shared.Session;

namespace ParkingApp.Api.Services.Session;

/// <summary>
/// Default <see cref="ISessionService"/>. Reads the employee's branch assignments
/// and, on selection, re-issues their token via <see cref="ITokenService"/>.
/// </summary>
public class SessionService : ISessionService
{
	private readonly ParkingDbContext _dbContext;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ITokenService _tokenService;

	public SessionService(
		ParkingDbContext dbContext,
		UserManager<ApplicationUser> userManager,
		ITokenService tokenService)
	{
		_dbContext = dbContext;
		_userManager = userManager;
		_tokenService = tokenService;
	}

	/// <inheritdoc />
	public async Task<List<BranchOptionDto>> GetMyBranchesAsync(string userId)
	{
		// EmployeeBranches is tenant-filtered automatically; join to Branches for names.
		return await _dbContext.EmployeeBranches
			.AsNoTracking()
			.Where(assignment => assignment.EmployeeId == userId)
			.Join(
				_dbContext.Branches,
				assignment => assignment.BranchId,
				branch => branch.Id,
				(assignment, branch) => new BranchOptionDto
				{
					Id = branch.Id,
					Name = branch.Name
				})
			.ToListAsync();
	}

	/// <inheritdoc />
	public async Task<(bool Success, string? Error, string? Token)> SelectBranchAsync(string userId, Guid branchId)
	{
		// One query does both jobs: confirm the assignment AND get the branch name.
		// A null result means "no such assignment for this employee" — same meaning the
		// AnyAsync==false check had before, so the security guarantee is unchanged.
		var branchName = await _dbContext.EmployeeBranches
			.Where(assignment => assignment.EmployeeId == userId && assignment.BranchId == branchId)
			.Join(
				_dbContext.Branches,
				assignment => assignment.BranchId,
				branch => branch.Id,
				(assignment, branch) => branch.Name)
			.FirstOrDefaultAsync();

		if (branchName is null)
		{
			return (false, "You are not assigned to this branch.", null);
		}

		// Re-issue from scratch: a JWT is immutable, so embedding the branch means
		// rebuilding the whole token (id, username, company, roles) plus the branch.
		var user = await _userManager.FindByIdAsync(userId);
		if (user is null)
		{
			return (false, "User not found.", null);
		}

		var roles = await _userManager.GetRolesAsync(user);
		var token = _tokenService.CreateToken(user, roles, branchId, branchName);

		return (true, null, token);
	}
}
