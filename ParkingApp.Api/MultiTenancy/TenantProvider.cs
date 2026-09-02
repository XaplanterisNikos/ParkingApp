using ParkingApp.Api.Extensions;

namespace ParkingApp.Api.MultiTenancy;

/// <summary>
/// Default <see cref="ITenantProvider"/>: reads the company id from the current
/// request's authenticated user claims.
/// </summary>
public class TenantProvider : ITenantProvider
{
	// Access to the current HttpContext
	private readonly IHttpContextAccessor _contextAccessor;

	public TenantProvider(IHttpContextAccessor contextAccessor)
	{
		_contextAccessor = contextAccessor;
	}

	/// <inheritdoc />
	public Guid? CurrentCompanyId 
	{
		get
		{
			var user = _contextAccessor.HttpContext?.User;

			// No authenticated user (anonymous request, or no HTTP context at all).
			if (user?.Identity?.IsAuthenticated != true)
			{
				return null;
			}

			return user.GetComapnyId();
		}

	}
}
