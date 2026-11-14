using System.Drawing;
using TQVaultAE.Domain.Entities;

namespace TQVaultAE.Domain.Actions;

/// <summary>How the piece worn in a slot compares with the same slot of a loadout.</summary>
public enum LoadoutSlotMatch
{
	/// <summary>Nothing on either side: no mark.</summary>
	BothEmpty,

	/// <summary>The worn piece is the loadout's (<see cref="ItemKey"/>, the identity Load keeps worn pieces by).</summary>
	Same,

	/// <summary>Both sides hold a piece, and they are not identical.</summary>
	Different,

	/// <summary>One side holds a piece, the other is empty.</summary>
	OneEmpty,
}

/// <summary>The mark of one slot: how it compares, and where it is drawn.</summary>
/// <param name="Slot">The equipment slot.</param>
/// <param name="Match">How the worn piece compares with the loadout's.</param>
/// <param name="Bounds">Where the mark is drawn, in cells of the equipment panel (see <see cref="LoadoutComparison.MarkBounds"/>).</param>
public readonly record struct LoadoutSlotMark(int Slot, LoadoutSlotMatch Match, RectangleF Bounds)
{
	/// <summary>Whether the two pieces can be compared side by side: both sides hold one.</summary>
	public bool CanCompare => this.Match is LoadoutSlotMatch.Same or LoadoutSlotMatch.Different;
}

/// <summary>
/// The worn equipment compared slot by slot with a loadout, for the marks beside the equipment slots: identical,
/// different, or one side empty. A slot empty on both sides gets no mark.
/// </summary>
public static class LoadoutComparison
{
	/// <summary>Side of a mark, in cells: half a cell, so that it fits in the gaps between the slot frames.</summary>
	public const float MarkSize = 0.5F;

	/// <summary>Room left between a slot's cells and a mark under it, in cells: the slot frame drawn around the cells.</summary>
	public const float FrameMargin = 0.15F;

	/// <summary>How the piece worn in a slot compares with the loadout's piece for that slot.</summary>
	/// <param name="worn">The worn piece (<see langword="null"/> or an empty item for an empty slot).</param>
	/// <param name="piece">The loadout's piece (<see langword="null"/> or an empty item for an empty slot).</param>
	public static LoadoutSlotMatch CompareSlot(Item? worn, Item? piece)
	{
		bool wornEmpty = ItemKey.IsEmptyItem(worn);
		bool pieceEmpty = ItemKey.IsEmptyItem(piece);
		if (wornEmpty && pieceEmpty)
			return LoadoutSlotMatch.BothEmpty;
		if (wornEmpty || pieceEmpty)
			return LoadoutSlotMatch.OneEmpty;

		return ItemIdentityComparer.Instance.Equals(worn, piece) ? LoadoutSlotMatch.Same : LoadoutSlotMatch.Different;
	}

	/// <summary>
	/// The marks of the worn equipment against a loadout, slot for slot (a piece worn in another slot, rings or weapon
	/// sets swapped, counts as different here: the marks tell what Load changes in each slot), in slot order; the slots
	/// empty on both sides have none.
	/// </summary>
	/// <param name="worn">The worn equipment, one item per slot (an empty item or <see langword="null"/> for an empty slot).</param>
	/// <param name="pieces">The loadout's pieces, one per slot; slots beyond the equipment's are ignored, missing ones are empty.</param>
	public static IReadOnlyList<LoadoutSlotMark> Compare(IReadOnlyList<Item?> worn, IReadOnlyList<Item?> pieces)
	{
		ArgumentNullException.ThrowIfNull(worn);
		ArgumentNullException.ThrowIfNull(pieces);

		var marks = new List<LoadoutSlotMark>();
		for (int slot = 0; slot < worn.Count && slot < EquipmentRules.ArtifactSlot + 1; slot++)
		{
			var match = CompareSlot(worn[slot], slot < pieces.Count ? pieces[slot] : null);
			if (match != LoadoutSlotMatch.BothEmpty)
				marks.Add(new LoadoutSlotMark(slot, match, MarkBounds(slot)));
		}

		return marks;
	}

	/// <summary>
	/// Where the mark of a slot is drawn, in cells of the equipment panel (x, y from its top left cell): in the gap
	/// beside the slot, never over a slot. The slots of the left column (weapon sets' first hands, artifact) have it on
	/// their left, level with their top; those of the right column (shields, arms) on their right; those of the middle
	/// column (head, neck, body, legs, each ring) under them, centred.
	/// </summary>
	/// <param name="slot">The equipment slot.</param>
	public static RectangleF MarkBounds(int slot)
	{
		var (location, size) = SlotCells(slot);
		const float sideGap = (1F - MarkSize) / 2F;

		// The columns of the equipment panel: left 1-2, middle 4-5, right 7-8, a gap of one cell around each.
		if (location.X < MiddleColumnFirst)
			return new RectangleF(location.X - 1F + sideGap, location.Y, MarkSize, MarkSize);
		if (location.X > MiddleColumnLast)
			return new RectangleF(location.X + size.Width + sideGap, location.Y, MarkSize, MarkSize);

		return new RectangleF(
			location.X + ((size.Width - MarkSize) / 2F),
			location.Y + size.Height + FrameMargin,
			MarkSize,
			MarkSize);
	}

	/// <summary>The cell columns of the middle slots (head, neck, body, legs, rings).</summary>
	private const int MiddleColumnFirst = 4;

	/// <inheritdoc cref="MiddleColumnFirst"/>
	private const int MiddleColumnLast = 5;

	/// <summary>The cells of a slot: its box for a weapon slot (the whole box, top and bottom tips included).</summary>
	private static (Point Location, Size Size) SlotCells(int slot)
	{
		if (slot < 0 || slot > EquipmentRules.ArtifactSlot)
			throw new ArgumentOutOfRangeException(nameof(slot), slot, "Not an equipment slot.");

		var location = SackCollection.GetEquipmentLocationOffset(slot);
		if (SackCollection.IsWeaponSlot(slot))
			return (SackCollection.GetWeaponLocationOffset(location.Y), SackCollection.WeaponLocationSize);

		return (location, SackCollection.GetEquipmentLocationSize(slot));
	}
}
