using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Services.Branches;
using ParkingApp.Shared.Branches;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// Endpoints for the current tenant's parking branches.
/// </summary>
[ApiController]
[Route("api/branches")]
[Authorize]
public class BranchesController : ControllerBase
{
	private readonly IBranchService _branchService;

	public BranchesController(IBranchService branchService)
	{
		_branchService = branchService;
	}

	/// <summary>Returns all branches belonging to the current tenant.</summary>
	[HttpGet]
	public async Task<ActionResult<ApiResponse<List<BranchDto>>>> GetAll()
	{
		var branches = await _branchService.GetAllAsync();
		return Ok(ApiResponse<List<BranchDto>>.Ok(branches));
	}

	/// <summary>Returns a single branch by id.</summary>
	[HttpGet("{id:guid}")]
	public async Task<ActionResult<ApiResponse<BranchDto>>> GetById([FromRoute] Guid id)
	{
		var branch = await _branchService.GetByIdAsync(id);
		if(branch is null)
		{
			return NotFound(ApiResponse<BranchDto>.Fail("Branch not found."));
		}

		return Ok(ApiResponse<BranchDto>.Ok(branch));
	}

	/// <summary>Creates a new branch for the current tenant.</summary>
	/// <param name="request">The branch to create.</param>
	[HttpPost]
	public async Task<ActionResult<ApiResponse<BranchDto>>> Create([FromBody] CreateBranchRequest reguest)
	{
		var branch = await _branchService.CreateAsync(reguest);
		return Ok(ApiResponse<BranchDto>.Ok(branch));
	}
}
