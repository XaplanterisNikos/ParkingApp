using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Data;
using ParkingApp.Api.Extensions;
using ParkingApp.Api.Services.Session;
using ParkingApp.Shared.Responses;
using ParkingApp.Shared.Session;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// The current employee's work session: listing their branches and selecting the
/// active one. Employee-facing (unlike the owner-only employee management endpoints).
/// </summary>
[ApiController]
[Route("api/session")]
[Authorize(Roles = DbSeeder.EmployeeRole)]
public class SessionController : ControllerBase
{
	private readonly ISessionService _sessionService;

	public SessionController(ISessionService sessionService)
	{
		_sessionService = sessionService;
	}

	/// <summary>Returns the branches the current employee is assigned to.</summary>
	[HttpGet("branches")]
	public async Task<ActionResult<ApiResponse<List<BranchOptionDto>>>> GetMyBranches()
	{
		var userId = User.GetUserId();
		var branches = await _sessionService.GetMyBranchesAsync(userId);
		return Ok(ApiResponse<List<BranchOptionDto>>.Ok(branches));
	}

	/// <summary>
	/// Selects the active work branch and returns a re-issued token carrying it.
	/// </summary>
	[HttpPost("select-branch")]
	public async Task<ActionResult<ApiResponse<SelectBranchResponse>>> SelectBranch(
		[FromBody] SelectBranchRequest request)
	{
		var userId = User.GetUserId();
		var (success, error, token) = await _sessionService.SelectBranchAsync(userId, request.BranchId);

		if (!success || token is null)
		{
			return BadRequest(ApiResponse<SelectBranchResponse>.Fail(error ?? "Could not select the branch."));
		}

		return Ok(ApiResponse<SelectBranchResponse>.Ok(new SelectBranchResponse { Token = token }));
	}
}