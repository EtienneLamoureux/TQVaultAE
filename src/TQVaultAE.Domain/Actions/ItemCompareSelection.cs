using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Actions;

/// <summary>
/// The "compare selection" mode of the item panels: the item menu's Compare entry picks the first item, then the next
/// left click on an item, in any panel, picks the second one and ends the mode. One instance is shared by every panel.
/// </summary>
public sealed class ItemCompareSelection
{
	/// <summary>The first item, while the mode is on; null otherwise.</summary>
	public Item? First { get; private set; }

	/// <summary>The mode is on: a left click on an item picks the second item instead of taking it.</summary>
	public bool IsActive => this.First is not null;

	/// <summary>Raised when the mode starts or ends, so that the panels redraw the first item and change the cursor.</summary>
	public event EventHandler? Changed;

	/// <summary>Starts the mode with <paramref name="first"/> as the first item (again with another item if already on).</summary>
	public void Start(Item first)
	{
		ArgumentNullException.ThrowIfNull(first);
		this.First = first;
		this.Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Ends the mode without comparing; nothing happens when it is off.</summary>
	/// <returns><see langword="true"/> when the mode was on.</returns>
	public bool Cancel()
	{
		if (!this.IsActive)
			return false;

		this.First = null;
		this.Changed?.Invoke(this, EventArgs.Empty);
		return true;
	}

	/// <summary>
	/// A left click on <paramref name="item"/> while the mode is on: another item ends the mode and gives the pair to
	/// compare; the first item itself ends the mode without comparing; an empty cell (null) keeps the mode on.
	/// </summary>
	/// <returns>The two items to compare, or null.</returns>
	public (Item First, Item Second)? Pick(Item? item)
	{
		if (this.First is not { } first || item is null)
			return null;

		this.Cancel();
		return ReferenceEquals(first, item) ? null : (first, item);
	}
}
