using ParkingApp.Client.Consumers.Branches;
using ParkingApp.Client.Consumers.Pricing;
using ParkingApp.Shared.Branches;
using ParkingApp.Shared.Pricing;
using ParkingApp.Shared.Spots;
using System.Globalization;

namespace ParkingApp.Client.ViewModels;

/// <summary>
/// View model for a branch's price list page. Loads the current version into an editable
/// form; saving sends the whole form and the server stores it as a new version.
/// Created manually per visit.
/// </summary>
public class PricingViewModel
{
	#region Fields
	// Times travel as "HH:mm", the format of the browser's time input
	private const string TimeFormat = "HH:mm";

	private readonly IBranchesConsumer _branchesConsumer;
	private readonly ITariffsConsumer _tariffsConsumer;
	private readonly Guid _branchId;
	#endregion

	#region Constructor
	/// <summary>
	/// Creates the view model. Instantiated manually by the page (not via DI).
	/// </summary>
	/// <param name="branchesConsumer">Loads the branch (for its name).</param>
	/// <param name="tariffsConsumer">Loads and saves the price list.</param>
	/// <param name="branchId">The branch whose price list is edited.</param>
	public PricingViewModel(IBranchesConsumer branchesConsumer, ITariffsConsumer tariffsConsumer, Guid branchId)
	{
		_branchesConsumer = branchesConsumer;
		_tariffsConsumer = tariffsConsumer;
		_branchId = branchId;
	}
	#endregion

	#region State
	/// <summary>The branch whose price list is edited.</summary>
	public BranchDto? Branch { get; private set; }

	/// <summary>The editable price list. Starts as the current version (or the server's defaults).</summary>
	public TariffDto Form { get; private set; } = new();

	/// <summary>True while the page's data is being loaded.</summary>
	public bool IsLoading { get; private set; } = true;

	/// <summary>Error message if loading failed.</summary>
	public string? LoadError { get; private set; }

	/// <summary>True while a save request is in flight.</summary>
	public bool IsSaving { get; private set; }

	/// <summary>The rules the last save broke (or a single general message).</summary>
	public List<string> SaveErrors { get; private set; } = new();

	/// <summary>True right after a successful save, to show a confirmation.</summary>
	public bool JustSaved { get; private set; }

	/// <summary>The regular prices, one row per spot size, in size order.</summary>
	public List<TariffRateDto> RegularRates => RatesOf(DayType.Regular);

	/// <summary>The holiday prices (empty when holidays use the regular prices).</summary>
	public List<TariffRateDto> HolidayRates => RatesOf(DayType.Holiday);

	/// <summary>
	/// True when holidays have their own prices. Turning it on copies the regular prices as a
	/// starting point; turning it off removes the holiday prices.
	/// </summary>
	public bool HasHolidayPrices
	{
		get => HolidayRates.Count > 0;
		set
		{
			if (value == HasHolidayPrices) return;

			if (value)
			{
				Form.Rates.AddRange(RegularRates.Select(rate => new TariffRateDto
				{
					Size = rate.Size,
					DayType = DayType.Holiday,
					DayStepPrice = rate.DayStepPrice,
					NightStepPrice = rate.NightStepPrice,
					DailyCap = rate.DailyCap
				}));
			}
			else
			{
				Form.Rates.RemoveAll(rate => rate.DayType == DayType.Holiday);
			}
		}
	}

	/// <summary>Start of the night period as "HH:mm", for the time field.</summary>
	public string NightStartText
	{
		get => Form.NightStart.ToString(TimeFormat, CultureInfo.InvariantCulture);
		set => Form.NightStart = ParseTime(value, Form.NightStart);
	}

	/// <summary>End of the night period as "HH:mm", for the time field.</summary>
	public string NightEndText
	{
		get => Form.NightEnd.ToString(TimeFormat, CultureInfo.InvariantCulture);
		set => Form.NightEnd = ParseTime(value, Form.NightEnd);
	}

