using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Actions;

/// <summary>Actions offered on the items of a sack, for example by its context menu.</summary>
[Flags]
public enum ItemActions
{
	None = 0,

	/// <summary>Delete the item under the cursor or the selection.</summary>
	Delete = 1 << 0,

	/// <summary>Take the relic or charm out of the first slot.</summary>
	RemoveRelic = 1 << 1,

	/// <summary>Take the relic or charm out of the second slot.</summary>
	RemoveRelic2 = 1 << 2,

	/// <summary>Split a stack in two.</summary>
	Split = 1 << 3,

	/// <summary>Copy the item to the drag cursor.</summary>
	Copy = 1 << 4,

	/// <summary>Put a copy of the item in the sack.</summary>
	Duplicate = 1 << 5,

	/// <summary>Move the item or the selection to another sack.</summary>
	MoveTo = 1 << 6,

	/// <summary>Edit the item: seed, affixes, completion bonuses, set pieces.</summary>
	Edit = 1 << 7,

	/// <summary>Show the item properties.</summary>
	Properties = 1 << 8,

	/// <summary>Clear the selection.</summary>
	ClearSelection = 1 << 9,

	/// <summary>Choose the item seed in a dialog. Part of <see cref="Edit"/>.</summary>
	Seed = 1 << 10,

	/// <summary>Give the item a new random seed. Part of <see cref="Edit"/>.</summary>
	ForceSeed = 1 << 11,

	/// <summary>Complete a relic or charm and roll its bonus. Part of <see cref="Edit"/>.</summary>
	CompleteRelic = 1 << 12,

	/// <summary>Craft the artifact of a formula. Part of <see cref="Edit"/>.</summary>
	CraftArtifact = 1 << 13,

	/// <summary>
	/// Compare the item with another one: starts the compare selection (<see cref="ItemCompareSelection"/>), where the
	/// next left click on an item, in any panel, picks the second item.
	/// </summary>
	Compare = 1 << 14,

	/// <summary>
	/// Link the item under the cursor to a mod, or to the base game ("Modded" menu). For that item even when others are
	/// selected.
	/// </summary>
	ChangeMod = 1 << 15,
}

/// <summary>
/// What the user points at and may do when asking for the actions of a sack.
/// </summary>
/// <param name="Item">Item under the cursor, or <see langword="null"/>.</param>
/// <param name="SelectionCount">Number of selected items.</param>
/// <param name="ItemIsTheSelection"><see langword="true"/> when the selection is <paramref name="Item"/> alone.</param>
/// <param name="ReadOnly">The sack cannot change, as the equipment of a read-only player.</param>
/// <param name="AllowItemEdit">The user enabled item editing.</param>
/// <param name="AllowItemCopy">The user enabled item copying.</param>
public readonly record struct ItemActionContext(
	Item? Item
	, int SelectionCount
	, bool ItemIsTheSelection
	, bool ReadOnly
	, bool AllowItemEdit
	, bool AllowItemCopy
);

/// <summary>
/// Where the "Move to" entries of a sack may send its items.
/// </summary>
/// <param name="Here">The panel of the sack.</param>
/// <param name="SackType">The type of the sack.</param>
/// <param name="MaxSacks">Number of bags of the panel.</param>
/// <param name="PlayerSackCount">Number of sacks of the current player, the main sack included.</param>
/// <param name="Panels">The panels that take moved items, in menu order; duplicates are ignored.</param>
/// <param name="SecondaryVaultShown">The secondary vault replaces the player and stash panels.</param>
/// <param name="PlayerIsImmortalThrone">Whether the current player is an Immortal Throne save; <see langword="null"/> without a player.</param>
/// <param name="Item">Item under the cursor, or <see langword="null"/> for a selection alone.</param>
/// <param name="CurrentSack">The sack shown, left out of the bags; -1 to use the sack of <paramref name="Item"/>.</param>
public readonly record struct MoveContext(
	AutoMoveLocation Here
	, SackType SackType
	, int MaxSacks
	, int PlayerSackCount
	, IEnumerable<AutoMoveLocation> Panels
	, bool SecondaryVaultShown
	, bool? PlayerIsImmortalThrone
	, Item? Item
	, int CurrentSack = -1
);

/// <summary>
/// A "Move to" entry: another bag of the same panel, or another panel.
/// </summary>
/// <param name="Location">The destination panel: the sack's own panel for a bag.</param>
/// <param name="BagIndex">The destination bag, or <see langword="null"/> for a whole panel.</param>
/// <param name="BagNumber">The number shown for the bag.</param>
/// <param name="IsMainPlayerSack">The bag is the main sack of the player, shown by its own name.</param>
public readonly record struct MoveDestination(
	AutoMoveLocation Location
	, int? BagIndex = null
	, int BagNumber = 0
	, bool IsMainPlayerSack = false
);

/// <summary>
/// Which <see cref="ItemActions"/> apply: the decisions of the sack context menu, apart from its drawing.
/// </summary>
public static class ItemActionRules
{
	/// <summary>
	/// Indicates whether the items of a sack of that type cannot change: with the <c>PlayerReadonly</c> setting on, the
	/// equipment of the player cannot be picked from, dropped on, moved or deleted. The player sacks and the other
	/// panels stay editable. This is the <see cref="ItemActionContext.ReadOnly"/> of a sack.
	/// </summary>
	/// <param name="sackType">The type of the sack.</param>
	/// <param name="playerReadonly">The <c>PlayerReadonly</c> user setting.</param>
	/// <returns><see langword="true"/> when the sack cannot change.</returns>
	public static bool IsReadOnly(SackType sackType, bool playerReadonly)
		=> playerReadonly && sackType == SackType.Equipment;

