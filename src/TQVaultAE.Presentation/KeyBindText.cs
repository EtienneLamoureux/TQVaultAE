using System.Globalization;
using TQVaultAE.Application.KeyBinds;

namespace TQVaultAE.Presentation;

/// <summary>
/// The texts of the key binds: action and group names, gestures as the user reads them (<c>Ctrl+Alt+Clic</c>,
/// <c>Maj+Flèche gauche</c>) and the conflicts.
/// </summary>
public static class KeyBindText
{
	/// <summary>The name of <paramref name="action"/>.</summary>
	public static string Action(KeyBindAction action)
		=> Resources.ResourceManager.GetString($"KeyBindAction{action}", Resources.Culture) ?? action.ToString();

	/// <summary>The name of <paramref name="category"/>.</summary>
	public static string Category(KeyBindCategory category)
		=> Resources.ResourceManager.GetString($"KeyBindCategory{category}", Resources.Culture) ?? category.ToString();

	/// <summary>
	/// <paramref name="gesture"/> as the user reads it: the modifiers, then the key by its name on the keyboard;
	/// <see cref="Resources.KeyBindsNone"/> for none.
	/// </summary>
	public static string Gesture(KeyGesture gesture)
	{
		if (gesture.IsNone)
			return Resources.KeyBindsNone;

		var parts = new List<string>();
		if (gesture.Modifiers.HasFlag(KeyModifiers.Ctrl))
			parts.Add("Ctrl");
		if (gesture.Modifiers.HasFlag(KeyModifiers.Shift))
			parts.Add(Resources.KeyNameShift);
		if (gesture.Modifiers.HasFlag(KeyModifiers.Alt))
			parts.Add("Alt");
		if (gesture.Key.Length > 0)
			parts.Add(Key(gesture.Key));

		return string.Join("+", parts);
	}

	/// <summary>A key by its name on the keyboard: <c>Oemplus</c> is <c>+</c>, <c>D5</c> is <c>5</c>, <c>NumPad5</c> is <c>Num 5</c>.</summary>
	public static string Key(string key)
	{
		var named = Resources.ResourceManager.GetString($"KeyName{key}", Resources.Culture);
		if (named is not null && !key.Equals("NumPad", StringComparison.OrdinalIgnoreCase))
			return named;

		// Look-alike names by their sign; the digits of the top row and of the numeric pad by their digit.
		return key.ToUpperInvariant() switch
		{
			"OEMPLUS" => "+",
			"OEMMINUS" => "-",
			"OEMCOMMA" => ",",
			"OEMPERIOD" => ".",
			"ADD" => NumPad("+"),
			"SUBTRACT" => NumPad("-"),
			"MULTIPLY" => NumPad("*"),
			"DIVIDE" => NumPad("/"),
			"DECIMAL" => NumPad("."),
			var k when k.Length == 2 && k[0] == 'D' && char.IsAsciiDigit(k[1]) => k[1..],
			var k when k.Length == 7 && k.StartsWith("NUMPAD", StringComparison.Ordinal) && char.IsAsciiDigit(k[6]) => NumPad(k[6..]),
			_ => key,
		};
	}

	private static string NumPad(string sign) => string.Format(CultureInfo.CurrentCulture, Resources.KeyNameNumPad, sign);

	/// <summary>The name of the setting <paramref name="requirement"/> stands for, as the settings dialog shows it.</summary>
	public static string Setting(KeyBindRequirement requirement)
		=> requirement switch
		{
			KeyBindRequirement.ItemCopy => Resources.SettingsAllowCopy,
			KeyBindRequirement.CharacterEdit => Resources.SettingsAllowEditCE,
			_ => string.Empty,
		};

	/// <summary>Why an action does nothing: the setting to tick.</summary>
	public static string TurnedOff(KeyBindRequirement requirement)
		=> string.Format(CultureInfo.CurrentCulture, Resources.KeyBindsTurnedOff, Setting(requirement));

	/// <summary>
	/// The note under the list when some actions are off with <paramref name="options"/>: the settings to tick; empty
	/// when every action is on.
	/// </summary>
	public static string TurnedOffNote(KeyBindOptions options)
	{
		var off = KeyBindRegistry.All
			.Where(d => !d.IsOn(options))
			.Select(d => d.Requirement)
			.Distinct()
			.Select(Setting)
			.ToList();

		return off.Count == 0 ? string.Empty : string.Format(CultureInfo.CurrentCulture, Resources.KeyBindsTurnedOffNote, string.Join(", ", off));
	}

	/// <summary>The gestures of an action, separated by commas.</summary>
	public static string Gestures(IEnumerable<KeyGesture> gestures) => string.Join(", ", gestures.Select(Gesture));

	/// <summary>A conflict: the gesture and the actions sharing it.</summary>
	public static string Conflict(KeyBindConflict conflict)
		=> string.Format(CultureInfo.CurrentCulture, Resources.KeyBindsConflict, Gesture(conflict.Gesture), string.Join(", ", conflict.Actions.Select(Action)));
}
