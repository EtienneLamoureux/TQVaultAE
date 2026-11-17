//-----------------------------------------------------------------------
// <copyright file="StashPanel.PlayerInfo.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.Application.Character;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
using TQVaultAE.GUI.Infrastructure;
using TQVaultAE.GUI.Tooltips;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Components.Panels;

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.GUI.Components.Rendering;
using TQVaultAE.GUI.Components.Character;

public partial class StashPanel
{
	/// <summary>
	/// Calculates the attribute bonuses of the gear and the skills and updates the player (after an edit of its skills).
	/// </summary>
	private void GetSkillStatBonus()
	{
		if (this.Player is not null)
			this.Services.PlayerService.UpdateAttributeBonuses(this.Player, this.equipmentPanel.WornItems);
	}

	/// <summary>The game's character window statistics (tabs I, II, III), read only, right of the equipment.</summary>
	private CharacterStatsPanel? _statsPanel;

	/// <summary>
	/// What the statistics panel shows, with the toggles of its Buffs tab for the character shown: memory only, back to
	/// the defaults for another one.
	/// </summary>
	private CharacterStatsSource? _stats;

	/// <summary>The fonts and tool tip of the statistics panel at the scale.</summary>
	private CharacterStatsLook? _statsLook;

	/// <summary>The Edit button under the statistics panel, which opens the character editor.</summary>
	private ScalingButton? _editButton;

	/// <summary>The button beside Edit that opens the character in a floating window.</summary>
	private ScalingButton? _floatButton;

	/// <summary>The Edit button's size at 96 dpi and zoom 1, that of the main window's menu buttons.</summary>
	private static readonly Size EditButtonDesignSize = new(137, 30);

	/// <summary>The loadouts of the character, under the gold bar, the whole width of the statistics panel.</summary>
	private LoadoutBar? _loadoutBar;

	/// <summary>The loadouts file the bar shows.</summary>
	private string? _loadoutsShown;

	/// <summary>The loadout to select once the bar is built (a search result shown before the equipment tab was).</summary>
	private string? _loadoutToSelect;

	/// <summary>The loadout chosen in the bar as a second equipment panel, in place of the statistics panel.</summary>
	private LoadoutView? _loadoutView;

	/// <summary>The room right of the equipment slots: the statistics or the loadout panel, the loadout bar, the Edit button.</summary>
	private CharacterRoom? _room;

	/// <summary>
	/// True from the moment the user chooses a loadout in the bar until the Back arrow (or another character): the
	/// loadout panel then shows in place of the statistics panel.
	/// </summary>
	private bool _loadoutPanelOpen;

	/// <summary>The view of the loadout panel, once the statistics panel was shown.</summary>
	internal LoadoutView? LoadoutView => this._loadoutView;

	/// <summary>The loadout panel, once the statistics panel was shown.</summary>
	internal LoadoutPanel? LoadoutPanel => this._loadoutView?.Panel;

	/// <summary>Whether the loadout panel is asked in place of the statistics panel.</summary>
	internal bool LoadoutPanelOpen
	{
		get => this._loadoutPanelOpen;
		set
		{
			if (this._loadoutPanelOpen == value)
				return;

			this._loadoutPanelOpen = value;
			this.DisplayPlayerInfo();
		}
	}

	/// <summary>Raised for each action asked on the loadout bar (add, load, update, rename, copy, delete, select).</summary>
	public event EventHandler<ListBarEventArgs<LoadoutBarAction>>? OnLoadoutAction;

	/// <summary>The loadout bar, once the statistics panel was shown.</summary>
	internal LoadoutBar? LoadoutBar => this._loadoutBar;

	/// <summary>Shows the statistics panel of the character next to the equipment, and hides it on the other tabs.</summary>
	private void DisplayPlayerInfo()
	{
		bool show = this.Player?.PlayerInfo is not null && this.equipmentPanel.Visible && this.CurrentBag == BagIdConstants.BAGID_EQUIPMENTPANEL;
		if (!show)
		{
			// Hidden, the loadout panel keeps its slots: a piece held from them may go to another tab.
			if (this._room is not null)
				this._room.Visible = false;
			// No bar shown, no loadout to compare with (another character may come without its statistics).
			this.equipmentPanel.ComparedLoadout = null;
			this._statsLook?.Tooltip?.Hide();
			return;
		}

		var panel = this.EnsureStatsPanel();
		var room = this._room!;
		room.EditAllowed = this.USettings.AllowCharacterEdit;
		this.LayoutStatsPanel(panel);
		if (!room.Visible)
		{
			// Over the equipment panel, whose empty cells reach under the room.
			room.Visible = true;
			room.BringToFront();
		}

		// The loadouts may have changed (read again) or the character turned read-only: the bar follows, drawn again
		// only when it changes; the loadout panel, when open, takes the place of the statistics.
		this.RefreshLoadouts();

		if (DisplayPlayerInfoLastName == this.Player!.PlayerName)
			return;

		// Another character, or its equipment or skills changed: the panel is built again.
		DisplayPlayerInfoLastName = this.Player.PlayerName;
		this._stats!.NewCharacter();
		panel.Invalidate();
	}

