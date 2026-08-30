using Microsoft.IdentityModel.Tokens;
using ParkingApp.Api.Data.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ParkingApp.Api.Services.Auth;

/// <summary>
/// Default <see cref="ITokenService"/> implementation. Reads JWT settings from
/// configuration and produces HMAC-SHA256 signed tokens.
/// </summary>
public class TokenService : ITokenService
{
	private readonly IConfiguration _configuration;

	public TokenService(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	/// <inheritdoc />
	public string CreateToken(ApplicationUser user, IEnumerable<string> roles)
	{
		var jwt = _configuration.GetSection("Jwt");
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		//The claims are the "facts" carried inside the token.
		var claims = new List<Claim>
		{
			// Who the user is (their Identity id).
			new(JwtRegisteredClaimNames.Sub, user.Id),
			new (ClaimTypes.NameIdentifier, user.Id),

			// Their username , handy on the client
			new(JwtRegisteredClaimNames.UniqueName, user.UserName!),

			// The tenant: this is what scopes every future query to one company.
			new("companyId",user.CompanyId.ToString())
		};

		// One role claim per role , so [Authorize(Roles="..")] works
		claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

		var expiryMinutes = int.Parse(jwt["ExpiryMInutes"]!);

		var token = new JwtSecurityToken(
			issuer: jwt["Issuer"],
			audience: jwt["Audience"],
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
			signingCredentials: credentials
			);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
