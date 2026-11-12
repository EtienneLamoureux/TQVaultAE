using TQVaultAE.Application.KeyBinds;

namespace TQVaultAE.GUI.Infrastructure;

/// <summary>
/// Turns the Windows Forms keys into the gestures of the key bind map (<see cref="KeyBindMap"/>), for the handlers
/// that ask it which action a key or a click runs.
/// </summary>
internal static class KeyBindInput
{
	/// <summary>The gesture of a key event's <see cref="KeyEventArgs.KeyData"/>: a modifier key alone gives modifiers only.</summary>
	public static KeyGesture FromKeys(Keys keyData)
	{
		var key = keyData & Keys.KeyCode;
		var name = key is Keys.None or Keys.ControlKey or Keys.ShiftKey or Keys.Menu or Keys.LControlKey or Keys.RControlKey
			or Keys.LShiftKey or Keys.RShiftKey or Keys.LMenu or Keys.RMenu
			? string.Empty
			: key.ToString();

		return new KeyGesture(name, ToModifiers(keyData));
	}

	/// <summary>A left click with <paramref name="modifierKeys"/> held (<see cref="Control.ModifierKeys"/>).</summary>
	public static KeyGesture Click(Keys modifierKeys) => new(KeyGesture.Click, ToModifiers(modifierKeys));

	/// <summary>
	/// Whether the modifiers held now make a click of the character editor spend or remove every point
	/// (<see cref="KeyBindAction.CharacterSpendAll"/>, Shift by default), for either mouse button.
	/// </summary>
	public static bool SpendAllHeld(KeyBindMap? keyBinds)
		=> keyBinds?.Matches(KeyBindAction.CharacterSpendAll, Click(Control.ModifierKeys)) == true;

	/// <summary>The modifiers of <paramref name="keys"/>.</summary>
	public static KeyModifiers ToModifiers(Keys keys)
		=> (keys.HasFlag(Keys.Control) ? KeyModifiers.Ctrl : KeyModifiers.None)
		| (keys.HasFlag(Keys.Shift) ? KeyModifiers.Shift : KeyModifiers.None)
		| (keys.HasFlag(Keys.Alt) ? KeyModifiers.Alt : KeyModifiers.None);

	/// <summary>
	/// The Windows Forms keys of a key gesture (the key and its modifiers), <see cref="Keys.None"/> for a mouse gesture or
	/// a key name Windows Forms does not have.
	/// </summary>
	public static Keys ToKeys(KeyGesture gesture)
	{
		if (!gesture.IsKey || !Enum.TryParse<Keys>(gesture.Key, ignoreCase: true, out var key) || int.TryParse(gesture.Key, out _))
			return Keys.None;

		return key
			| (gesture.Modifiers.HasFlag(KeyModifiers.Ctrl) ? Keys.Control : Keys.None)
			| (gesture.Modifiers.HasFlag(KeyModifiers.Shift) ? Keys.Shift : Keys.None)
			| (gesture.Modifiers.HasFlag(KeyModifiers.Alt) ? Keys.Alt : Keys.None);
	}
}
