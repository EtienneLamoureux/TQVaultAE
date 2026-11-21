using System.Drawing;
using Microsoft.Extensions.Logging;
using TQVaultAE.Application.Results;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Services;

public partial class LoadoutService
{
	/// <inheritdoc/>
	public LoadoutEmptyResult Empty(LoadoutSet loadouts, string name, SackCollection bag, Size bagSize, ContainerPlace bagPlace)
	{
		if (loadouts.ReadFailed || loadouts.Find(name) is not { } loadout || bag.IsReadOnly)
			return new LoadoutEmptyResult(LoadoutEmptyOutcome.Refused);

		var pieces = Enumerable.Range(0, loadout.Pieces.Count).Select(slot => PieceAt(loadout.Pieces, slot)).OfType<Item>().ToList();
		if (pieces.Count == 0)
			return new LoadoutEmptyResult(LoadoutEmptyOutcome.NothingToEmpty);

		// Piece by piece, in slot order, at the first free spot as an auto-move places it: one without room (or that the
		// bag does not take) stays in the loadout.
		var moved = pieces.Where(item => this.ItemMovementService.TryPlace(item, bag, bagSize, bagPlace)).ToList();
		int left = pieces.Count - moved.Count;
		if (moved.Count == 0)
			return new LoadoutEmptyResult(LoadoutEmptyOutcome.NoRoom, Left: left);

		var taken = new HashSet<Item>(moved, ReferenceEqualityComparer.Instance);
		var emptied = loadout with { Pieces = [.. loadout.Pieces.Select(p => p is not null && taken.Contains(p) ? null : p)] };
		loadouts.Loadouts = [.. loadouts.Loadouts.Select(l => l == loadout ? emptied : l)];
		loadouts.IsModified = true;

		// The pieces keep their search entries, now at the bag's place; the loadout's entries are made again without them.
		this.Changed(loadouts);

		this.Log.LogInformation("Loadout {Name} of {Character} emptied: {Moved} piece(s) moved to {Bag} sack {SackNumber}, {Left} left for want of room",
			loadout.Name, loadouts.CharacterName, moved.Count, bagPlace.Name, bagPlace.SackNumber, left);
		return new LoadoutEmptyResult(left == 0 ? LoadoutEmptyOutcome.Emptied : LoadoutEmptyOutcome.PartlyEmptied, moved.Count, left) { Moved = moved };
	}
}
