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

		// All or nothing, as a load storing the pieces it takes off: then they go at the spots the room check found.
		if (!this.HasRoom(bag, bagSize, pieces))
			return new LoadoutEmptyResult(LoadoutEmptyOutcome.NoRoom, pieces.Count);

		var moved = new List<Item>();
		foreach (var item in pieces)
		{
			if (this.ItemMovementService.TryPlace(item, bag, bagSize, bagPlace))
				moved.Add(item);
			else
				this.Log.LogError("Loadout {Name}: no room for {Item} after the room check", loadout.Name, item.BaseItemId);
		}

		// A piece not placed stays in the loadout: nothing is lost.
		var taken = new HashSet<Item>(moved, ReferenceEqualityComparer.Instance);
		var emptied = loadout with { Pieces = [.. loadout.Pieces.Select(p => p is not null && taken.Contains(p) ? null : p)] };
		loadouts.Loadouts = [.. loadouts.Loadouts.Select(l => l == loadout ? emptied : l)];
		loadouts.IsModified = true;

		// The pieces keep their search entries, now at the bag's place; the loadout's entries are made again without them.
		this.Changed(loadouts);

		this.Log.LogInformation("Loadout {Name} of {Character} emptied: {Moved} piece(s) moved to {Bag} sack {SackNumber}",
			loadout.Name, loadouts.CharacterName, moved.Count, bagPlace.Name, bagPlace.SackNumber);
		return new LoadoutEmptyResult(LoadoutEmptyOutcome.Emptied, moved.Count) { Moved = moved };
	}
}
