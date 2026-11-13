using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Actions;

/// <summary>What loading a loadout does to one equipment slot.</summary>
public enum LoadoutSlotAction
{
	/// <summary>The piece worn in the slot is the loadout's: it stays.</summary>
	Keep,

	/// <summary>The loadout's piece is worn in another slot (<see cref="LoadoutSlotStep.FromSlot"/>): it moves here.</summary>
	Move,

	/// <summary>The loadout's piece is not worn: a copy of it goes in the slot.</summary>
	Copy,

	/// <summary>The loadout has nothing in the slot: it is left empty.</summary>
	Clear,
}

/// <summary>The step of one slot.</summary>
/// <param name="Slot">The equipment slot.</param>
/// <param name="Action">What happens to it.</param>
/// <param name="FromSlot">For <see cref="LoadoutSlotAction.Move"/>, the slot the piece is worn in now; else -1.</param>
public readonly record struct LoadoutSlotStep(int Slot, LoadoutSlotAction Action, int FromSlot = -1);

/// <summary>
/// How the worn equipment becomes a loadout's: a worn piece identical to a loadout piece (<see cref="ItemKey"/>)
/// stays worn, in its slot or moved to the loadout's slot for it (rings or weapon sets swapped); the loadout pieces not
/// worn are copied in; the worn pieces the loadout does not use leave the equipment.
/// </summary>
public sealed class LoadoutPlan
{
	private LoadoutPlan(IReadOnlyList<LoadoutSlotStep> steps, IReadOnlyList<int> leavingSlots)
	{
		this.Steps = steps;
		this.LeavingSlots = leavingSlots;
	}

	/// <summary>One step per equipment slot, in slot order.</summary>
	public IReadOnlyList<LoadoutSlotStep> Steps { get; }

	/// <summary>The slots whose worn piece leaves the equipment (to be stored elsewhere), in slot order.</summary>
	public IReadOnlyList<int> LeavingSlots { get; }

	/// <summary>True when the worn equipment already is the loadout's, piece for piece and slot for slot.</summary>
	public bool ChangesNothing => this.LeavingSlots.Count == 0
		&& this.Steps.All(step => step.Action is LoadoutSlotAction.Keep or LoadoutSlotAction.Clear);

	/// <summary>Number of copies of loadout pieces put in.</summary>
	public int CopyCount => this.Steps.Count(step => step.Action == LoadoutSlotAction.Copy);

	/// <summary>Number of worn pieces that change slot.</summary>
	public int MoveCount => this.Steps.Count(step => step.Action == LoadoutSlotAction.Move);

	/// <summary>
	/// Plans the load of a loadout over the worn equipment.
	/// </summary>
	/// <param name="worn">The worn equipment, one item per slot (an empty item or <see langword="null"/> for an empty slot).</param>
	/// <param name="pieces">The loadout's pieces, one per slot (<see langword="null"/> or empty for an empty slot); slots
	/// beyond the equipment's are ignored, missing ones are empty.</param>
	public static LoadoutPlan Make(IReadOnlyList<Item?> worn, IReadOnlyList<Item?> pieces)
	{
		ArgumentNullException.ThrowIfNull(worn);
		ArgumentNullException.ThrowIfNull(pieces);

		int slots = worn.Count;
		Item? PieceOf(int slot) => slot < pieces.Count ? pieces[slot] : null;

		var steps = new LoadoutSlotStep?[slots];
		var used = new bool[slots];

		// First the pieces already in their slot, so that a piece worn twice is not taken from the slot that keeps it.
		for (int slot = 0; slot < slots; slot++)
		{
			var piece = PieceOf(slot);
			if (ItemKey.IsEmptyItem(piece))
				steps[slot] = new LoadoutSlotStep(slot, ItemKey.IsEmptyItem(worn[slot]) ? LoadoutSlotAction.Keep : LoadoutSlotAction.Clear);
			else if (ItemIdentityComparer.Instance.Equals(worn[slot], piece))
			{
				steps[slot] = new LoadoutSlotStep(slot, LoadoutSlotAction.Keep);
				used[slot] = true;
			}
		}

		// Then the pieces worn elsewhere, else copies.
		for (int slot = 0; slot < slots; slot++)
		{
			if (steps[slot] is not null)
				continue;

			var piece = PieceOf(slot);
			int from = -1;
			for (int other = 0; other < slots; other++)
			{
				if (!used[other] && ItemIdentityComparer.Instance.Equals(worn[other], piece))
				{
					from = other;
					break;
				}
			}

			if (from == -1)
				steps[slot] = new LoadoutSlotStep(slot, LoadoutSlotAction.Copy);
			else
			{
				used[from] = true;
				steps[slot] = new LoadoutSlotStep(slot, LoadoutSlotAction.Move, from);
			}
		}

		var leaving = Enumerable.Range(0, slots).Where(slot => !used[slot] && !ItemKey.IsEmptyItem(worn[slot])).ToList();
		return new LoadoutPlan([.. steps.Select(step => step!.Value)], leaving);
	}
}
