using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Components.Panels;

/// <summary>
/// The loadout panel in place of the statistics panel: the <see cref="LoadoutPanel"/> on the stone of the slots
/// (<see cref="EquipmentSlotsView"/>), with the game's small buttons in the gaps of the stone: Back (returns to the
/// statistics) in the upper right, previous and next loadout in the lower left and right, Gear swap and Empty loadout
/// under the rings.
/// </summary>
public sealed class LoadoutView : EquipmentSlotsView
{
	private readonly IUIService _uiService;
	private readonly GameButton _back;
	private readonly GameButton _swap;
	private readonly GameButton _empty;

	/// <summary>Initializes a new instance of the LoadoutView class.</summary>
	/// <param name="dragInfo">The drag shared by every panel.</param>
	/// <param name="services">The services.</param>
	public LoadoutView(ItemDragInfo dragInfo, GuiServices services)
		: base(new LoadoutPanel(dragInfo, services), services.UIService)
	{
		this._uiService = services.UIService;
		this.Name = "loadoutView";
		this._back = new GameButton(this._uiService, """InGameUI\tutorial\buttonback{0}01.tex""", Resources.LoadoutBack) { Name = "loadoutBackButton" };
		this._back.Pressed += (_, _) => this.BackRequested?.Invoke(this, EventArgs.Empty);

		// The game has no disable picture of this tile (greyed instead); its lighter over picture shows at rest, the up one under the mouse.
		this._swap = new GameButton(this._uiService, """InGameUI\characterscreen\weaponswapbtn{0}01.tex""", Resources.LoadoutSwap, hasDisable: false, restState: "over", hoverState: "up")
		{
			Name = "loadoutSwapButton",
		};
		this._swap.Pressed += (_, _) => this.SwapRequested?.Invoke(this, EventArgs.Empty);

		// The game's inventory button of the HUD (no disable picture: greyed instead); it opens the menu of destinations.
		this._empty = new GameButton(this._uiService, """InGameUI\hudinventorybutton{0}01.tex""", Resources.LoadoutEmpty, hasDisable: false)
		{
			Name = "loadoutEmptyButton",
		};
		this._empty.Pressed += (_, _) => this.EmptyRequested?.Invoke(this, EventArgs.Empty);
		foreach (var button in new[] { this._back, this._swap, this._empty })
		{
			this.Controls.Add(button);
			button.BringToFront();
		}
	}

	/// <summary>The loadout panel.</summary>
	public LoadoutPanel Panel => (LoadoutPanel)this.SlotsPanel;

	/// <summary>The Back arrow, which returns to the statistics panel.</summary>
	internal Control BackButton => this._back;

	/// <summary>The Gear swap button, which exchanges the loadout's pieces and the worn ones.</summary>
	internal Control SwapButton => this._swap;

	/// <summary>Raised when the user clicks the Gear swap button.</summary>
	public event EventHandler? SwapRequested;

	/// <summary>Whether the Gear swap button can be clicked.</summary>
	public bool CanSwap => this._swap.Available;

	/// <summary>
	/// Why Gear swap is greyed (the loadout bar's reason, shown as the button's tool tip), or null when it can be clicked.
	/// </summary>
	public string? SwapLocked
	{
		get => this._swap.Available ? null : this._swap.LockedText;
		set
		{
			this._swap.LockedText = value;
			this._swap.Available = value is null;
		}
	}

	/// <summary>The Empty loadout button, which opens the menu of destinations (inventory, stash, vault).</summary>
	internal Control EmptyButton => this._empty;

	/// <summary>Raised when the user clicks the Empty loadout button: the owner shows the menu of destinations under it.</summary>
	public event EventHandler? EmptyRequested;

	/// <summary>
	/// Why Empty loadout is greyed (the loadout bar's reason, shown as the button's tool tip), or null when it can be clicked.
	/// </summary>
	public string? EmptyLocked
	{
		get => this._empty.Available ? null : this._empty.LockedText;
		set
		{
			this._empty.LockedText = value;
			this._empty.Available = value is null;
		}
	}

	/// <summary>Raised when the user clicks the Back arrow.</summary>
	public event EventHandler? BackRequested;

	/// <summary>The buttons in the gaps of the stone, at the panel's cell size: the arrows, Back, Gear swap and Empty loadout.</summary>
	protected override void PlaceExtras(Point panelLocation)
	{
		base.PlaceExtras(panelLocation);
		var (back, _, _, swap, empty) = LoadoutPanel.ButtonCells();
		this.PlaceOnStone(this._back, back, panelLocation);
		this.PlaceOnStone(this._swap, swap, panelLocation);
		this.PlaceOnStone(this._empty, empty, panelLocation);
	}
}