	/// <summary>The overnight checkout time as "HH:mm", for the time field.</summary>
	public string CheckoutText
	{
		get => Form.OvernightCheckoutTime.ToString(TimeFormat, CultureInfo.InvariantCulture);
		set => Form.OvernightCheckoutTime = ParseTime(value, Form.OvernightCheckoutTime);
	}
	#endregion

	#region Public methods
	/// <summary>Loads the branch and its current price list.</summary>
	public async Task InitializeAsync()
	{
		IsLoading = true;
		LoadError = null;

		try
		{
			var branchResult = await _branchesConsumer.GetByIdAsync(_branchId);

			if (branchResult is not { Success: true, Value: not null })
			{
				LoadError = branchResult?.Message ?? "Το υποκατάστημα δεν βρέθηκε.";
				return;
			}

			Branch = branchResult.Value;

			var tariffResult = await _tariffsConsumer.GetCurrentAsync(_branchId);

			if (tariffResult is not { Success: true, Value: not null })
			{
				LoadError = tariffResult?.Message ?? "Δεν ήταν δυνατή η φόρτωση του τιμοκαταλόγου.";
				return;
			}

			SetForm(tariffResult.Value);
		}
		catch
		{
			LoadError = "Δεν υπάρχει σύνδεση με τον server. Δοκιμάστε ξανά σε λίγο.";
		}
		finally
		{
			IsLoading = false;
		}
	}

	/// <summary>Adds an empty package row for the owner to fill in.</summary>
	public void AddPackage()
	{
		Form.Packages.Add(new OvernightPackageDto());
		JustSaved = false;
	}

	/// <summary>Removes a package row from the form (nothing is saved until "Save").</summary>
	/// <param name="package">The row to remove.</param>
	public void RemovePackage(OvernightPackageDto package)
	{
		Form.Packages.Remove(package);
		JustSaved = false;
	}

	/// <summary>Saves the whole form as a new version; on success the form shows the saved version.</summary>
	public async Task SaveAsync()
	{
		IsSaving = true;
		SaveErrors = new List<string>();
		JustSaved = false;

		try
		{
			var result = await _tariffsConsumer.SaveAsync(_branchId, Form);

			if (result is { Success: true, Value: not null })
			{
				SetForm(result.Value);
				JustSaved = true;
			}
			else
			{
				// The server lists every broken rule; fall back to its general message
				SaveErrors = result?.Errors is { Count: > 0 } errors
					? errors
					: new List<string> { result?.Message ?? "Ο τιμοκατάλογος δεν αποθηκεύτηκε." };
			}
		}
		catch
		{
			SaveErrors = new List<string> { "Δεν υπάρχει σύνδεση με τον server. Δοκιμάστε ξανά σε λίγο." };
		}
		finally
		{
			IsSaving = false;
		}
	}
	#endregion

	#region Helpers
	/// <summary>
	/// Puts a price list into the form, making sure every spot size has a row of regular prices
	/// (a new size shows up as an empty row to fill in).
	/// </summary>
	private void SetForm(TariffDto tariff)
	{
		foreach (var size in Enum.GetValues<SpotSize>())
		{
			if (!tariff.Rates.Any(rate => rate.Size == size && rate.DayType == DayType.Regular))
			{
				tariff.Rates.Add(new TariffRateDto { Size = size, DayType = DayType.Regular });
			}
		}

		Form = tariff;
	}

	/// <summary>The rows of one day type, in size order. They are the same objects as in the form.</summary>
	private List<TariffRateDto> RatesOf(DayType dayType) =>
		Form.Rates.Where(rate => rate.DayType == dayType).OrderBy(rate => rate.Size).ToList();

	/// <summary>Reads "HH:mm" from the time field; keeps the old value if the text is not a time.</summary>
	private static TimeOnly ParseTime(string? text, TimeOnly current) =>
		TimeOnly.TryParseExact(text, TimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time)
			? time
			: current;
	#endregion
}
