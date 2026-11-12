namespace TQVaultAE.Application.KeyBinds;

/// <summary>Several actions of one scope share a gesture.</summary>
/// <param name="Gesture">The shared gesture.</param>
/// <param name="Scope">The scope of the actions.</param>
/// <param name="Actions">The actions, in the order of <see cref="KeyBindRegistry.All"/>.</param>
public sealed record KeyBindConflict(KeyGesture Gesture, KeyBindScope Scope, IReadOnlyList<KeyBindAction> Actions);

/// <summary>
/// The gestures of every action (<see cref="KeyBindRegistry"/>): the defaults with the user's changes. The GUI handlers
/// ask it whether a key or a click is bound to an action; the key binds dialog edits a copy of it.
/// </summary>
/// <remarks>
/// Saved as the actions that differ from their defaults (<see cref="ToOverrides"/>), so a default changed in a later
/// version reaches the actions the user left alone.
/// </remarks>
public sealed class KeyBindMap
{
	/// <summary>The gestures one action can have.</summary>
	public const int MaxGestures = 2;

	/// <summary>Separates the gestures of one action in <see cref="ToOverrides"/>.</summary>
	private const string Separator = ", ";

	private readonly Dictionary<KeyBindAction, KeyGesture[]> _gestures = [];

	private KeyBindMap()
	{
		this.ResetAll();
	}

	/// <summary>The default gestures.</summary>
	public static KeyBindMap Defaults() => new();

