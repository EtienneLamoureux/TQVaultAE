using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

/// <summary>
/// The items of a sack at their place, and whether the sack was modified: what a rearrangement that fails puts back.
/// </summary>
internal sealed class SackSnapshot
{
	private readonly SackCollection _sack;
	private readonly List<(Item Item, int X, int Y)> _items;
	private readonly bool _wasModified;

	private SackSnapshot(SackCollection sack)
	{
		this._sack = sack;
		this._items = [.. sack.Select(item => (item, item.PositionX, item.PositionY))];
		this._wasModified = sack.IsModified;
	}

	public static SackSnapshot Take(SackCollection sack) => new(sack);

	/// <summary>
	/// Puts the sack back as it was: the items themselves, not copies, since the search and the panels know them, and
	/// changes made before are still to be saved.
	/// </summary>
	public void Restore()
	{
		this._sack.EmptySack();
		foreach (var (item, x, y) in this._items)
		{
			item.PositionX = x;
			item.PositionY = y;
			this._sack.AddItem(item);
		}

		this._sack.IsModified = this._wasModified;
	}
}
