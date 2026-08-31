using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParkingApp.Api.Data.Entities;
using ParkingApp.Api.Services.Auth;
using ParkingApp.Shared.Auth;
using ParkingApp.Shared.Responses;

namespace ParkingApp.Api.Controllers;

/// <summary>
/// Handles authentication. Currently exposes login, which exchanges valid
/// credentials for a signed JWT access token.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
	private readonly ITokenService _tokenService;

	public AuthController(
		UserManager<ApplicationUser> userManager,
		SignInManager<ApplicationUser> signManager,
		ITokenService tokenService )
	{
		_userManager = userManager;
		_signInManager = signManager;
		_tokenService = tokenService;
	}


	/// <summary>
	/// Authenticates a user and returns a JWT on success.
	/// </summary>
	/// <param name="request">The login credentials.</param>
	/// <returns>200 with a <see cref="LoginResponse"/>, or 401 if credentials are invalid.</returns>
	[HttpPost("login")]
	public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
	{
		// Look the user up by their username (which is the email for owners).
		var user = await _userManager.FindByNameAsync(request.UserName);

		// Same generic 401 whether the user is missing OR the password is wrong,
		// so we never reveal which usernames exist (avoids user enumeration).
		if (user is null)
		{
			return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid username or password"));
		}

		var passwordValid = await _signInManager.CheckPasswordSignInAsync(
			user, request.Password,lockoutOnFailure: false);

		if(!passwordValid.Succeeded)
		{
			return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid username or password."));
		}

		//Roles become role claims inside the token
		var roles = await _userManager.GetRolesAsync(user);
		var token = _tokenService.CreateToken(user, roles);

		var payload = new LoginResponse { Token = token, FullName = user.FullName };
		return Ok(ApiResponse<LoginResponse>.Ok(payload));
	}
}
