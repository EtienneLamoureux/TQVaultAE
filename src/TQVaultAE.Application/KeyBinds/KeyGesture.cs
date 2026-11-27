using System.Text;

namespace TQVaultAE.Application.KeyBinds;

/// <summary>The modifier keys held with a key or a click.</summary>
[Flags]
public enum KeyModifiers
{
	None = 0,
	Ctrl = 1,
	Shift = 2,
	Alt = 4,
}

/// <summary>
/// A key or a mouse click with the modifiers held, written <c>Ctrl+Alt+Click</c>, <c>Alt+W</c>, <c>Back</c>. Keys are
/// named like the Windows Forms <c>Keys</c> members (<c>Home</c>, <c>Oemplus</c>, <c>F5</c>); the mouse has
/// <see cref="Click"/>, <see cref="RightClick"/> and <see cref="Wheel"/>. A gesture may also be modifiers alone
/// (<c>Shift</c>), held during another action.
/// </summary>
/// <remarks>Two gestures are equal when they have the same modifiers and the same key, whatever its case.</remarks>
public readonly struct KeyGesture : IEquatable<KeyGesture>
{
	/// <summary>A click of the left mouse button.</summary>
	public const string Click = "Click";

	/// <summary>A click of the right mouse button.</summary>
	public const string RightClick = "RightClick";

	/// <summary>A turn of the mouse wheel.</summary>
	public const string Wheel = "Wheel";

	/// <summary>Key names Windows gives twice, written with the first one.</summary>
	private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
	{
		["Return"] = "Enter",
		["Prior"] = "PageUp",
		["Next"] = "PageDown",
		["Capital"] = "CapsLock",
		["Snapshot"] = "PrintScreen",
	};

	private readonly string? _key;

	/// <summary>Initializes a new instance of the <see cref="KeyGesture"/> struct.</summary>
	/// <param name="key">The key name, <see cref="Click"/>, or empty for modifiers alone.</param>
	/// <param name="modifiers">The modifiers held.</param>
	public KeyGesture(string? key, KeyModifiers modifiers = KeyModifiers.None)
	{
		this._key = key is null ? string.Empty : Aliases.GetValueOrDefault(key.Trim(), key.Trim());
		this.Modifiers = modifiers;
	}

	/// <summary>No gesture: an action without a binding.</summary>
	public static KeyGesture None => default;

	/// <summary>The key name; empty for modifiers alone or no gesture.</summary>
	public string Key => this._key ?? string.Empty;

	/// <summary>The modifiers held.</summary>
	public KeyModifiers Modifiers { get; }

	/// <summary>No key and no modifier.</summary>
	public bool IsNone => this.Key.Length == 0 && this.Modifiers == KeyModifiers.None;

	/// <summary>A click of the left button, with or without modifiers.</summary>
	public bool IsClick => string.Equals(this.Key, Click, StringComparison.OrdinalIgnoreCase);

	/// <summary>A gesture of the mouse: a click of either button or the wheel.</summary>
	public bool IsMouse
		=> this.IsClick
		|| string.Equals(this.Key, RightClick, StringComparison.OrdinalIgnoreCase)
		|| string.Equals(this.Key, Wheel, StringComparison.OrdinalIgnoreCase);

	/// <summary>Modifiers held without a key.</summary>
	public bool IsModifiersOnly => this.Key.Length == 0 && this.Modifiers != KeyModifiers.None;

	/// <summary>A gesture of the keyboard: a key, with or without modifiers.</summary>
	public bool IsKey => this.Key.Length > 0 && !this.IsMouse;

	/// <summary>The gesture with other modifiers.</summary>
	public KeyGesture WithModifiers(KeyModifiers modifiers) => new(this.Key, modifiers);

	/// <inheritdoc/>
	public bool Equals(KeyGesture other)
		=> this.Modifiers == other.Modifiers && string.Equals(this.Key, other.Key, StringComparison.OrdinalIgnoreCase);

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is KeyGesture other && this.Equals(other);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(this.Key), this.Modifiers);

	public static bool operator ==(KeyGesture left, KeyGesture right) => left.Equals(right);

	public static bool operator !=(KeyGesture left, KeyGesture right) => !left.Equals(right);

	/// <summary>The gesture as written in the configuration: <c>Ctrl+Shift+Alt+Key</c>; empty for none.</summary>
	public override string ToString()
	{
		var text = new StringBuilder();
		foreach (var (flag, name) in ModifierNames)
		{
			if (!this.Modifiers.HasFlag(flag))
				continue;

			if (text.Length > 0)
				text.Append('+');
			text.Append(name);
		}

		if (this.Key.Length > 0)
		{
			if (text.Length > 0)
				text.Append('+');
			text.Append(this.Key);
		}

		return text.ToString();
	}

	/// <summary>The modifiers in the order they are written.</summary>
	internal static readonly (KeyModifiers Flag, string Name)[] ModifierNames =
	[
		(KeyModifiers.Ctrl, "Ctrl"),
		(KeyModifiers.Shift, "Shift"),
		(KeyModifiers.Alt, "Alt"),
	];

	/// <summary>
	/// Reads a gesture written like <see cref="ToString"/>: modifiers (<c>Ctrl</c> or <c>Control</c>, <c>Shift</c>,
	/// <c>Alt</c>, any case, any order) and at most one key, the last, a name of letters and digits starting with a letter.
	/// </summary>
	/// <returns>false for empty text, an unknown token or two keys.</returns>
	public static bool TryParse(string? text, out KeyGesture gesture)
	{
		gesture = None;
		if (string.IsNullOrWhiteSpace(text))
			return false;

		var modifiers = KeyModifiers.None;
		string? key = null;
		foreach (var raw in text.Split('+'))
		{
			var token = raw.Trim();
			KeyModifiers modifier = token.ToUpperInvariant() switch
			{
				"CTRL" or "CONTROL" => KeyModifiers.Ctrl,
				"SHIFT" => KeyModifiers.Shift,
				"ALT" => KeyModifiers.Alt,
				_ => KeyModifiers.None,
			};

			if (modifier != KeyModifiers.None && key is null)
				modifiers |= modifier;
			else if (key is null && IsKeyName(token))
				key = token;
			else
				return false;
		}

		gesture = new KeyGesture(key, modifiers);
		return true;
	}

	private static bool IsKeyName(string token)
		=> token.Length > 0 && char.IsAsciiLetter(token[0]) && token.All(char.IsAsciiLetterOrDigit);
}
