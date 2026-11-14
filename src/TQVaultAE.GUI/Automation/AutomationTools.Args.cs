//-----------------------------------------------------------------------
// <copyright file="AutomationTools.Args.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using ModelContextProtocol;
using TQVaultAE.Application.KeyBinds;
using TQVaultAE.GUI.Infrastructure;

namespace TQVaultAE.GUI.Automation;

/// <summary>
/// Reading the arguments of the tools, the same way for each of them: keys, modifiers, mouse buttons, enum names and
/// menu paths. Names ignore case.
/// </summary>
internal static partial class AutomationTools
{
	/// <summary>
	/// Reads a key for <c>press_key</c>, <c>list_keys</c> and the key binds dialog: one character (<c>c</c>, <c>5</c>),
	/// a name (<c>backspace</c>, <c>esc</c>, <c>pgdn</c>) or a gesture with modifiers (<c>ctrl+alt+d</c>, <c>alt</c>).
	/// </summary>
	private static Keys ParseKey(string text)
	{
		var trimmed = text.Trim();
		var normalized = trimmed.ToUpperInvariant() switch
		{
			"BACKSPACE" => "Back",
			"ESC" => "Escape",
			"DEL" => "Delete",
			"PGUP" => "PageUp",
			"PGDN" => "PageDown",
			var t when t.Length == 1 && char.IsAsciiDigit(t[0]) => "D" + t,
			_ => trimmed,
		};

		if (!KeyGesture.TryParse(normalized, out var gesture))
			throw new McpException($"Unknown key '{text}'. Use a character, a key name or a gesture like 'ctrl+alt+d'.");

		if (gesture.IsModifiersOnly)
			return AutomationInput.ModifierKeys(gesture.Modifiers);

		var keys = KeyBindInput.ToKeys(gesture);
		return keys == Keys.None ? throw new McpException($"'{text}' is not a key Windows Forms knows.") : keys;
	}

	/// <summary>The modifiers of a click tool: <paramref name="modifiers"/> ('ctrl+alt', empty for none), Shift added by <paramref name="shift"/>.</summary>
	private static Keys ClickModifiers(string? modifiers, bool shift = false)
		=> (string.IsNullOrWhiteSpace(modifiers) ? Keys.None : ParseKey(modifiers) & Keys.Modifiers) | (shift ? Keys.Shift : Keys.None);

	/// <summary>Reads a mouse button: 'left' or 'right'.</summary>
	private static MouseButtons ParseButton(string button) => button.Trim().ToLowerInvariant() switch
	{
		"left" => MouseButtons.Left,
		"right" => MouseButtons.Right,
		_ => throw new McpException($"Unknown button '{button}': 'left' or 'right'."),
	};

	/// <summary>Reads the name of a value of <typeparamref name="T"/>; <paramref name="what"/> names it in the error message.</summary>
	private static T ParseEnum<T>(string value, string what)
		where T : struct, Enum
		=> Enum.TryParse<T>(value.Trim(), ignoreCase: true, out var parsed) && !int.TryParse(value, out _)
			? parsed
			: throw new McpException($"Unknown {what} '{value}': {string.Join(", ", Enum.GetNames<T>())}.");

	/// <summary>Reads a menu path: entry texts from the top level down, separated by '&gt;'; empty for none.</summary>
	private static string[] MenuPath(string? path)
		=> path?.Split('>', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
}
