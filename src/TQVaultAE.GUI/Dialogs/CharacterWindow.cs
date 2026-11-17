using System.Globalization;
using TQVaultAE.Application.Layout;
using TQVaultAE.Application.Session;
using TQVaultAE.Domain.Entities;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.GUI.Components.Panels;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Forms;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Models;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// A character in a floating window: its equipment on the stone of the slots and the four statistics tabs side by side
/// (I, II, III, Buffs), so that a toggle of the Buffs tab shows its effect on every number at once. A modeless window
/// owned by the main window, one per character (<see cref="Views.MainFormUserControl.OpenCharacterWindow"/>); its buff
/// toggles are its own, in memory only. Items go to and from the main window by drag and drop, with the main window's
/// rules; changes are saved by the main window's Save.
/// </summary>
internal sealed class CharacterWindow : VaultForm
{
	/// <summary>The name of the window's form.</summary>
	public const string FormName = "CharacterWindow";

	/// <summary>The gap between the border and the content, in design pixels.</summary>
	private const int ContentGap = 6;

	private readonly Func<(Bitmap Picture, Rectangle Source, Rectangle Target, Size Slots)?> _slotsGeometry;
	private readonly ItemDragInfo _dragInfo;
	private readonly CharacterWindowEquipmentPanel _equipment;
	private readonly EquipmentSlotsView _slots;
	private readonly CharacterStatsPanel[] _tabs;
	private readonly CharacterStatsSource _stats;
	private readonly CharacterStatsLook _look;

	/// <summary>Set while the window lays itself out: its moves are not the user's.</summary>
	private bool _layingOut;

	/// <summary>Initializes a new instance of the CharacterWindow class.</summary>
	/// <param name="owner">The main window, which owns the window.</param>
	/// <param name="save">The character.</param>
	/// <param name="player">The character's loaded save, shared with the main window.</param>
	/// <param name="dragInfo">The drag shared by every panel of the main window.</param>
	/// <param name="slotsGeometry">The slots picture and cells of the main equipment, at the main window's scale.</param>
	public CharacterWindow(MainForm owner, PlayerSave save, PlayerCollection player, ItemDragInfo dragInfo,
		Func<(Bitmap Picture, Rectangle Source, Rectangle Target, Size Slots)?> slotsGeometry)
		: base(owner.Services)
	{
		this.Owner = owner;
		this.Save = save;
		this.Player = player;
		this._dragInfo = dragInfo;
		this._slotsGeometry = slotsGeometry;

		// Laid out in code at the user's zoom and the window's own DPI (LayOutContent), again when either changes.
		this.UserZoomForLayout();
		CaravanDialog.SetUpWindow(this, FormName);
		this.AutoScaleMode = AutoScaleMode.None;
		this.StartPosition = FormStartPosition.Manual;
		this.Text = string.Format(CultureInfo.CurrentCulture, Resources.CharacterWindowTitle, save.Name);
		this.KeyPreview = true;

		this._equipment = new CharacterWindowEquipmentPanel(dragInfo, this.Services, save)
		{
			PlayerCollection = player,
		};
		this._slots = new EquipmentSlotsView(this._equipment, this.UIService) { Name = "characterWindowSlots" };

		// The game data of the character's own map while its statistics are computed (the main window may show another).
		this._stats = new CharacterStatsSource(this.Services) { ReadScope = () => this.Services.GameMapService.ReadWith(save) };
		this._look = new CharacterStatsLook(this.FontService, this.UIService);
		this._tabs = [.. Enumerable.Range(0, CharacterWindowLayout.StatsPanels).Select(tab => new CharacterStatsPanel(this.UIService)
		{
			Name = $"characterWindowTab{tab + 1}",
			Tab = tab,
			TabsFixed = true,
			GoldBarShown = tab == 0,
			View = () => this._stats.View(this.Player, this._equipment.WornItems),
		})];

		// Each window its own toggles: a click in its Buffs tab changes the four tabs at once.
		this._stats.BindBuffs(this._tabs[CharacterStatsPanel.BuffsTab], this.InvalidateTabs);
		this._equipment.OnEquipmentChanged += (_, _) =>
		{
			this._stats.Invalidate();
			this.InvalidateTabs();
			this.EquipmentChanged?.Invoke(this, EventArgs.Empty);
		};

		this.Controls.Add(this._slots);
		this.Controls.AddRange(this._tabs);

		// Tab I's gold bar goes on under tab II: tab I in front.
		this._tabs[0].BringToFront();

		this.DrawCustomBorder = true;
		this.ResizeCustomAllowed = false;
		this._equipment.Sack = player.EquipmentSack;
	}

	/// <summary>The character shown.</summary>
	public PlayerSave Save { get; }

	/// <summary>The character's loaded save, shared with the main window.</summary>
	public PlayerCollection Player { get; private set; }

	/// <summary>The equipment panel.</summary>
	internal CharacterWindowEquipmentPanel EquipmentPanel => this._equipment;

	/// <summary>The statistics panels, tab I first.</summary>
	internal IReadOnlyList<CharacterStatsPanel> StatsPanels => this._tabs;

	/// <summary>The buff toggles of this window.</summary>
	internal Application.Character.BuffSelection Buffs => this._stats.Buffs;

	/// <summary>Raised when an item was put in or taken out of the equipment from this window.</summary>
	public event EventHandler? EquipmentChanged;

	/// <summary>Raised when the user moved the window (not when it lays itself out).</summary>
	public event EventHandler? PlaceChanged;

	/// <summary>Follows the main window's zoom while open (<see cref="VaultForm.FollowUserZoom"/>).</summary>
	protected internal override bool FollowsUserZoomChanges => true;

