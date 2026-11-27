using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Forge;

namespace TQVaultAE.Domain.Actions;

/// <summary>What the second item of an <see cref="ItemPairSelection"/> is picked for.</summary>
public enum ItemPairPurpose
{
	/// <summary>Compare the two items (the item menu's Compare).</summary>
	Compare,

	/// <summary>Socket the first item, a relic or charm, into the second one by the game's rules (<see cref="ForgeMode.Game"/>).</summary>
	SocketGameRules,

	/// <summary>Socket the first item, a relic or charm, into any slot of the second one (<see cref="ForgeMode.God"/>).</summary>
	SocketNoRules,
}

/// <summary>The two items of an <see cref="ItemPairSelection"/>, once the second one is picked.</summary>
/// <param name="First">The item the selection started with.</param>
/// <param name="Second">The item picked by the click.</param>
/// <param name="Purpose">What the pair is for.</param>
/// <param name="FirstSack">The sack holding <paramref name="First"/> when the selection started, when given.</param>
public sealed record ItemPair(Item First, Item Second, ItemPairPurpose Purpose, SackCollection? FirstSack);

/// <summary>
/// The "pick a second item" mode of the item panels: an item menu entry (Compare, Insert into item) picks the first
/// item, then the next left click on an item, in any panel, picks the second one and ends the mode. One instance is
/// shared by every panel.
/// </summary>
public sealed class ItemPairSelection
{
	/// <summary>The first item, while the mode is on; null otherwise.</summary>
	public Item? First { get; private set; }

	/// <summary>The sack holding <see cref="First"/>, when the mode was started with it.</summary>
	public SackCollection? FirstSack { get; private set; }

	/// <summary>What the second item is picked for, while the mode is on.</summary>
	public ItemPairPurpose Purpose { get; private set; }

	/// <summary>The mode is on: a left click on an item picks the second item instead of taking it.</summary>
	public bool IsActive => this.First is not null;

	/// <summary>Raised when the mode starts or ends, so that the panels redraw the first item and change the cursor.</summary>
	public event EventHandler? Changed;

	/// <summary>
	/// Starts the mode with <paramref name="first"/> as the first item (again with another item or purpose if already on).
	/// </summary>
	/// <param name="first">The first item.</param>
	/// <param name="purpose">What the second item is picked for.</param>
	/// <param name="firstSack">The sack holding <paramref name="first"/>, for a purpose that changes it.</param>
	public void Start(Item first, ItemPairPurpose purpose = ItemPairPurpose.Compare, SackCollection? firstSack = null)
	{
		ArgumentNullException.ThrowIfNull(first);
		this.First = first;
		this.FirstSack = firstSack;
		this.Purpose = purpose;
		this.Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Ends the mode without picking; nothing happens when it is off.</summary>
	/// <returns><see langword="true"/> when the mode was on.</returns>
	public bool Cancel()
	{
		if (!this.IsActive)
			return false;

		this.First = null;
		this.FirstSack = null;
		this.Purpose = ItemPairPurpose.Compare;
		this.Changed?.Invoke(this, EventArgs.Empty);
		return true;
	}

	/// <summary>
	/// A left click on <paramref name="item"/> while the mode is on: another item ends the mode and gives the pair; the
	/// first item itself ends the mode without a pair; an empty cell (null) keeps the mode on.
	/// </summary>
	/// <returns>The pair, or null.</returns>
	public ItemPair? Pick(Item? item)
	{
		if (this.First is not { } first || item is null)
			return null;

		var pair = new ItemPair(first, item, this.Purpose, this.FirstSack);
		this.Cancel();
		return ReferenceEquals(first, item) ? null : pair;
	}
}
