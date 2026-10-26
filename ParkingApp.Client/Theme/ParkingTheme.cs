using MudBlazor;

namespace ParkingApp.Client.Theme;

/// <summary>
/// The application's MudBlazor theme: colors, font, shapes and shadows.
/// </summary>
public static class ParkingTheme
{
	/// <summary>Parking sign blue. The only strong color: used for selection and main actions.</summary>
	public const string Blue = "#1F4FBF";

	/// <summary>Main text color.</summary>
	public const string Ink = "#16202A";

	/// <summary>Secondary text color.</summary>
	public const string Muted = "#5A6573";

	/// <summary>Page background.</summary>
	public const string Ground = "#F3F5F7";

	/// <summary>Borders and dividers.</summary>
	public const string Line = "#DCE1E7";

	/// <summary>The theme passed to MudThemeProvider.</summary>
	public static MudTheme Theme { get; } = new()
	{
		PaletteLight = new PaletteLight
		{
			Primary = Blue,
			PrimaryContrastText = "#FFFFFF",
			Secondary = Muted,
			SecondaryContrastText = "#FFFFFF",
			Info = Blue,
			Success = "#1F7A4D",
			Warning = "#9A6700",
			Error = "#B42318",
			Dark = Ink,
			TextPrimary = Ink,
			TextSecondary = Muted,
			ActionDefault = Muted,
			Background = Ground,
			Surface = "#FFFFFF",
			AppbarBackground = "#FFFFFF",
			AppbarText = Ink,
			DrawerBackground = "#FFFFFF",
			DrawerText = Ink,
			DrawerIcon = Muted,
			LinesDefault = Line,
			LinesInputs = "#C9D0D8",
			Divider = Line,
			TableLines = "#E8ECF0"
		},

		Typography = new Typography
		{
			Default = new DefaultTypography
			{
				FontFamily = ["Commissioner", "system-ui", "sans-serif"],
				FontSize = ".9375rem",
				LineHeight = "1.45",
				LetterSpacing = "normal"
			},
			// Page titles
			H4 = new H4Typography { FontSize = "1.75rem", FontWeight = "700", LineHeight = "1.25", LetterSpacing = "normal" },
			// Section titles
			H5 = new H5Typography { FontSize = "1.25rem", FontWeight = "600", LineHeight = "1.3", LetterSpacing = "normal" },
			// Card titles
			H6 = new H6Typography { FontSize = "1.0625rem", FontWeight = "600", LineHeight = "1.35", LetterSpacing = "normal" },
			Body1 = new Body1Typography { FontSize = ".9375rem", LineHeight = "1.45", LetterSpacing = "normal" },
			Body2 = new Body2Typography { FontSize = ".875rem", LineHeight = "1.45", LetterSpacing = "normal" },
			Caption = new CaptionTypography { FontSize = ".8125rem", LineHeight = "1.4", LetterSpacing = "normal" },
			// Sentence case on buttons, not upper case
			Button = new ButtonTypography { FontWeight = "600", TextTransform = "none", LetterSpacing = "normal" }
		},

		LayoutProperties = new LayoutProperties
		{
			DefaultBorderRadius = "8px",
			DrawerWidthLeft = "248px",
			AppbarHeight = "64px"
		},

		// Flat surfaces separated by borders. Only floating layers (menus, dialogs) keep a soft shadow.
		Shadows = new Shadow { Elevation = CreateElevation() }
	};

	/// <summary>
	/// Builds the 26 elevation levels MudBlazor expects: none up to level 4, one soft shadow above.
	/// </summary>
	private static string[] CreateElevation()
	{
		const string soft = "0 8px 24px rgba(22,32,42,0.12)";
		var levels = new string[26];

		for (var level = 0; level < levels.Length; level++)
		{
			levels[level] = level <= 4 ? "none" : soft;
		}

		return levels;
	}
}