	/// <summary>The actions available in <paramref name="context"/>.</summary>
	public static ItemActions Available(in ItemActionContext context)
	{
		var item = context.Item;
		bool hasSelection = context.SelectionCount > 0;

		// Something to act on: the item under the cursor, the selection, or both.
		bool anyTarget = item is not null || hasSelection;

		// The item alone is the target: no selection, or a selection of that item only.
		bool singleItem = item is not null && (!hasSelection || context.ItemIsTheSelection);

		var actions = ItemActions.None;

		if (anyTarget && !context.ReadOnly)
			actions |= ItemActions.Delete | ItemActions.MoveTo;

		// A vault item only: a character's files cannot keep a choice, which the next read would lose.
		if (item is not null && !context.ReadOnly && item.InVault)
			actions |= ItemActions.ChangeMod;

		if (singleItem)
		{
			// Comparing only reads the items: read-only sacks too.
			actions |= ItemActions.Properties | ItemActions.Compare;

			if (context.AllowItemCopy)
				actions |= ItemActions.Copy | ItemActions.Duplicate;

			if (!context.ReadOnly)
			{
				if (item!.DoesStack && item.Number > 1)
					actions |= ItemActions.Split;

				if (context.AllowItemEdit)
				{
					actions |= ItemActions.Edit;

					if (item.HasSocketedSlot1)
						actions |= ItemActions.RemoveRelic;

					if (item.HasSocketedSlot2)
						actions |= ItemActions.RemoveRelic2;
				}
			}
		}

		if (hasSelection && !context.ReadOnly)
			actions |= ItemActions.ClearSelection;

		return actions;
	}

	/// <summary>
	/// The action of a key pressed over a sack, when <paramref name="context"/> allows it, as its context menu would:
	/// <c>c</c> copies, <c>d</c> duplicates and Backspace deletes the item under the cursor. The keys act on that item
	/// alone, never on the selection.
	/// </summary>
	/// <returns>The action to run, or <see cref="ItemActions.None"/>.</returns>
	public static ItemActions KeyAction(char key, in ItemActionContext context)
	{
		if (context.Item is null)
			return ItemActions.None;

		var action = key switch
		{
			'c' => ItemActions.Copy,
			'd' => ItemActions.Duplicate,
			'\b' => ItemActions.Delete,
			_ => ItemActions.None,
		};

		return action & Available(context with { SelectionCount = 0, ItemIsTheSelection = false });
	}

	/// <summary>
	/// Indicates whether <paramref name="item"/> can be equipped by a player of that game:
	/// a Titan Quest original player only takes the items of the base game.
	/// </summary>
	/// <param name="item">Item to check.</param>
	/// <param name="immortalThronePlayer"><see langword="true"/> for an Immortal Throne (or later) player.</param>
	/// <returns><see langword="true"/> when the item suits the player.</returns>
	public static bool IsSuitableFor(Item item, bool immortalThronePlayer)
		=> immortalThronePlayer || item.GameDlc == GameDlc.TitanQuest;

	/// <summary>
	/// The entries of <see cref="ItemActions.Edit"/> that do not need a catalog: the seed for any item, the completion of
	/// an incomplete relic or charm, the artifact of a formula.
	/// </summary>
	/// <param name="item">The item being edited.</param>
	public static ItemActions EditActions(Item item)
	{
		var actions = ItemActions.Seed | ItemActions.ForceSeed;

		if (item.IsRelicOrCharm && !item.IsRelicComplete)
			actions |= ItemActions.CompleteRelic;

		if (item.IsFormulae)
			actions |= ItemActions.CraftArtifact;

		return actions;
	}

	/// <summary>
	/// The destinations of <see cref="ItemActions.MoveTo"/>: the other bags of the panel first, then the other panels.
	/// </summary>
	public static IReadOnlyList<MoveDestination> MoveDestinations(in MoveContext context)
	{
		var destinations = new List<MoveDestination>();
		var item = context.Item;

		if (context.MaxSacks > 1)
		{
			// Vault bags are shown from 1, the player sacks from 0 since sack #0 is the main player sack.
			// From the sack, not the item: a selection right-clicked on an empty cell has no item under the cursor.
			int bagOffset = context.SackType == SackType.Vault ? 1 : 0;
			int? currentSack = context.CurrentSack >= 0 ? context.CurrentSack : item?.Place.SackNumber;

			for (int i = 0; i < context.MaxSacks; ++i)
			{
				bool mainPlayerSack = context.SackType == SackType.Player && i == 0;

				if (context.SackType == SackType.Player && !mainPlayerSack && i >= context.PlayerSackCount)
					continue;

				if (i != currentSack)
					destinations.Add(new MoveDestination(context.Here, i, i + bagOffset, mainPlayerSack));
			}
		}

		var here = context.Here;
		var panels = context.Panels.Where(location => location != here).Distinct();

		// TQ original save
		if (context.PlayerIsImmortalThrone == false)
		{
			// There is no Stash on a TQ original save
			panels = panels.Where(location => location is not (AutoMoveLocation.Stash or AutoMoveLocation.Trash));

			// TQIT+ items cannot go to the equipment, sacks and inventory (a multiple selection may hold some)
			if (item is null || item.GameDlc != GameDlc.TitanQuest)
				panels = panels.Where(location => location != AutoMoveLocation.Player);
		}

		bool secondaryVault = context.SecondaryVaultShown;
		foreach (var location in panels)
		{
			bool shown = location switch
			{
				AutoMoveLocation.SecondaryVault => secondaryVault,
				AutoMoveLocation.Player or AutoMoveLocation.Stash => !secondaryVault,
				AutoMoveLocation.Vault or AutoMoveLocation.Trash => true,
				_ => false,
			};

			if (shown)
				destinations.Add(new MoveDestination(location));
		}

		return destinations;
	}
}
