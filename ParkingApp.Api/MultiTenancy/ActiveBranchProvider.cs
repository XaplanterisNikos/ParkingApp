using ParkingApp.Api.Extensions;

namespace ParkingApp.Api.MultiTenancy;

/// <summary>
/// Default <see cref="IActiveBranchProvider"/>: reads the active branch id from the
/// current request's authenticated user claims.
/// </summary>
public class ActiveBranchProvider : IActiveBranchProvider
{
	// Access to the current HttpContext (and through it, the user's claims)
	private readonly IHttpContextAccessor _contextAccessor;

	/// <summary>
	/// Creates the provider.
	/// </summary>
	/// <param name="contextAccessor">Accessor for the current request's HttpContext.</param>
	public ActiveBranchProvider(IHttpContextAccessor contextAccessor)
	{
		_contextAccessor = contextAccessor;
	}

	/// <inheritdoc />
	public Guid? CurrentBranchId
		// No HttpContext (e.g. startup/seeding) → the whole chain yields null
		=> _contextAccessor.HttpContext?.User.GetActiveBranchId();
}