using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The equipment of a floating character window: the character's own equipment sack, with the slots, pictures and rules
/// of the main equipment (requirements, read-only character, <c>PlayerReadonly</c>), or a loadout's pieces shown read-only
/// (<see cref="ShowsLoadout"/>). Items go to and from the main window by drag and drop (one drag shared by every panel);
/// Move to sends them to the vault shown or the transfer stash, never to another character's bags.
/// </summary>
public sealed class CharacterWindowEquipmentPanel : EquipmentPanel
{
	private readonly PlayerSave _save;

	/// <summary>Initializes a new instance of the CharacterWindowEquipmentPanel class.</summary>
	/// <param name="dragInfo">The drag shared by every panel of the main window.</param>
	/// <param name="services">The services.</param>
	/// <param name="save">The character shown.</param>
	public CharacterWindowEquipmentPanel(ItemDragInfo dragInfo, GuiServices services, PlayerSave save)
		: base(LoadoutPanel.SlotCells.Width, LoadoutPanel.SlotCells.Height, dragInfo, AutoMoveLocation.NotSet, services)
	{
		this._save = save;
		this.Name = "characterWindowEquipment";
	}

	/// <summary>
	/// True while a loadout's pieces are shown in place of the equipment: they are only looked at (tool tips, Compare,
	/// statistics), never moved; the loadout panel of the main window changes them.
	/// </summary>
	public bool ShowsLoadout { get; set; }

	/// <inheritdoc/>
	public override bool ItemsReadOnly => this.ShowsLoadout || base.ItemsReadOnly;

	/// <summary>The character's map: its items are read with it, whatever character the main window shows.</summary>
	protected override string? SlotMap => this.Services.GameMapService.ModNameFor(this._save);

	/// <summary>
	/// The vault shown and, for an Immortal Throne character, the transfer stash: the main window's player and stash
	/// panels may show another character.
	/// </summary>
	protected override IReadOnlyList<MoveDestination> MoveDestinations(Item? focusedItem)
		=> this.Owner?.IsImmortalThrone == true
			? [new(AutoMoveLocation.Vault), new(AutoMoveLocation.Stash)]
			: [new(AutoMoveLocation.Vault)];

	/// <summary>The stash entry is the transfer stash (<see cref="AutoMovePlanner.StashTarget"/>).</summary>
	protected override string MoveLabel(AutoMoveLocation location)
		=> location == AutoMoveLocation.Stash ? Resources.GlobalTransferStash : base.MoveLabel(location);

	/// <summary>The quick move keys reach the same panels as Move to.</summary>
	protected override bool QuickMoveAllowed(AutoMoveLocation destination)
		=> this.MoveDestinations(null).Any(d => d.Location == destination);
}
