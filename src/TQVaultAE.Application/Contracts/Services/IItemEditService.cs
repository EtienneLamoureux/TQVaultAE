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

	/// <summary>Links <paramref name="item"/> to a mod, or to none, and reads it again with that mod's data.</summary>
	/// <param name="sack">The sack holding the item.</param>
	/// <param name="item">The item to change.</param>
	/// <param name="modName">The mod's name (<c>InstalledMod.Name</c>); null for the base game.</param>
	/// <returns><see langword="true"/> when the link changed (the item and the sack are then marked as modified).</returns>
	bool ChangeMod(SackCollection sack, Item item, string? modName);

	/// <summary>Links a vault to a mod, or to none: its items without a mod of their own, or of the vault's previous mod,
	/// follow it and are read again with that mod's data.</summary>
	/// <param name="vault">The vault.</param>
	/// <param name="modName">The mod's name (<c>InstalledMod.Name</c>); null for the base game.</param>
	/// <returns>The items read again; empty when the link did not change.</returns>
	IReadOnlyList<Item> ChangeVaultMod(PlayerCollection vault, string? modName);
}
