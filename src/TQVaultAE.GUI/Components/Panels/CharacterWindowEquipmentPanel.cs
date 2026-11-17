using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Actions;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Models;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The equipment of a floating character window: the character's own equipment sack, with the slots, pictures and rules
/// of the main equipment (requirements, read-only character, <c>PlayerReadonly</c>). Items go to and from the main window
/// by drag and drop (one drag shared by every panel); "Move to" names the main window's bags, so it is left out here.
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

	/// <summary>The character's map: its items are read with it, whatever character the main window shows.</summary>
	protected override string? SlotMap => this.Services.GameMapService.ModNameFor(this._save);

	/// <summary>"Move to" sends items to the main window's bags: an item goes there by drag and drop.</summary>
	protected override ItemActions UnavailableActions => ItemActions.MoveTo;
}
