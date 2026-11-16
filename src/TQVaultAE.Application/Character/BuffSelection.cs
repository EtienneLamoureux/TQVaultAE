using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Character;

/// <summary>
/// Which buff sources of the Buffs tab are on: each source starts at its default (<see cref="BuffSource.DefaultOn"/>)
/// until a click toggles it. Kept in memory only, never saved; one per character shown.
/// </summary>
public sealed class BuffSelection
{
	private readonly Dictionary<string, bool> _toggled = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>Changes at every toggle and scroll choice: part of the key of what is computed from the selection.</summary>
	public int Version { get; private set; }

	/// <summary>The scroll picked in the tab (<c>OneShot_Scroll</c> record), null for none.</summary>
	public RecordId? Scroll { get; private set; }

	/// <summary>True when the source counts.</summary>
	public bool IsOn(BuffSource source)
	{
		ArgumentNullException.ThrowIfNull(source);
		return this._toggled.TryGetValue(source.Id, out var on) ? on : source.DefaultOn;
	}

	/// <summary>Turns a source on or off.</summary>
	public void Set(BuffSource source, bool on)
	{
		ArgumentNullException.ThrowIfNull(source);
		if (this.IsOn(source) == on)
			return;

		if (on == source.DefaultOn)
			this._toggled.Remove(source.Id);
		else
			this._toggled[source.Id] = on;

		this.Version++;
	}

	/// <summary>Turns a source off when on, on when off.</summary>
	public void Toggle(BuffSource source) => this.Set(source, !this.IsOn(source));

	/// <summary>Picks the scroll whose buff the tab offers; its buff starts off.</summary>
	public void PickScroll(RecordId? scroll)
	{
		if (Equals(scroll, this.Scroll))
			return;

		this.Scroll = scroll;
		this.Version++;
	}
}