	/// <summary>
	/// The equipment changed outside the panels (a loadout loaded): the attributes the requirements count and the
	/// statistics are computed again, and the panel drawn again.
	/// </summary>
	public void EquipmentReplaced()
	{
		this.GetSkillStatBonus();
		this.DisplayPlayerInfoLastName = null;
		this.equipmentPanel.Invalidate();
		this.Refresh();
	}

	/// <summary>
	/// Shows the loadouts of the shown character in the bar: those read with it, listed only for a read-only character
	/// or a file that could not be read.
	/// </summary>
	/// <param name="select">The loadout to select; by default the one selected, when the bar still shows this character.</param>
	public void RefreshLoadouts(string? select = null)
	{
		if (this._loadoutBar is not { } bar)
		{
			this._loadoutToSelect = select;
			return;
		}

		select ??= this._loadoutToSelect;
		this._loadoutToSelect = null;
		var player = this.Player;
		var set = player is null ? null : this.Services.Loadouts.Find(System.IO.Path.GetDirectoryName(player.PlayerFile) ?? string.Empty);
		if (select is null && set?.FilePath == this._loadoutsShown)
			select = bar.Selected;

		this._loadoutsShown = set?.FilePath;
		// The PlayerReadonly setting keeps the equipment as it is: Gear swap is greyed, the other actions stay.
		string? equipmentLocked = this.USettings.PlayerReadonly
			? string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.LoadoutEquipmentLocked, Resources.SettingsPlayerReadonly)
			: null;
		bar.ShowLoadouts([.. set?.Loadouts.Select(l => l.Name) ?? []], select, set is not null && !set.ReadFailed && !player!.IsReadOnly, equipmentLocked);
		this.CompareWithSelectedLoadout();
		this.ShowSelectedLoadout(set);
		this.ShownLoadoutsChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>Why Update and Copy are refused (they write copies of pieces), or null when item copying is allowed.</summary>
	/// <param name="allowItemCopy">The Allow Item Copying setting.</param>
	internal static string? LoadoutCopyLocked(bool allowItemCopy)
		=> allowItemCopy ? null : string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.LoadoutCopyLocked, Resources.SettingsAllowCopy);

	/// <summary>
	/// Shows the loadout selected in the bar in the loadout panel, in place of the statistics panel, while the panel is
	/// open; the statistics panel otherwise. A read-only character's loadouts are composed too (their file is apart from
	/// the save), never those of a file that could not be read.
	/// </summary>
	/// <param name="set">The shown character's loadouts.</param>
	private void ShowSelectedLoadout(TQVaultAE.Application.Session.LoadoutSet? set)
	{
		if (this._room is not { } room || this._loadoutView is not { } view || this._loadoutBar is not { } bar)
			return;

		var loadout = set is { ReadFailed: false } ? set.Find(bar.Selected) : null;
		bool show = this._loadoutPanelOpen && bar.Visible && loadout is not null;

		// The slots follow the selected loadout even while the statistics show: a piece held from them stays valid.
		view.Panel.ShowLoadout(loadout is null ? null : set, loadout?.Name, this.Player);
		view.CanTurn = set is { ReadFailed: false, Loadouts.Count: > 1 };
		view.SwapLocked = bar.CanAct(LoadoutBarAction.Swap) ? null : bar.LockedReason(LoadoutBarAction.Swap) ?? Resources.LoadoutSwap;
		view.EmptyLocked = bar.CanEmpty ? null : bar.LockedReason(LoadoutBarAction.EmptyToStash) ?? Resources.LoadoutEmpty;
		if (room.LoadoutShown != show)
		{
			room.LoadoutShown = show;
			if (!show)
				this._statsLook?.Tooltip?.Hide();
		}
	}

	/// <summary>
	/// The previous (-1) or next (+1) loadout of the character in the loadout panel, in the order of the bar's list,
	/// the last one followed by the first.
	/// </summary>
	/// <param name="step">-1 or +1.</param>
	private void TurnLoadout(int step)
	{
		if (this._loadoutBar is not { } bar || this.Player is not { } player)
			return;

		var set = this.Services.Loadouts.Find(System.IO.Path.GetDirectoryName(player.PlayerFile) ?? string.Empty);
		if (set is not { ReadFailed: false } || set.Loadouts.Count < 2)
			return;

		var names = set.Loadouts.Select(l => l.Name).ToList();
		int index = names.FindIndex(n => string.Equals(n, bar.Selected, StringComparison.OrdinalIgnoreCase));
		int next = (((index < 0 ? 0 : index + step) % names.Count) + names.Count) % names.Count;
		this._loadoutPanelOpen = true;
		this.RefreshLoadouts(names[next]);
	}

	/// <summary>The Empty loadout button of the loadout panel: the bar's menu of destinations, under the button.</summary>
	private void ShowEmptyMenu()
	{
		if (this._loadoutBar is { } bar && this._loadoutView is { } view)
			bar.EmptyMenu.Show(view.EmptyButton, new Point(0, view.EmptyButton.Height));
	}

	/// <summary>
	/// Selects a loadout in the bar and shows it in the loadout panel, in place of the statistics panel (a search result
	/// of a loadout piece); once the statistics panel is built when the Equipment tab was not shown yet.
	/// </summary>
	/// <param name="name">The loadout.</param>
	public void ShowLoadoutPanel(string? name)
	{
		this._loadoutPanelOpen = true;
		this.RefreshLoadouts(name);
	}

	/// <summary>The loadout panel gave the loadout new pieces: the marks of the equipment compare with them.</summary>
	private void LoadoutPiecesChanged(object? sender, EventArgs e)
	{
		this.CompareWithSelectedLoadout();
		this.equipmentPanel.Invalidate();
		this.ShownLoadoutsChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// The marks beside the equipment slots compare the worn pieces with the loadout selected in the bar; none without
	/// one, or when the loadouts cannot change (a read-only character, a file that could not be read).
	/// </summary>
	private void CompareWithSelectedLoadout()
	{
		Loadout? loadout = null;
		if (this._loadoutBar is { CanChange: true, Selected: { } name } && this.Player is { } player)
			loadout = this.Services.Loadouts.Find(System.IO.Path.GetDirectoryName(player.PlayerFile) ?? string.Empty)?.Find(name);

		this.equipmentPanel.ComparedLoadout = loadout;
	}

	private CharacterStatsPanel EnsureStatsPanel()
	{
		if (this._statsPanel is not null)
			return this._statsPanel;

		var stats = this._stats = new CharacterStatsSource(this.Services);
		this._statsLook = new CharacterStatsLook(this.FontService, this.UIService);
		this._statsPanel = new CharacterStatsPanel(this.UIService, this.FontService)
		{
			Name = "PlayerStatsPanel",
			View = () => stats.View(this.Player, this.equipmentPanel.WornItems),
			Visible = false,
		};

		// The Buffs tab's toggles and the weapon set gems change only what the panel shows, never the save.
		stats.BindBuffs(this._statsPanel, this._statsPanel.Invalidate);
		stats.BindWeaponSets(this._statsPanel, this._statsPanel.Invalidate);

		// The gold button of the dialogs; its font and bounds follow the scale in LayoutStatsPanel.
		var editButton = CaravanDialog.Button(this.FontService, this.UIService.FontZoom, "editButton", Resources.CharacterEditBtn);
		editButton.DialogResult = DialogResult.OK;
		editButton.Padding = new Padding(10, 0, 10, 0);
		editButton.Visible = false;
		editButton.Click += EditButton_Click;
		this._editButton = editButton;

		// Beside Edit: the character in a floating window, with the four tabs at once.
		var floatButton = CaravanDialog.Button(this.FontService, this.UIService.FontZoom, "floatButton", Resources.CharacterWindowBtn);
		floatButton.Padding = new Padding(10, 0, 10, 0);
		floatButton.Visible = false;
		floatButton.Click += (_, _) => this.FloatCharacterClicked?.Invoke(this, EventArgs.Empty);
		this._floatButton = floatButton;

		this._loadoutBar = new LoadoutBar(this.UIService, this.FontService)
		{
			Name = "loadoutBar",
			Visible = false,
			CopyLocked = () => LoadoutCopyLocked(this.USettings.AllowItemCopy),
		};
		this._loadoutBar.ActionRequested += (_, e) =>
		{
			if (e.Action == LoadoutBarAction.Select)
			{
				// Choosing a loadout shows it in the loadout panel, in place of the statistics.
				this.CompareWithSelectedLoadout();
				this._loadoutPanelOpen = true;
				this.RefreshLoadouts();
			}
			this.OnLoadoutAction?.Invoke(this, e);
		};

		this._loadoutView = new LoadoutView(this.DragInfo, this.Services);
		this._loadoutView.BackRequested += (_, _) => this.LoadoutPanelOpen = false;
		this._loadoutView.TurnRequested += (_, step) => this.TurnLoadout(step);
		this._loadoutView.SwapRequested += (_, _) => this._loadoutBar.Request(LoadoutBarAction.Swap);
		this._loadoutView.EmptyRequested += (_, _) => this.ShowEmptyMenu();
		this._loadoutView.Panel.PiecesChanged += this.LoadoutPiecesChanged;
		this.ForwardEvents(this._loadoutView.Panel);

		this._room = new CharacterRoom(this._statsPanel, this._loadoutView, this._loadoutBar, editButton, floatButton) { Visible = false };
		this.Controls.Add(this._room);
		this._room.BringToFront();
		this.Disposed += (_, _) => this._statsLook?.Dispose();
		return this._statsPanel;
	}

	/// <summary>The base font changed: the statistics panel fonts and tool tip are made again from it.</summary>
	protected override void OnFontsRebased()
	{
		if (this._statsPanel is not { } panel)
			return;

		this._statsLook?.Reset();
		this.LayoutStatsPanel(panel);
		panel.Invalidate();
	}

	/// <summary>
	/// Places the room of the statistics on the black field of the equipment background, the statistics panel at the
	/// game's size times the scale on the box of the background; the room lays out its parts. The fonts follow the scale.
	/// </summary>
	private void LayoutStatsPanel(CharacterStatsPanel panel)
	{
		var scale = this.UIService.Scale;
		var look = this._statsLook!;
		if (look.Apply([panel], scale))
		{
			this.equipmentPanel.MarkTooltip = look.Tooltip;
			this._loadoutBar!.TextFont = look.Light(15F, FontStyle.Regular);
			this._loadoutBar.BarScale = scale;
			this._editButton!.Font = this.FontService.GetFontLight(11F, this.UIService.FontZoom);
			this._floatButton!.Font = this.FontService.GetFontLight(11F, this.UIService.FontZoom);
		}

		// The room: from the statistics panel's left to the black field's right, from the field's top to its bottom.
		var location = new Point(PLAYERINFO_TOPRIGHT - PLAYERINFO_WIDTH, PLAYERINFO_TOPHEIGHT);
		int gap = Convert.ToInt32(4 * scale);
		var field = this.EquipmentField();
		var roomBounds = Rectangle.FromLTRB(location.X, Math.Min(location.Y, field.Top + gap), field.Right - gap, field.Bottom - gap);
		var room = this._room!;
		if (room.Bounds != roomBounds)
			room.Bounds = roomBounds;

		// The Edit button is sized from its design at the scale, as the panels are.
		room.Arrange(
			new Point(location.X - roomBounds.X, location.Y - roomBounds.Y),
			TQVaultAE.Application.Layout.WindowLayout.Scaled(EditButtonDesignSize, scale),
			gap);

		if (this.EquipmentSlotsPicture() is var (picture, source, target))
			this._loadoutView!.LayOut(picture, source, target, this.equipmentPanel.SizeFor(LoadoutPanel.SlotCells));
	}

	/// <summary>
	/// The part of the equipment background with the slots, where it lands relative to the equipment panel, and the size of
	/// the slot cells (<see cref="LoadoutPanel.SlotCells"/>), at the window's scale: the floating character windows draw
	/// the same slots.
	/// </summary>
	/// <returns>Null before the panel is laid out.</returns>
	internal (Bitmap Picture, Rectangle Source, Rectangle Target, Size Slots)? SlotsGeometry()
		=> this.EquipmentSlotsPicture() is var (picture, source, target)
			? (picture, source, target, this.equipmentPanel.SizeFor(LoadoutPanel.SlotCells))
			: null;
}
