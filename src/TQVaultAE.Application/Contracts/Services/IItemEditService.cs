using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// The item edits offered by the sack context menu: affixes, completion bonuses, relics, seed, copies.
/// </summary>
/// <remarks>
/// The edits that change an item in place also mark the item and its sack as modified. The edits that make a new item
/// (copy, split, craft, removed relic) return it for the caller to drag; they never place it in a sack.
/// Confirmations, repaint and cache invalidation stay with the caller.
/// </remarks>
public interface IItemEditService
{
	/// <summary>Replaces the prefix of <paramref name="item"/>.</summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item to change.</param>
	/// <param name="affixId">The new prefix record.</param>
	/// <returns><see langword="true"/> when the prefix changed; <see langword="false"/> when it already was this one.</returns>
	bool ChangePrefix(SackCollection sack, Item item, RecordId affixId);

	/// <summary>Replaces the suffix of <paramref name="item"/>.</summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item to change.</param>
	/// <param name="affixId">The new suffix record.</param>
	/// <returns><see langword="true"/> when the suffix changed; <see langword="false"/> when it already was this one.</returns>
	bool ChangeSuffix(SackCollection sack, Item item, RecordId affixId);

	/// <summary>Removes the prefix of <paramref name="item"/>.</summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item to change.</param>
	void RemovePrefix(SackCollection sack, Item item);

	/// <summary>Removes the suffix of <paramref name="item"/>.</summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item to change.</param>
	void RemoveSuffix(SackCollection sack, Item item);

	/// <summary>
	/// Replaces a completion bonus: the bonus of a completed relic, charm or artifact, or the bonus of a relic socketed in
	/// the item (first or second slot).
	/// </summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item to change.</param>
	/// <param name="bonusId">The new bonus record.</param>
	/// <param name="secondRelic"><see langword="true"/> for the bonus of the relic in the second slot.</param>
	/// <returns><see langword="true"/> when the bonus changed; <see langword="false"/> when it already was this one.</returns>
	bool ChangeCompletionBonus(SackCollection sack, Item item, RecordId bonusId, bool secondRelic = false);

	/// <summary>Completes a relic or charm and rolls its completion bonus.</summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">An incomplete relic or charm.</param>
	void CompleteRelic(SackCollection sack, Item item);

	/// <summary>Crafts the artifact of a formula, with a rolled completion bonus.</summary>
	/// <param name="formula">The formula; it is not changed.</param>
	/// <returns>The new artifact, or <see langword="null"/> when the formula gives none.</returns>
	Item? CraftArtifact(Item formula);

	/// <summary>Takes the relic or charm out of a slot of <paramref name="item"/>.</summary>
	/// <remarks>
	/// God mode in the Forge can socket anything (a weapon, an affix...). Only relics and charms come back; anything else
	/// is removed with the slot. The sack is marked as modified either way.
	/// </remarks>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item holding the relic.</param>
	/// <param name="secondSlot"><see langword="true"/> for the second slot.</param>
	/// <returns>The relic or charm to place, or <see langword="null"/> when the slot held something else.</returns>
	Item? RemoveRelic(SackCollection sack, Item item, bool secondSlot);

	/// <summary>Splits a stack: <paramref name="item"/> keeps one, the rest is returned.</summary>
	/// <param name="sack">The sack holding the item, marked as modified when the stack was split.</param>
	/// <param name="item">A stack of more than one item.</param>
	/// <returns>The rest of the stack, or <see langword="null"/> when there is nothing to split.</returns>
	Item? Split(SackCollection sack, Item item);

	/// <summary>
	/// Copies <paramref name="item"/> with a new seed, so the copy rolls its own stats. Like <see cref="Duplicate"/>,
	/// <see cref="NewSetMember"/> and <see cref="CraftArtifact"/>, it leaves the source item and its sack unchanged.
	/// </summary>
	/// <param name="item">The item to copy.</param>
	Item Copy(Item item);

	/// <summary>Copies <paramref name="item"/> with the same seed: an exact duplicate.</summary>
	/// <param name="item">The item to copy.</param>
	Item Duplicate(Item item);

	/// <summary>Makes another member of the set of <paramref name="setItem"/>, with its record data read.</summary>
	/// <param name="setItem">The set item it is made from: its block data are kept.</param>
	/// <param name="memberId">The record of the set member.</param>
	Item NewSetMember(Item setItem, RecordId memberId);

	/// <summary>Gives <paramref name="item"/> a new random seed.</summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item to change.</param>
	/// <returns><see langword="true"/> when the seed changed (the sack is then marked as modified).</returns>
	bool ForceSeed(SackCollection sack, Item item);

