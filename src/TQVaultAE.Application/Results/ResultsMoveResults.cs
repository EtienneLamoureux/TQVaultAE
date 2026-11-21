using System.Drawing;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Results;

/// <summary>
/// A bag the results window moves items to: a stash or a vault bag, with what its items inherit.
/// </summary>
/// <param name="Sack">The bag.</param>
/// <param name="SackSize">Its size in cells.</param>
/// <param name="Place">The place its items get (the search follows them).</param>
/// <param name="ContainerMod">The map its items inherit: the vault's mod, or the shown character's map for a stash; null
/// for the base game.</param>
public sealed record ResultsMoveTarget(SackCollection Sack, Size SackSize, ContainerPlace Place, string? ContainerMod);

/// <summary>Why the results window did not move an item, or did not put it back.</summary>
public enum ResultsMoveSkip
{
	/// <summary>No free cells for it in the bag.</summary>
	NoRoom,

	/// <summary>It belongs to a read-only character (its bags, equipment, stash or loadouts).</summary>
	ReadOnly,

	/// <summary>It is worn and the <c>PlayerReadonly</c> setting locks the equipment.</summary>
	EquipmentLocked,

	/// <summary>The bag does not take this kind of item (the relic vault).</summary>
	NotAccepted,

	/// <summary>It is no longer where the search found it (moved, or its container read again).</summary>
	NotFound,

	/// <summary>It is already in the bag.</summary>
	AlreadyThere,

	/// <summary>The bag is a read-only character's (its own stash).</summary>
	TargetReadOnly,

	/// <summary>Its loadouts file could not be written.</summary>
	LoadoutNotWritten,
}

/// <summary>An item the results window did not move or put back, and why.</summary>
public readonly record struct ResultsMoveSkipped(Item Item, ResultsMoveSkip Reason);

/// <summary>
/// Where a moved item was, so an undo puts it back: its container, its cell or equipment slot or loadout slot, its place
/// and the map it was read with.
/// </summary>
public sealed class ResultsMoveOrigin
{
	/// <summary>The item moved.</summary>
	public required Item Item { get; init; }

	/// <summary>The bag it was in; null for a loadout piece.</summary>
	public SackCollection? Sack { get; init; }

	/// <summary>Its equipment slot when it was worn; -1 otherwise.</summary>
	public int EquipmentSlot { get; init; } = -1;

	/// <summary>The loadouts it was a piece of; null otherwise.</summary>
	public LoadoutSet? Loadouts { get; init; }

	/// <summary>The loadout and slot it was a piece of; null otherwise.</summary>
	public LoadoutPieceSlot? LoadoutSlot { get; init; }

	/// <summary>Its cell (or equipment cell).</summary>
	public Point Cell { get; init; }

	/// <summary>Its place, as the search showed it.</summary>
	public required ContainerPlace Place { get; init; }

	/// <summary>The map it was read with (<see cref="Item.ContainerMod"/>).</summary>
	public string? ContainerMod { get; init; }

	/// <summary>Whether a container had given it a map (<see cref="Item.HasContainerMod"/>).</summary>
	public bool HasContainerMod { get; init; }

	/// <summary>Whether it was shown 1 x 1 for want of room (<see cref="Item.LacksRoom"/>).</summary>
	public bool LacksRoom { get; init; }

	/// <summary>The character owning its equipment, when it was worn: its gear bonuses are summed again.</summary>
	public PlayerCollection? Wearer { get; init; }
}

/// <summary>A slot of a loadout.</summary>
/// <param name="Loadout">The loadout's name.</param>
/// <param name="Slot">The equipment slot.</param>
public readonly record struct LoadoutPieceSlot(string Loadout, int Slot);

/// <summary>A piece put back in a loadout slot.</summary>
/// <param name="Slot">The loadout and its slot.</param>
/// <param name="Piece">The piece.</param>
public readonly record struct LoadoutPiecePlacement(LoadoutPieceSlot Slot, Item Piece);

/// <summary>One move of the results window: the bag the items went to and where each came from, kept to undo it.</summary>
/// <param name="Target">The bag.</param>
/// <param name="Origins">Where each moved item came from, in the order moved.</param>
public sealed record ResultsMoveBatch(ResultsMoveTarget Target, IReadOnlyList<ResultsMoveOrigin> Origins);

/// <summary>
/// What a move or an undo of the results window did.
/// </summary>
/// <param name="Done">The items moved (or put back).</param>
/// <param name="Skipped">The items left where they were, with the reason.</param>
/// <param name="ReadAgain">The items read again with another map: their tool tips and search names are made again.</param>
/// <param name="NoRoom">The items shown 1 x 1 for want of room once read with the new map.</param>
/// <param name="Wearers">The characters whose equipment changed: their gear bonuses are summed again.</param>
/// <param name="LoadoutsChanged">The loadouts whose pieces changed: the loadout bar shows them again.</param>
/// <param name="Batch">The move, to undo it; null for an undo or when nothing moved.</param>
public sealed record ResultsMoveOutcome(
	IReadOnlyList<Item> Done,
	IReadOnlyList<ResultsMoveSkipped> Skipped,
	IReadOnlyList<Item> ReadAgain,
	IReadOnlyList<Item> NoRoom,
	IReadOnlyList<PlayerCollection> Wearers,
	IReadOnlyList<LoadoutSet> LoadoutsChanged,
	ResultsMoveBatch? Batch)
{
	/// <summary>The sacks the move or undo changed, for their bag button tool tips.</summary>
	public IReadOnlyList<SackCollection> ChangedSacks { get; init; } = [];
}
