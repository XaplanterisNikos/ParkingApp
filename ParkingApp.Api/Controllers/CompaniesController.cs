using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Extensions;
using ParkingApp.Api.Services.Companies;
using ParkingApp.Shared.Companies;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// Endpoints for the caller's own company (tenant).
/// </summary>
[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController : ControllerBase
{
	private readonly ICompanyService _companyService;

	public CompaniesController(ICompanyService companyService)
	{
		_companyService = companyService;
	}

	/// <summary>
	/// Returns the company that the current user belongs to.
	/// The company id is taken from the caller's token, never from the request,
	/// so a user can only ever retrieve their own company.
	/// </summary>
	/// <returns>200 with the company, or 404 if it no longer exists.</returns>
	[HttpGet("about")]
	public async Task<ActionResult<ApiResponse<CompanyDto>>> GetMyCompany()
	{
		var companyId = User.GetComapnyId();
		var company = await _companyService.GetByIdAsync(companyId);

		if(company is null)
		{
			return NotFound(ApiResponse<CompanyDto>.Fail("Company not found"));
		}

		return Ok(ApiResponse <CompanyDto>.Ok(company));
	}
}
