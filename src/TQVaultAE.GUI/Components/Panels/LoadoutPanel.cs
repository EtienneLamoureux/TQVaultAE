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
