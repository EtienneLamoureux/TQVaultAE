using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// A loadout of the shown character as a second equipment panel: the same slots, pictures and drop rules as the
/// equipment (slot classes, the item requirement setting, two-handed weapons taking both hands), with the loadout's
/// pieces in their slots. Pieces are moved, never copied: one dropped here leaves where it came from, one taken out goes
/// to the hand. The loadout changes in memory (its loadouts marked modified) and is written with the other files.
/// </summary>
/// <remarks>
/// The panel shows an equipment sack built from the loadout (<see cref="SackType.Equipment"/>, one item per slot, an
/// empty item in an empty slot), so <see cref="EquipmentRules"/> work on it unchanged; the panel itself is a
/// <see cref="SackType.Loadout"/> container: the items dropped here take the loadout's place (search), and neither the
/// character's read-only state nor the <c>PlayerReadonly</c> setting applies (the loadouts file is apart from the save).
/// </remarks>
public sealed class LoadoutPanel : EquipmentPanel
{
	/// <summary>The cells the slots take in the equipment panel: columns 0 to 9, rows 0 to 13.</summary>
	public static readonly Size SlotCells = new(10, 14);

	/// <summary>The size of the Back arrow in cells: its 16 x 20 picture at the cell size of 32 pixels.</summary>
	private static readonly SizeF BackCells = new(0.5F, 0.625F);

	/// <summary>The size of the previous and next arrows in cells: their 20 x 14 pictures at the cell size of 32 pixels.</summary>
	private static readonly SizeF TurnCells = new(0.625F, 0.4375F);

	/// <summary>The size of the Gear swap button's picture, 26 x 34 pixels at the cell size of 32 pixels.</summary>
	private static readonly Size SwapPicture = new(26, 34);

	/// <summary>The size of the Empty loadout button's picture, 25 x 32 pixels at the cell size of 32 pixels.</summary>
	private static readonly Size EmptyPicture = new(25, 32);

	/// <summary>The gap between the Gear swap and Empty loadout buttons, in cells.</summary>
	private const float ButtonGap = 0.125F;

	/// <summary>
	/// Where the panel's buttons sit, in cells, in the gaps of the stone as the marks of the equipment do: the Back arrow
	/// right of the right column, level with its top; the previous and next arrows left of the left column and right of
	/// the right column, level with their bottom; the Gear swap and Empty loadout buttons side by side under the rings,
	/// centred as a pair.
	/// </summary>
	public static (RectangleF Back, RectangleF Previous, RectangleF Next, RectangleF Swap, RectangleF Empty) ButtonCells()
	{
		float rightTop = float.MaxValue, leftBottom = 0F, rightBottom = 0F, rightEnd = 0F;
		for (int slot = 0; slot <= EquipmentRules.ArtifactSlot; slot++)
		{
			var (location, size) = LoadoutComparison.SlotCells(slot);
			if (location.X < LoadoutComparison.MiddleColumnFirst)
				leftBottom = Math.Max(leftBottom, location.Y + size.Height);
			else if (location.X > LoadoutComparison.MiddleColumnLast)
			{
				rightTop = Math.Min(rightTop, location.Y);
				rightBottom = Math.Max(rightBottom, location.Y + size.Height);
				rightEnd = Math.Max(rightEnd, location.X + size.Width);
			}
		}

		// Centred in the one-cell gaps: column 0 on the left, the column after the right slots on the right.
		float Centred(float column, float width) => column + ((1F - width) / 2F);

		// Under the rings: from the margin of their frames, shrunk to the bottom of the slots when taller.
		var (ring1, ringSize) = LoadoutComparison.SlotCells(EquipmentRules.Ring1Slot);
		var (ring2, _) = LoadoutComparison.SlotCells(EquipmentRules.Ring2Slot);
		float swapTop = ring1.Y + ringSize.Height + LoadoutComparison.FrameMargin;
		float swapHeight = Math.Min(SwapPicture.Height / 32F, SlotCells.Height - swapTop);
		float swapWidth = swapHeight * SwapPicture.Width / SwapPicture.Height;
		float emptyHeight = swapHeight * EmptyPicture.Height / SwapPicture.Height;
		float emptyWidth = emptyHeight * EmptyPicture.Width / EmptyPicture.Height;
		float ringsCentre = (ring1.X + ring2.X + ringSize.Width) / 2F;
		float pairLeft = ringsCentre - ((swapWidth + ButtonGap + emptyWidth) / 2F);

		return (
			new RectangleF(Centred(rightEnd, BackCells.Width), rightTop, BackCells.Width, BackCells.Height),
			new RectangleF(Centred(0F, TurnCells.Width), leftBottom - TurnCells.Height, TurnCells.Width, TurnCells.Height),
			new RectangleF(Centred(rightEnd, TurnCells.Width), rightBottom - TurnCells.Height, TurnCells.Width, TurnCells.Height),
			new RectangleF(pairLeft, swapTop, swapWidth, swapHeight),
			new RectangleF(pairLeft + swapWidth + ButtonGap, swapTop + ((swapHeight - emptyHeight) / 2F), emptyWidth, emptyHeight));
	}

	/// <summary>The pieces shown, as the loadout held them when the sack was built or last composed here.</summary>
	private IReadOnlyList<Item?>? shownPieces;

