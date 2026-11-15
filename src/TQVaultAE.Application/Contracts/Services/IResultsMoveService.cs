using System.Drawing;
using TQVaultAE.Application.Results;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Application.Contracts.Services;

/// <summary>
/// Moves the items of search results (a selection of the results window, or all of them) to a stash or a vault bag, and
/// puts them back (undo). An item is found where the search placed it (<see cref="ContainerPlace"/>): a vault bag, a
/// character's bag, equipment or own stash, a shared stash, or a loadout. Items are placed one after the other at the
/// first free cells, as the auto-move places one; the ones without room, of a read-only character, or that the bag does
/// not take stay where they are and are reported. The sacks changed are marked modified: the moves are saved like any
/// user move. The loadouts file is written at once, as every loadout change.
/// </summary>
public interface IResultsMoveService
{
	/// <summary>The target of a stash, or null without one.</summary>
	/// <param name="stash">The transfer stash, the character's own stash or the relic vault, as the stash panel shows them.</param>
	/// <param name="stashType">Its kind.</param>
	ResultsMoveTarget? StashTarget(Stash? stash, StashType stashType);

	/// <summary>The target of a vault bag, or null when the vault has no such bag.</summary>
	/// <param name="vault">The vault.</param>
	/// <param name="bag">The bag number, 0-based.</param>
	/// <param name="bagSize">The size of a vault bag in cells.</param>
	ResultsMoveTarget? VaultTarget(PlayerCollection vault, int bag, Size bagSize);

	/// <summary>Whether a stash takes every item (the relic vault takes relics, charms, artifacts and formulae only).</summary>
	/// <param name="items">The items.</param>
	/// <param name="stashType">The stash.</param>
	bool Accepts(IEnumerable<Item> items, StashType stashType);

	/// <summary>
	/// Moves the items to the target, each at the first free cells, skipping and reporting the others; then the items
	/// moved take the target's map, as items put there by <i>Move To</i>.
	/// </summary>
	/// <param name="items">The items, moved in this order.</param>
	/// <param name="target">The bag.</param>
	/// <returns>What moved and what did not; its <see cref="ResultsMoveOutcome.Batch"/> undoes it.</returns>
	ResultsMoveOutcome Move(IReadOnlyList<Item> items, ResultsMoveTarget target);

	/// <summary>
	/// Puts the items of a move back where they were: their cell when it is still free (else the first free cells of their
	/// bag), their equipment slot when it is still empty, their loadout slot. An item no longer in the target bag, or whose
	/// place is gone, read-only or full, stays and is reported.
	/// </summary>
	/// <param name="batch">The move.</param>
	ResultsMoveOutcome Undo(ResultsMoveBatch batch);
}
