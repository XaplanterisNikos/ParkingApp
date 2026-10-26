using ParkingApp.Client.Consumers.Pricing;
using ParkingApp.Shared.Pricing;
using System.Globalization;

namespace ParkingApp.Client.ViewModels;

/// <summary>
/// View model for the company's holiday calendar page: the Sunday setting, the marked dates,
/// and the form to add one. Created manually per visit.
/// </summary>
public class HolidaysViewModel
{
	#region Fields
	// Dates travel as "yyyy-MM-dd", the format of the browser's date input
	private const string DateFormat = "yyyy-MM-dd";

	private readonly IHolidaysConsumer _holidaysConsumer;
	#endregion

	#region Constructor
	/// <summary>
	/// Creates the view model. Instantiated manually by the page (not via DI).
	/// </summary>
	/// <param name="holidaysConsumer">Loads and changes the holiday calendar.</param>
	public HolidaysViewModel(IHolidaysConsumer holidaysConsumer)
	{
		_holidaysConsumer = holidaysConsumer;
	}
	#endregion

	#region State
	/// <summary>True when every Sunday is charged with holiday prices.</summary>
	public bool SundaysAreHolidays { get; private set; }

	/// <summary>The marked dates, in date order.</summary>
	public List<HolidayDto> Holidays { get; private set; } = new();

	/// <summary>True while the page's data is being loaded.</summary>
	public bool IsLoading { get; private set; } = true;

	/// <summary>Error message if loading failed.</summary>
	public string? LoadError { get; private set; }

	/// <summary>True while a change is being sent (disables the controls).</summary>
	public bool IsSaving { get; private set; }

	/// <summary>Error message of the last change, if it failed.</summary>
	public string? Error { get; private set; }

	/// <summary>The date of the new holiday as "yyyy-MM-dd", for the date field.</summary>
	public string NewDateText { get; set; } = string.Empty;

	/// <summary>The name of the new holiday.</summary>
	public string NewName { get; set; } = string.Empty;
	#endregion

	#region Public methods
	/// <summary>Loads the calendar.</summary>
	public async Task InitializeAsync()
	{
		IsLoading = true;
		LoadError = null;

		try
		{
			var result = await _holidaysConsumer.GetCalendarAsync();

			if (result is not { Success: true, Value: not null })
			{
				LoadError = result?.Message ?? "Δεν ήταν δυνατή η φόρτωση των αργιών.";
				return;
			}

			SundaysAreHolidays = result.Value.SundaysAreHolidays;
			Holidays = result.Value.Holidays;
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

	/// <summary>Changes the Sunday setting at once (no separate save button).</summary>
	/// <param name="value">The new setting.</param>
	public async Task SetSundaysAreHolidaysAsync(bool value)
	{
		await RunAsync(async () =>
		{
			var result = await _holidaysConsumer.SetSettingsAsync(new HolidaySettingsRequest { SundaysAreHolidays = value });

			if (result is { Success: true })
			{
				SundaysAreHolidays = value;
			}
			else
			{
				Error = result?.Message ?? "Η ρύθμιση δεν αποθηκεύτηκε.";
			}
		});
	}

	/// <summary>Adds the date of the form to the calendar and clears the form.</summary>
	public async Task AddAsync()
	{
		if (!DateOnly.TryParseExact(NewDateText, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
		{
			Error = "Διαλέξτε ημερομηνία.";
			return;
		}

		await RunAsync(async () =>
		{
			var result = await _holidaysConsumer.AddAsync(new CreateHolidayRequest { Date = date, Name = NewName });

			if (result is { Success: true, Value: not null })
			{
				// Keep the list in date order without a reload
				Holidays = Holidays.Append(result.Value).OrderBy(holiday => holiday.Date).ToList();
				NewDateText = string.Empty;
				NewName = string.Empty;
			}
			else
			{
				Error = result?.Message ?? "Η αργία δεν προστέθηκε.";
			}
		});
	}

	/// <summary>Removes a date from the calendar.</summary>
	/// <param name="holiday">The holiday to remove.</param>
	public async Task DeleteAsync(HolidayDto holiday)
	{
		await RunAsync(async () =>
		{
			var result = await _holidaysConsumer.DeleteAsync(holiday.Id);

			if (result is { Success: true })
			{
				Holidays = Holidays.Where(item => item.Id != holiday.Id).ToList();
			}
			else
			{
				Error = result?.Message ?? "Η αργία δεν διαγράφηκε.";
			}
		});
	}
	#endregion

	#region Helpers
	/// <summary>
	/// Runs one change with the shared busy flag and error handling: the controls are disabled
	/// while it runs, and a network failure becomes a message.
	/// </summary>
	private async Task RunAsync(Func<Task> change)
	{
		IsSaving = true;
		Error = null;

		try
		{
			await change();
		}
		catch
		{
			Error = "Δεν υπάρχει σύνδεση με τον server. Δοκιμάστε ξανά σε λίγο.";
		}
		finally
		{
			IsSaving = false;
		}
	}
	#endregion
}