	/// <summary>Initializes a new instance of the LoadoutPanel class.</summary>
	/// <param name="dragInfo">The drag shared by every panel.</param>
	/// <param name="services">The services.</param>
	public LoadoutPanel(ItemDragInfo dragInfo, GuiServices services)
		: base(SlotCells.Width, SlotCells.Height, dragInfo, AutoMoveLocation.NotSet, services)
	{
		this.SackType = SackType.Loadout;
		this.Name = "loadoutPanel";
	}

	/// <summary>The loadouts of the shown character, while a loadout is shown.</summary>
	public LoadoutSet? Loadouts { get; private set; }

	/// <summary>The loadout shown, or null.</summary>
	public string? LoadoutName { get; private set; }

	/// <summary>Raised once the loadout got new pieces from this panel (or one changed in place).</summary>
	public event EventHandler? PiecesChanged;

	/// <summary>
	/// Shows a loadout, or nothing. The slots are built again only when the loadout's pieces changed outside the panel
	/// (update, a move from the search results, another character); a rename keeps them.
	/// </summary>
	/// <param name="loadouts">The character's loadouts, or null.</param>
	/// <param name="name">The loadout.</param>
	/// <param name="player">The character, whose equipment gives the number of slots.</param>
	public void ShowLoadout(LoadoutSet? loadouts, string? name, PlayerCollection? player)
	{
		var loadout = loadouts is { ReadFailed: false } ? loadouts.Find(name) : null;
		if (loadouts is null || loadout is null || player is null)
		{
			this.ReleaseDrag();
			this.Loadouts = null;
			this.LoadoutName = null;
			this.shownPieces = null;
			this.PlayerCollection = null;
			this.Sack = null;
			return;
		}

		this.Loadouts = loadouts;
		this.LoadoutName = loadout.Name;
		this.PlayerCollection = player;
		if (this.Sack is not null && ReferenceEquals(loadout.Pieces, this.shownPieces))
			return;

		this.ReleaseDrag();
		this.shownPieces = loadout.Pieces;
		this.Sack = BuildSack(loadout, player.EquipmentSack?.NumberOfSlots ?? EquipmentRules.ArtifactSlot + 1, loadouts.Mod);
		this.Invalidate();
	}

	/// <summary>
	/// The equipment sack of a loadout: its pieces in their slots (the items themselves, not copies), an empty item in
	/// each empty slot and in the shadow slot of a two-handed weapon.
	/// </summary>
	/// <param name="loadout">The loadout.</param>
	/// <param name="slots">The number of equipment slots of the character.</param>
	/// <param name="mod">The character's map, given to the empty items.</param>
	public static SackCollection BuildSack(Loadout loadout, int slots, string? mod)
	{
		var sack = new SackCollection
		{
			SackType = SackType.Equipment,
			IsImmortalThrone = slots > EquipmentRules.ArtifactSlot,
			size = slots,
			slots = slots,
		};

		for (int slot = 0; slot < slots; slot++)
		{
			var piece = slot < loadout.Pieces.Count ? loadout.Pieces[slot] : null;
			Item item;
			if (piece is not null)
				item = piece;
			else
			{
				item = new Item().MakeEmptyItem();
				item.SetContainerMod(mod);
			}

			item.Location = SackCollection.GetEquipmentLocationOffset(slot);
			sack.AddItem(item);
		}

		sack.IsModified = false;
		return sack;
	}

	/// <summary>
	/// The slots changed (a piece dropped here, or taken out and put elsewhere): the loadout gets its new pieces, the
	/// loadouts are marked modified and the search follows.
	/// </summary>
	public override void SlotsChanged()
	{
		if (this.Loadouts is not { } loadouts || this.LoadoutName is not { } name || this.Sack is not { } sack)
			return;

		// The slots only: a piece picked up by a weapon swap waits at the end of the sack, in the hand.
		if (!this.Services.Loadouts.Compose(loadouts, name, [.. sack.Take(sack.NumberOfSlots)]))
			return;

		this.shownPieces = loadouts.Find(name)?.Pieces;
		this.PiecesChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>A piece deleted, or changed in place (affix, relic, seed): the loadouts are written at the next save.</summary>
	protected override void OnShownItemsChanged()
	{
		if (this.Loadouts is not { } loadouts)
			return;

		this.SlotsChanged();
		loadouts.IsModified = true;
		this.PiecesChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>The character's map: the pieces of a loadout are read with it, as the character's items.</summary>
	protected override string? SlotMap => this.Loadouts?.Mod;

	/// <summary>"Move to" sends items to the panels' bags: a loadout piece goes elsewhere by drag and drop.</summary>
	protected override ItemActions UnavailableActions => ItemActions.MoveTo;

	/// <summary>The loadout's place: the loadouts file, the character, the loadout's index and name.</summary>
	internal override ContainerPlace PlaceHere()
	{
		if (this.Loadouts is not { } loadouts || this.LoadoutName is not { } name)
			return base.PlaceHere();

		int index = loadouts.Loadouts.ToList().FindIndex(l => string.Equals(l.Name, name, StringComparison.OrdinalIgnoreCase));
		return new ContainerPlace
		{
			SackType = SackType.Loadout,
			Path = loadouts.FilePath,
			Name = loadouts.CharacterName,
			SackNumber = index,
			Loadout = name,
		};
	}

	/// <summary>An item held from the slots shown goes back before they are replaced.</summary>
	private void ReleaseDrag()
	{
		if (this.Sack is not null && this.DragInfo.IsActive && this.DragInfo.SrcSack == this.Sack && this.DragInfo.CanBeCanceled)
			this.DragInfo.Cancel();
	}
}