	/// <summary>Gives <paramref name="item"/> the seed typed in the seed dialog.</summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item to change.</param>
	/// <param name="seed">The new seed.</param>
	/// <returns><see langword="true"/> when the seed changed (the item and the sack are then marked as modified);
	/// <see langword="false"/> when it is the same seed or not a valid one (see <see cref="Item.IsValidSeed"/>).</returns>
	bool SetSeed(SackCollection sack, Item item, int seed);

	/// <summary>Links a vault to a mod, or to none: every item of the vault takes the new map and is read again with
	/// that data (its mod, <see cref="Item.Mod"/>, follows).</summary>
	/// <param name="vault">The vault.</param>
	/// <param name="modName">The mod's identity (<c>InstalledMod.Id</c>); null for the base game.</param>
	/// <param name="fits">Whether an item of a sack, read again (it may take another size), still has room in the
	/// vault; it may move the item to a free spot, of its bag or of another. An item it refuses is shown 1 x 1
	/// (<see cref="Item.LacksRoom"/>). Null when the size cannot matter.</param>
	/// <returns>The items read again, and those shown 1 x 1 for want of room.</returns>
	VaultModChange ChangeVaultMod(PlayerCollection vault, string? modName, Func<SackCollection, Item, bool>? fits = null);

	/// <summary>An item just put in a container takes the container's map (MODLINK.md) and, with a new map, is read
	/// again with it. An item shown 1 x 1 (<see cref="Item.LacksRoom"/>) gets its real size back where it fits.</summary>
	/// <param name="item">The item just placed.</param>
	/// <param name="containerMod">The map of the new container: the vault's mod, the character's, the shared stash's;
	/// null for the base game.</param>
	/// <param name="fits">Whether the item, at its size, fits where it lies or at another free spot, where it moves it.
	/// When not, an item read with a new map goes back to its previous map; an item that kept its map stays 1 x 1.</param>
	/// <returns>What happened to the item's data.</returns>
	SettleOutcome SettleIn(Item item, string? containerMod, Func<Item, bool> fits);

	/// <summary>Whether an item about to be put in a container would have room there once it takes its map
	/// (<see cref="SettleIn"/>): read with a new map it may take another size. The item is left as it was, with its
	/// data and position.</summary>
	/// <param name="item">The item about to be placed.</param>
	/// <param name="containerMod">The map of the container; null for the base game.</param>
	/// <param name="fits">Whether the item, at the size of its new map, has room; it must not keep a move.</param>
	/// <returns><see langword="true"/> when the item keeps its map there, or has room with the new one.</returns>
	bool HasRoomOnceSettled(Item item, string? containerMod, Func<Item, bool> fits);

	/// <summary>
	/// Items put in a bag without a drop (Move To, import, paste) take its container's map like dropped ones
	/// (<see cref="SettleIn"/>). An item that no longer fits where it lies once read again moves to a free spot of the
	/// bag, else of another bag of the container (<paramref name="elsewhere"/>); without one, it is shown 1 x 1
	/// (<see cref="Item.LacksRoom"/>).
	/// </summary>
	/// <param name="sack">The bag holding the items.</param>
	/// <param name="sackSize">The size of that bag in cells.</param>
	/// <param name="items">The items just put there.</param>
	/// <param name="containerMod">The map of the container: the vault's mod, the character's, the shared stash's.</param>
	/// <param name="elsewhere">Moves an item that has no room in its bag to a free spot of another bag of the container;
	/// false when there is none. Null for none.</param>
	/// <returns>The items read again, and those shown 1 x 1 for want of room.</returns>
	SettleResult SettleAll(SackCollection sack, System.Drawing.Size sackSize, IEnumerable<Item> items, string? containerMod, Func<SackCollection, Item, bool>? elsewhere = null);
}

/// <summary>What <see cref="IItemEditService.SettleIn"/> did to an item's data.</summary>
public enum SettleOutcome
{
	/// <summary>The item keeps the data it was read with, and its size.</summary>
	Unchanged,

	/// <summary>The item took a new map and was read again with it, or got its real size back: its pictures and
	/// tool tip change.</summary>
	ReadAgain,

	/// <summary>Read with the new map, the item had no room: it was put back on its previous map, and must not stay
	/// there (the drop is refused).</summary>
	NoRoom,
}

/// <summary>The items <see cref="IItemEditService.SettleAll"/> read again, and those shown 1 x 1 for want of room.</summary>
public sealed record SettleResult(IReadOnlyList<Item> ReadAgain, IReadOnlyList<Item> NoRoom);

/// <summary>The items <see cref="IItemEditService.ChangeVaultMod"/> read again, and those shown 1 x 1 for want of
/// room.</summary>
public sealed record VaultModChange(IReadOnlyList<Item> Changed, IReadOnlyList<Item> NoRoom)
{
	/// <summary>The link did not change.</summary>
	public static readonly VaultModChange None = new([], []);
}
