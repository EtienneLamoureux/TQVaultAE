using TQVaultAE.GUI.Components.Menus;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Character;

/// <summary>What the user asked the loadout bar for.</summary>
public enum LoadoutBarAction
{
	/// <summary>Another loadout is selected in the list.</summary>
	Select,

	/// <summary>Adds an empty loadout ([+] button).</summary>
	Add,

	/// <summary>Merges the worn equipment into the selected loadout.</summary>
	Update,

	/// <summary>Renames the selected loadout.</summary>
	Rename,

	/// <summary>Copies the selected loadout under a new name.</summary>
	Copy,

	/// <summary>Deletes the selected loadout.</summary>
	Delete,

	/// <summary>Gear swap: the selected loadout's pieces and the worn ones change places (menu).</summary>
	Swap,

	/// <summary>Empty loadout: its pieces move to the character's inventory (menu).</summary>
	EmptyToInventory,

	/// <summary>Empty loadout: its pieces move to the stash (menu).</summary>
	EmptyToStash,

	/// <summary>Empty loadout: its pieces move to the bag of the vault shown (menu).</summary>
	EmptyToVault,
}

/// <summary>
/// The loadouts of the shown character, under the gold of the statistics panel: a drop-down of their names in the gold
/// box of the character window and the [+] button of the attributes to add one (<see cref="GameListBar{TAction}"/>). The
/// right-click menu holds every action.
/// </summary>
public sealed class LoadoutBar : GameListBar<LoadoutBarAction>
{
	public LoadoutBar(IUIService uiService, IFontService fontService)
		: base(uiService, fontService)
	{
		this.EmptyMenu = new ContextMenuStrip { ShowImageMargin = false, ShowItemToolTips = true };
		CustomProfessionalRenderer.Apply(this.EmptyMenu);
		this.EmptyMenu.ItemClicked += this.ActionClicked;
		this.EmptyMenu.FillOnOpening(this.FillEmptyMenu);
	}

	/// <summary>The menu of destinations of Empty loadout, opened by the loadout panel's button.</summary>
	internal ContextMenuStrip EmptyMenu { get; }

	/// <summary>Whether Empty loadout offers a destination now (the loadout panel's button is greyed otherwise).</summary>
	internal bool CanEmpty => EmptyActions.Any(this.CanAct);

	/// <summary>The destinations of Empty loadout, in menu order.</summary>
	private static readonly LoadoutBarAction[] EmptyActions = [LoadoutBarAction.EmptyToInventory, LoadoutBarAction.EmptyToStash, LoadoutBarAction.EmptyToVault];

	/// <summary>
	/// Why the equipment cannot change (the PlayerReadonly setting), or null: Gear swap and Empty to inventory are then
	/// greyed and their tool tip says why; the loadouts can still be added, updated, renamed, copied and deleted.
	/// </summary>
	public string? EquipmentLocked { get; private set; }

	/// <summary>
	/// Why the pieces cannot be copied now (the Allow Item Copying setting off), or null: Update and Copy write copies,
	/// so they are greyed and their tool tip says why. Read at each menu opening: a setting change counts at once.
	/// </summary>
	public Func<string?>? CopyLocked { get; set; }

	protected internal override LoadoutBarAction SelectAction => LoadoutBarAction.Select;

	protected internal override LoadoutBarAction AddAction => LoadoutBarAction.Add;

	protected override string NoneText => Resources.LoadoutNone;

	protected override string AddToolTip => Resources.LoadoutAdd;

	protected override string ReadOnlyText => Resources.LoadoutReadOnly;

	protected override bool SelectsFirst => true;

	/// <summary>Shows the loadouts of a character.</summary>
	/// <param name="names">Their names.</param>
	/// <param name="selected">The one to select; the first one when null or unknown.</param>
	/// <param name="canChange">False for a character whose loadouts are only listed.</param>
	/// <param name="equipmentLocked">Why the equipment cannot change, or null when Gear swap can change it.</param>
	public void ShowLoadouts(IReadOnlyList<string> names, string? selected, bool canChange, string? equipmentLocked = null)
	{
		bool lockChanged = equipmentLocked != this.EquipmentLocked;
		this.EquipmentLocked = equipmentLocked;
		if (!this.SetList(names, selected, canChange) && lockChanged)
			this.Invalidate();
	}

	/// <inheritdoc/>
	public override bool CanAct(LoadoutBarAction action) => action switch
	{
		LoadoutBarAction.Select => this.Names.Count > 0,
		LoadoutBarAction.Add => this.CanChange,
		LoadoutBarAction.Swap or LoadoutBarAction.EmptyToInventory => this.CanChange && this.Selected is not null && this.EquipmentLocked is null,
		LoadoutBarAction.Update or LoadoutBarAction.Copy => this.CanChange && this.Selected is not null && this.CopyLocked?.Invoke() is null,
		_ => this.CanChange && this.Selected is not null,
	};

	/// <summary>
	/// Why an action is greyed, for its tool tip, or null when the bar offers it: a read-only character, then the
	/// Allow Item Copying setting for Update and Copy, the PlayerReadonly setting for Gear swap and Empty to inventory.
	/// </summary>
	/// <param name="action">The action.</param>
	public override string? LockedReason(LoadoutBarAction action)
		=> this.CanAct(action) ? null
		: !this.CanChange ? Resources.LoadoutReadOnly
		: action is LoadoutBarAction.Update or LoadoutBarAction.Copy ? this.CopyLocked?.Invoke()
		: this.EquipmentLocked is null ? null
		: action is LoadoutBarAction.EmptyToInventory ? string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.LoadoutInventoryLocked, Resources.SettingsPlayerReadonly)
		: action is LoadoutBarAction.Swap ? this.EquipmentLocked
		: null;

	protected override void FillActions(ToolStripItemCollection items)
		=> items.AddRange(
		[
			this.Entry(Resources.LoadoutSwap, LoadoutBarAction.Swap),
			this.SubMenu(Resources.LoadoutEmpty, this.CanEmpty, this.EmptyEntries()),
			this.Entry(Resources.LoadoutUpdate, LoadoutBarAction.Update),
			this.Entry(Resources.LoadoutRename, LoadoutBarAction.Rename),
			this.Entry(Resources.LoadoutCopy, LoadoutBarAction.Copy),
			this.Entry(Resources.LoadoutDelete, LoadoutBarAction.Delete),
			new ToolStripSeparator(),
			this.Entry(Resources.LoadoutAddMenu, LoadoutBarAction.Add),
		]);

	private void FillEmptyMenu()
	{
		this.EmptyMenu.Font = this.MenuFont();
		this.EmptyMenu.Items.Clear();
		this.EmptyMenu.Items.AddRange(this.EmptyEntries());
	}

	/// <summary>
	/// The destinations of Empty loadout, for the right-click menu's submenu and the loadout panel's button menu; a greyed
	/// one says why (the PlayerReadonly setting for the inventory).
	/// </summary>
	private ToolStripItem[] EmptyEntries()
		=>
		[
			this.Entry(Resources.LoadoutEmptyToInventory, LoadoutBarAction.EmptyToInventory),
			this.Entry(Resources.LoadoutEmptyToStash, LoadoutBarAction.EmptyToStash),
			this.Entry(Resources.LoadoutEmptyToVault, LoadoutBarAction.EmptyToVault),
		];

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			this.EmptyMenu.Dispose();

		base.Dispose(disposing);
	}
}