	/// <summary>
	/// The equipment changed elsewhere (the main window shows the same character, or the save was read again): the
	/// slots and the statistics are drawn again, the toggles kept.
	/// </summary>
	public void EquipmentChangedElsewhere()
	{
		this._stats.Invalidate();
		this._equipment.Invalidate();
		this.InvalidateTabs();
	}

	/// <summary>
	/// The character's save was read again into a new instance (hot reload, another map): the window shows it, its toggles
	/// kept. An item held from the old equipment goes back first.
	/// </summary>
	public void Rebind(PlayerCollection player)
	{
		if (ReferenceEquals(player, this.Player))
			return;

		this.ReleaseDrag();
		this.Player = player;
		this._equipment.PlayerCollection = player;
		this._equipment.Sack = player.EquipmentSack;
		this.EquipmentChangedElsewhere();
	}

	/// <summary>Lays the content out again (the main window's scale changed: its equipment geometry follows).</summary>
	public void Relayout()
	{
		if (this.IsHandleCreated && !this.IsDisposed)
			this.LayOutContent(keepLocation: true);
	}

	private void InvalidateTabs()
	{
		foreach (var tab in this._tabs)
			tab.Invalidate();
	}

	/// <summary>An item held from this window's equipment goes back to its slot.</summary>
	/// <returns>False when it cannot (a modified item): the window must stay.</returns>
	private bool ReleaseDrag()
	{
		if (!this._dragInfo.IsActive || !ReferenceEquals(this._dragInfo.SrcSackPanel, this._equipment))
			return true;

		if (!this._dragInfo.CanBeCanceled)
			return false;

		this._dragInfo.Cancel();
		return true;
	}

	/// <summary>
	/// Lays the slots and the four tabs out at the window's scale (the user's zoom times its monitor's DPI factor, made
	/// smaller when the content would not fit its screen) and fits the window around them.
	/// </summary>
	/// <param name="keepLocation">True to keep the window where it is, within its screen; else its place is left as set.</param>
	private void LayOutContent(bool keepLocation)
	{
		if (this._slotsGeometry() is not var (picture, source, target, slots))
			return;

		this._layingOut = true;
		try
		{
			float mainScale = this.UIService.Scale;
			var region = EquipmentSlotsView.RegionSize(target, slots);
			var slotsDesign = new Size((int)Math.Ceiling(region.Width / mainScale), (int)Math.Ceiling(region.Height / mainScale));
			var firstDesign = CharacterStatsPanel.DesignSize;
			var statsDesign = CharacterStatsPanel.DesignSizeWithoutGoldBar;

			var chrome = this.ClientSize - this.ContentArea.Size;
			var area = Screen.FromControl(this).WorkingArea;
			float wanted = this.UIService.Zoom * this.DpiScale();
			int margin = (int)Math.Ceiling(ContentGap * wanted * 2);
			float scale = CharacterWindowLayout.FitScale(wanted, slotsDesign, firstDesign, statsDesign,
				new Size(area.Width - chrome.Width - margin, area.Height - chrome.Height - margin));

			var (slotsBounds, tabBounds, content) = CharacterWindowLayout.Arrange(slotsDesign, firstDesign, statsDesign, scale);
			int gap = (int)Math.Round(ContentGap * scale);
			var origin = this.ContentArea.Location + new Size(gap, gap);

			this._look.Apply(this._tabs, scale);
			for (int i = 0; i < this._tabs.Length; i++)
				this._tabs[i].Location = tabBounds[i].Location + (Size)origin;

			slotsBounds.Offset(origin);
			this._slots.Bounds = slotsBounds;
			this._slots.LayOut(picture, source, target, slots, scale / mainScale);

			this.ClientSize = content + chrome + new Size(2 * gap, 2 * gap);
			if (keepLocation)
				this.Location = WindowLayout.KeepOnScreen(this.Location, this.Size, [area]);
		}
		finally
		{
			this._layingOut = false;
		}
	}

	/// <inheritdoc/>
	protected override void OnLoad(EventArgs e)
	{
		base.OnLoad(e);
		this.LayOutContent(keepLocation: true);
	}

	/// <summary>The window moved to a monitor of another DPI: laid out again at its new scale.</summary>
	protected override void FollowDpi()
	{
		this.LayOutContent(keepLocation: true);
		this.RescalePhase("content");
	}

	/// <summary>The user's zoom changed: laid out again at the new zoom.</summary>
	protected override void OnUserZoomFollowed()
	{
		this.LayOutContent(keepLocation: true);
		this.RescalePhase("content");
	}

	/// <summary>The base font changed: the statistics fonts are made again from it.</summary>
	protected override void OnBaseFontChanged()
	{
		base.OnBaseFontChanged();
		this._look.Reset();
		this.Relayout();
	}

	/// <inheritdoc/>
	protected override void OnLocationChanged(EventArgs e)
	{
		base.OnLocationChanged(e);
		if (!this._layingOut && this.Visible && this.WindowState == FormWindowState.Normal)
			this.PlaceChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Escape puts an item held back, as in the main window.</summary>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.KeyCode == Keys.Escape && this._dragInfo.IsActive && this._dragInfo.CanBeCanceled)
		{
			this._dragInfo.Cancel();
			this.Refresh();
			e.Handled = true;
		}
	}

	/// <summary>An item held from this window's equipment goes back first; a modified one keeps the window open.</summary>
	protected override void OnFormClosing(FormClosingEventArgs e)
	{
		if (e.CloseReason == CloseReason.UserClosing && !this.ReleaseDrag())
		{
			this.Services.MessageBoxes.Inform(Resources.MainFormHoldingItem, Resources.MainFormHoldingItem2);
			e.Cancel = true;
			return;
		}

		this._look.Tooltip?.Hide();
		base.OnFormClosing(e);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
			this._look.Dispose();

		base.Dispose(disposing);
	}
}
