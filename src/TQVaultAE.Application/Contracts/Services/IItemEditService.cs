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
	/// is removed with the slot, and the sack is marked as modified.
	/// </remarks>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item holding the relic.</param>
	/// <param name="secondSlot"><see langword="true"/> for the second slot.</param>
	/// <returns>The relic or charm to place, or <see langword="null"/> when the slot held something else.</returns>
	Item? RemoveRelic(SackCollection sack, Item item, bool secondSlot);

	/// <summary>Splits a stack: <paramref name="item"/> keeps one, the rest is returned.</summary>
	/// <param name="item">A stack of more than one item.</param>
	/// <returns>The rest of the stack, or <see langword="null"/> when there is nothing to split.</returns>
	Item? Split(Item item);

	/// <summary>Copies <paramref name="item"/> with a new seed, so the copy rolls its own stats.</summary>
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

	/// <summary>Gives <paramref name="item"/> a mod, or the base game, as its own map, and reads it again with that
	/// data.</summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item to change.</param>
	/// <param name="modName">The mod's name (<c>InstalledMod.Name</c>); null for the base game.</param>
	/// <param name="fits">Whether the item, read again (it may take another size), still has room in its container; it
	/// may move the item to a free spot. When it answers no, the choice is refused: the item keeps its map. Null when
	/// the size cannot matter.</param>
	/// <returns><see cref="ModChoiceOutcome.Changed"/> when the item's map changed (the item and the sack are then
	/// marked as modified).</returns>
	ModChoiceOutcome ChangeMod(SackCollection sack, Item item, string? modName, Func<Item, bool>? fits = null);

	/// <summary>Links a vault to a mod, or to none: its items inheriting its map take the new one and are read again
	/// with that data; items with an own map keep it.</summary>
	/// <param name="vault">The vault.</param>
	/// <param name="modName">The mod's name (<c>InstalledMod.Name</c>); null for the base game.</param>
	/// <param name="fits">Whether an item of a sack, read again (it may take another size), still has room in the
	/// vault; it may move the item to a free spot, of its bag or of another. An item it refuses keeps its previous map,
	/// as its own. Null when the size cannot matter.</param>
	/// <returns>The items read again, and those that kept their map for want of room.</returns>
	VaultModChange ChangeVaultMod(PlayerCollection vault, string? modName, Func<SackCollection, Item, bool>? fits = null);

	/// <summary>An item just put in a container takes its map there (MODLINK.md): an own map goes with it; an inherited
	/// one becomes the item's own when it leaves a character for a vault, and else becomes the new container's. Read
	/// with a new map, the item is read again.</summary>
	/// <param name="item">The item just placed.</param>
	/// <param name="inVault">Whether a vault now holds it (else a character or a game stash).</param>
	/// <param name="containerMod">The map of the new container: the vault's mod, or the character's; null for the base
	/// game.</param>
	/// <param name="fits">Whether the item, read again (its size may change), fits where it lies or at another free
	/// spot, where it moves it. When not, the item keeps its previous map, as its own.</param>
	/// <returns>What happened to the item's data.</returns>
	SettleOutcome SettleIn(Item item, bool inVault, string? containerMod, Func<Item, bool> fits);

	/// <summary>Whether an item about to be put in a container would have room there once it takes its map
	/// (<see cref="SettleIn"/>): read with a new map it may take another size. The item is left as it was, with its
	/// data and position.</summary>
	/// <param name="item">The item about to be placed.</param>
	/// <param name="inVault">Whether a vault would hold it.</param>
	/// <param name="containerMod">The map of the container; null for the base game.</param>
	/// <param name="fits">Whether the item, at the size of its new map, has room; it must not keep a move.</param>
	/// <returns><see langword="true"/> when the item keeps its data there, or has room with the new ones.</returns>
	bool HasRoomOnceSettled(Item item, bool inVault, string? containerMod, Func<Item, bool> fits);

	/// <summary>
	/// Items put in a bag without a drop (Move To, import, paste) take their map there like dropped ones
	/// (<see cref="SettleIn"/>). An item that no longer fits where it lies once read again (it takes its real size) moves
	/// to a free spot of the bag, else of another bag of the container (<paramref name="elsewhere"/>); without one, it
	/// keeps its previous map.
	/// </summary>
	/// <param name="sack">The bag holding the items.</param>
	/// <param name="sackSize">The size of that bag in cells.</param>
	/// <param name="items">The items just put there.</param>
	/// <param name="inVault">Whether the bag is a vault's.</param>
	/// <param name="containerMod">The map of the container: the vault's mod, or the character's.</param>
	/// <param name="elsewhere">Moves an item that has no room in its bag to a free spot of another bag of the container;
	/// false when there is none. Null for none.</param>
	/// <returns>The items read again, and those that kept their map for want of room.</returns>
	SettleResult SettleAll(SackCollection sack, System.Drawing.Size sackSize, IEnumerable<Item> items, bool inVault, string? containerMod, Func<SackCollection, Item, bool>? elsewhere = null);
}

/// <summary>What <see cref="IItemEditService.SettleIn"/> did to an item's data.</summary>
public enum SettleOutcome
{
	/// <summary>The item keeps the data it was read with.</summary>
	Unchanged,

	/// <summary>The item took a new map and was read again with it.</summary>
	ReadAgain,

	/// <summary>Read with the new map, the item had no room: it kept its previous map, as its own.</summary>
	KeptOwnMap,
}

/// <summary>What <see cref="IItemEditService.ChangeMod"/> did with the map chosen for an item.</summary>
public enum ModChoiceOutcome
{
	/// <summary>The item already had that map as its own.</summary>
	Unchanged,

	/// <summary>The item took the chosen map as its own.</summary>
	Changed,

	/// <summary>Read with the chosen map, the item would have had no room in its container: it keeps its map.</summary>
	NoRoom,
}

/// <summary>The items <see cref="IItemEditService.SettleAll"/> read again, and those that kept their map.</summary>
public sealed record SettleResult(IReadOnlyList<Item> ReadAgain, IReadOnlyList<Item> Kept);

/// <summary>The items <see cref="IItemEditService.ChangeVaultMod"/> read again, and those that kept their map.</summary>
public sealed record VaultModChange(IReadOnlyList<Item> Changed, IReadOnlyList<Item> Kept)
{
	/// <summary>The link did not change.</summary>
	public static readonly VaultModChange None = new([], []);
}
