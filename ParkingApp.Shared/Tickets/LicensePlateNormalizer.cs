namespace ParkingApp.Shared.Tickets;

/// <summary>
/// Normalizes and checks licence plates. The server uses it as the final authority;
/// the client may use the same code to preview the result while the employee types.
/// A normalized plate is uppercase, has no spaces, dashes or dots, and uses Latin letters only.
/// </summary>
/// <remarks>
/// This class checks the FORMAT of a plate only. It does not check whether the plate
/// is registered to a real vehicle.
/// </remarks>
public static class LicensePlateNormalizer
{
	/// <summary>The shortest normalized plate that is accepted.</summary>
	public const int MinLength = 2;

	/// <summary>The longest normalized plate that is accepted (the database column allows 15).</summary>
	public const int MaxLength = 10;

	// Greek plates use only the 14 Greek capitals that have a Latin look-alike.
	// Each Greek letter maps to the Latin letter that looks the same.
	private static readonly Dictionary<char, char> GreekToLatin = new()
	{
		['Α'] = 'A', ['Β'] = 'B', ['Ε'] = 'E', ['Ζ'] = 'Z', ['Η'] = 'H', ['Ι'] = 'I', ['Κ'] = 'K',
		['Μ'] = 'M', ['Ν'] = 'N', ['Ο'] = 'O', ['Ρ'] = 'P', ['Τ'] = 'T', ['Υ'] = 'Y', ['Χ'] = 'X'
	};

	// The reverse map, used to show a stored Greek plate with Greek letters.
	private static readonly Dictionary<char, char> LatinToGreek =
		GreekToLatin.ToDictionary(pair => pair.Value, pair => pair.Key);

	/// <summary>
	/// Normalizes a plate typed by a person and checks it against the rules of its kind.
	/// </summary>
	/// <param name="input">The plate as typed (any case, may contain spaces, dashes or dots).</param>
	/// <param name="kind">The kind of the plate.</param>
	/// <param name="normalized">The normalized plate when the method returns true; otherwise an empty string.</param>
	/// <param name="error">A message for the employee when the method returns false; otherwise null.</param>
	/// <returns>True when the plate is valid for its kind.</returns>
	public static bool TryNormalize(string? input, PlateKind kind, out string normalized, out string? error)
	{
		normalized = string.Empty;

		// Uppercase first, so that "αβγ" and "abc" are handled like "ΑΒΓ" and "ABC".
		var characters = (input ?? string.Empty)
			.ToUpperInvariant()
			// Separators people type out of habit are not part of the plate.
			.Where(character => character is not (' ' or '-' or '.'))
			.ToList();

		if (characters.Count < MinLength || characters.Count > MaxLength)
		{
			error = $"Η πινακίδα πρέπει να έχει από {MinLength} έως {MaxLength} χαρακτήρες.";
			return false;
		}

		var result = new char[characters.Count];

		for (var index = 0; index < characters.Count; index++)
		{
			var character = characters[index];

			if (char.IsAsciiDigit(character))
			{
				// Digits are the same for every kind.
				result[index] = character;
			}
			else if (kind == PlateKind.Greek && GreekToLatin.TryGetValue(character, out var latin))
			{
				// A Greek letter is stored as its Latin look-alike.
				result[index] = latin;
			}
			else if (kind == PlateKind.Greek && LatinToGreek.ContainsKey(character))
			{
				// A Latin look-alike typed on a Latin keyboard is already in stored form.
				result[index] = character;
			}
			else if (kind == PlateKind.Foreign && char.IsAsciiLetterUpper(character))
			{
				result[index] = character;
			}
			else
			{
				error = kind == PlateKind.Greek
					? $"Ο χαρακτήρας «{character}» δεν επιτρέπεται σε ελληνική πινακίδα."
					: $"Ο χαρακτήρας «{character}» δεν επιτρέπεται σε ξένη πινακίδα.";
				return false;
			}
		}

		normalized = new string(result);
		error = null;
		return true;
	}

	/// <summary>
	/// Returns the plate in the form people expect to read: Greek letters for a Greek plate,
	/// unchanged for a foreign plate.
	/// </summary>
	/// <param name="normalizedPlate">A plate as stored (normalized).</param>
	/// <param name="kind">The kind of the plate.</param>
	public static string ToDisplay(string normalizedPlate, PlateKind kind)
	{
		if (kind != PlateKind.Greek) return normalizedPlate;

		// Every letter of a stored Greek plate has a Greek look-alike; digits stay as they are.
		return new string(normalizedPlate
			.Select(character => LatinToGreek.TryGetValue(character, out var greek) ? greek : character)
			.ToArray());
	}
}