	/// <summary>
	/// The defaults with the changes saved by <see cref="ToOverrides"/>: action name, gestures separated by commas, empty
	/// for an action without a gesture. Unknown or fixed actions and unreadable gestures are skipped; an action none of
	/// whose gestures can be read keeps its defaults.
	/// </summary>
	public static KeyBindMap FromOverrides(IEnumerable<KeyValuePair<string, string>>? overrides)
	{
		var map = new KeyBindMap();
		foreach (var (name, value) in overrides ?? [])
		{
			// By name only: a number would also parse.
			if (!Enum.GetNames<KeyBindAction>().Contains(name, StringComparer.Ordinal))
				continue;

			var action = Enum.Parse<KeyBindAction>(name);
			var definition = KeyBindRegistry.Get(action);
			if (!definition.Editable)
				continue;

			var texts = (value ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
			var gestures = texts
				.Select(text => KeyGesture.TryParse(text, out var gesture) ? gesture : KeyGesture.None)
				.Where(gesture => !gesture.IsNone && definition.Accepts(gesture))
				.Distinct()
				.Take(MaxGestures)
				.ToArray();

			if (texts.Length > 0 && gestures.Length == 0)
				continue;

			map.SetAll(action, gestures);
		}

		return map;
	}

	/// <summary>The actions whose gestures differ from their defaults, written for <see cref="FromOverrides"/>.</summary>
	public IReadOnlyDictionary<string, string> ToOverrides()
		=> KeyBindRegistry.All
			.Where(d => d.Editable && !this.IsDefault(d.Action))
			.ToDictionary(d => d.Action.ToString(), d => string.Join(Separator, this.Get(d.Action)), StringComparer.Ordinal);

	/// <summary>A copy, edited without changing this map.</summary>
	public KeyBindMap Clone()
	{
		var copy = new KeyBindMap();
		copy.CopyFrom(this);
		return copy;
	}

	/// <summary>Takes the gestures of <paramref name="other"/>.</summary>
	public void CopyFrom(KeyBindMap other)
	{
		foreach (var (action, gestures) in other._gestures)
			this._gestures[action] = [.. gestures];
	}

	/// <summary>The gestures of <paramref name="action"/>, without the empty slots.</summary>
	public IReadOnlyList<KeyGesture> Get(KeyBindAction action)
		=> [.. this._gestures[action].Where(g => !g.IsNone)];

	/// <summary>The gesture in <paramref name="slot"/> of <paramref name="action"/>, <see cref="KeyGesture.None"/> when empty.</summary>
	public KeyGesture Get(KeyBindAction action, int slot)
		=> slot >= 0 && slot < MaxGestures ? this._gestures[action][slot] : KeyGesture.None;

	/// <summary>Whether <paramref name="gesture"/> is one of the gestures of <paramref name="action"/>.</summary>
	public bool Matches(KeyBindAction action, KeyGesture gesture)
		=> !gesture.IsNone && Array.IndexOf(this._gestures[action], gesture) >= 0;

	/// <summary>
	/// The first of <paramref name="actions"/> bound to <paramref name="gesture"/>, for a handler that offers several.
	/// </summary>
	public KeyBindAction? Find(KeyGesture gesture, params ReadOnlySpan<KeyBindAction> actions)
	{
		foreach (var action in actions)
			if (this.Matches(action, gesture))
				return action;

		return null;
	}

	/// <summary>
	/// Puts <paramref name="gesture"/> in <paramref name="slot"/> of <paramref name="action"/>, or empties it with
	/// <see cref="KeyGesture.None"/>. The same gesture in its other slot is emptied.
	/// </summary>
	/// <returns>false, changing nothing, for a fixed action, a slot out of range or a gesture of the wrong kind.</returns>
	public bool Set(KeyBindAction action, int slot, KeyGesture gesture)
	{
		var definition = KeyBindRegistry.Get(action);
		if (!definition.Editable || slot < 0 || slot >= MaxGestures || !definition.Accepts(gesture))
			return false;

		var gestures = this._gestures[action];
		for (int i = 0; i < gestures.Length; i++)
			if (i != slot && !gesture.IsNone && gestures[i] == gesture)
				gestures[i] = KeyGesture.None;

		gestures[slot] = gesture;
		return true;
	}

	/// <summary>Whether <paramref name="action"/> has its default gestures, in any slot order.</summary>
	public bool IsDefault(KeyBindAction action)
	{
		var current = this.Get(action);
		var defaults = KeyBindRegistry.Get(action).Defaults;
		return current.Count == defaults.Count && current.All(defaults.Contains);
	}

	/// <summary>Gives <paramref name="action"/> its default gestures.</summary>
	public void Reset(KeyBindAction action) => this.SetAll(action, KeyBindRegistry.Get(action).Defaults);

	/// <summary>Gives every action its default gestures.</summary>
	public void ResetAll()
	{
		foreach (var definition in KeyBindRegistry.All)
			this.SetAll(definition.Action, definition.Defaults);
	}

	private void SetAll(KeyBindAction action, IReadOnlyList<KeyGesture> gestures)
	{
		var slots = new KeyGesture[MaxGestures];
		for (int i = 0; i < Math.Min(MaxGestures, gestures.Count); i++)
			slots[i] = gestures[i];

		this._gestures[action] = slots;
	}

	/// <summary>The gestures shared by several actions of one scope, fixed actions included.</summary>
	public IReadOnlyList<KeyBindConflict> Conflicts()
		=> KeyBindRegistry.All
			.SelectMany(d => this.Get(d.Action).Select(g => (d.Scope, Gesture: g, d.Action)))
			.GroupBy(x => (x.Scope, x.Gesture))
			.Where(g => g.Select(x => x.Action).Distinct().Count() > 1)
			.Select(g => new KeyBindConflict(g.Key.Gesture, g.Key.Scope, [.. g.Select(x => x.Action).Distinct()]))
			.ToList();

	/// <summary>Whether the gesture in <paramref name="slot"/> of <paramref name="action"/> is shared with another action.</summary>
	public bool HasConflict(KeyBindAction action, int slot)
	{
		var gesture = this.Get(action, slot);
		if (gesture.IsNone)
			return false;

		var scope = KeyBindRegistry.Get(action).Scope;
		return KeyBindRegistry.All.Any(d => d.Action != action && d.Scope == scope && this.Matches(d.Action, gesture));
	}
}
