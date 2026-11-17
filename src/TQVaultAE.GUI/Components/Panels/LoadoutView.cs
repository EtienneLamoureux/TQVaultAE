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
	/// <summary>The previous loadout arrow's pictures, with {0} for the state.</summary>
	internal const string PreviousPicture = """InGameUI\characterscreen\characterrotateleft{0}01.tex""";

	/// <summary>The next loadout arrow's pictures, with {0} for the state.</summary>
	internal const string NextPicture = """InGameUI\characterscreen\characterrotateright{0}01.tex""";

	/// <summary>The size of the previous and next arrows' pictures, in pixels at scale 1.</summary>
	internal static readonly Size TurnPictureSize = new(20, 14);

	private readonly IUIService _uiService;
	private readonly GameButton _back;
	private readonly GameButton _previous;
	private readonly GameButton _next;
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
		this._previous = new GameButton(this._uiService, PreviousPicture, Resources.LoadoutPrevious) { Name = "loadoutPreviousButton" };
		this._previous.Pressed += (_, _) => this.TurnRequested?.Invoke(this, -1);
		this._next = new GameButton(this._uiService, NextPicture, Resources.LoadoutNext) { Name = "loadoutNextButton" };
		this._next.Pressed += (_, _) => this.TurnRequested?.Invoke(this, 1);

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
		foreach (var button in new[] { this._back, this._previous, this._next, this._swap, this._empty })
		{
			this.Controls.Add(button);
			button.BringToFront();
		}
	}

	/// <summary>The loadout panel.</summary>
	public LoadoutPanel Panel => (LoadoutPanel)this.SlotsPanel;

	/// <summary>The Back arrow, which returns to the statistics panel.</summary>
	internal Control BackButton => this._back;

	/// <summary>The arrow that shows the previous loadout.</summary>
	internal Control PreviousButton => this._previous;

	/// <summary>The arrow that shows the next loadout.</summary>
	internal Control NextButton => this._next;

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

	/// <summary>Raised when the user clicks the previous (-1) or next (+1) arrow.</summary>
	public event EventHandler<int>? TurnRequested;

	/// <summary>Whether the previous and next arrows can be clicked: greyed with a single loadout.</summary>
	public bool CanTurn
	{
		get => this._next.Available;
		set
		{
			this._previous.Available = value;
			this._next.Available = value;
		}
	}

	/// <summary>The buttons in the gaps of the stone, at the panel's cell size.</summary>
	protected override void PlaceExtras(Point panelLocation)
	{
		var (back, previous, next, swap, empty) = LoadoutPanel.ButtonCells();
		void Place(Control button, RectangleF cells)
		{
			var bounds = this.Panel.CellsToPixels(cells);
			bounds.Offset(panelLocation);
			if (button.Bounds != bounds)
				button.Bounds = bounds;
		}

		Place(this._back, back);
		Place(this._previous, previous);
		Place(this._next, next);
		Place(this._swap, swap);
		Place(this._empty, empty);
	}
}
