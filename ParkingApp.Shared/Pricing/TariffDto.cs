namespace ParkingApp.Shared.Pricing;

/// <summary>
/// The current price list of a branch. When the branch has no price list yet, the server
/// returns suggested defaults with <see cref="VersionId"/> = null, ready to be saved.
/// </summary>
public class TariffDto : SaveTariffRequest
{
	/// <summary>The id of the version; null when nothing has been saved yet.</summary>
	public Guid? VersionId { get; set; }

	/// <summary>When the version was saved; null when nothing has been saved yet.</summary>
	public DateTimeOffset? CreatedAt { get; set; }
}
