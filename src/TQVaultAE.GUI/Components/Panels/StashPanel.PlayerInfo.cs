//-----------------------------------------------------------------------
// <copyright file="StashPanel.PlayerInfo.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
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

	/// <summary>The Edit button under the statistics panel, which opens the character editor.</summary>
	private ScalingButton? _editButton;

	private CharacterPanelViewBuilder? _statsBuilder;
	private CharacterPanelView? _statsView;
	private GameTooltip? _statsTooltip;

	/// <summary>The fonts made for the statistics panel, at the scale they were made for.</summary>
	private readonly List<Font> _statsFonts = [];
	private float _statsScale;

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
	public event EventHandler<LoadoutBarEventArgs>? OnLoadoutAction;

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
			this._statsTooltip?.Hide();
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
		this._statsView = null;
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
		// The PlayerReadonly setting keeps the equipment as it is: Load is greyed, the other actions stay.
		string? loadLocked = this.USettings.PlayerReadonly
			? string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.LoadoutEquipmentLocked, Resources.SettingsPlayerReadonly)
			: null;
		bar.ShowLoadouts([.. set?.Loadouts.Select(l => l.Name) ?? []], select, set is not null && !set.ReadFailed && !player!.IsReadOnly, loadLocked);
		this.CompareWithSelectedLoadout();
		this.ShowSelectedLoadout(set);
	}

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
		view.CanSwap = bar.CanAct(LoadoutBarAction.Swap);
		if (room.LoadoutShown != show)
		{
			room.LoadoutShown = show;
			if (!show)
				this._statsTooltip?.Hide();
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

		this._statsBuilder = new CharacterPanelViewBuilder(this.Services.CharacterStatsService, this.TranslationService);
		this._statsPanel = new CharacterStatsPanel(this.UIService)
		{
			Name = "PlayerStatsPanel",
			View = () => this._statsView ??= this.BuildStatsView(),
			Visible = false,
		};

		// The gold button of the dialogs; its font and bounds follow the scale in LayoutStatsPanel.
		var editButton = CaravanDialog.Button(this.FontService, this.UIService.FontZoom, "editButton", Resources.CharacterEditBtn);
		editButton.DialogResult = DialogResult.OK;
		editButton.Padding = new Padding(10, 0, 10, 0);
		editButton.Visible = false;
		editButton.Click += EditButton_Click;
		this._editButton = editButton;

		this._loadoutBar = new LoadoutBar(this.UIService, this.FontService) { Name = "loadoutBar", Visible = false };
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
		this._loadoutView.Panel.PiecesChanged += this.LoadoutPiecesChanged;
		this.ForwardEvents(this._loadoutView.Panel);

		this._room = new CharacterRoom(this._statsPanel, this._loadoutView, this._loadoutBar, editButton) { Visible = false };
		this.Controls.Add(this._room);
		this._room.BringToFront();
		this.Disposed += (_, _) =>
		{
			this._statsTooltip?.Dispose();
			foreach (var font in this._statsFonts)
				font.Dispose();
		};
		return this._statsPanel;
	}

	/// <summary>The base font changed: the statistics panel fonts and tool tip are made again from it.</summary>
	protected override void OnFontsRebased()
	{
		if (this._statsPanel is not { } panel)
			return;

		this._statsScale = 0;
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
		if (scale != this._statsScale)
		{
			this._statsScale = scale;
			var old = this._statsFonts.ToList();
			this._statsFonts.Clear();
			Font Owned(Font font)
			{
				this._statsFonts.Add(font);
				return font;
			}

			panel.PanelScale = scale;
			panel.NameFont = this.FontService.GetFontLight(20F * scale, FontStyle.Regular, GraphicsUnit.Pixel);
			panel.ClassFont = this.FontService.GetFontLight(16F * scale, FontStyle.Regular, GraphicsUnit.Pixel);
			panel.TextFont = this.FontService.GetFontLight(15F * scale, FontStyle.Regular, GraphicsUnit.Pixel);
			panel.TitleFont = this.FontService.GetFontLight(16F * scale, FontStyle.Bold, GraphicsUnit.Pixel);
			panel.TabFont = this.FontService.GetFontLight(15F * scale, FontStyle.Regular, GraphicsUnit.Pixel);
			panel.NumberFont = Owned(new Font("Arial", 13F * scale, FontStyle.Bold, GraphicsUnit.Pixel));
			panel.ResistanceFont = Owned(new Font("Arial", 14F * scale, FontStyle.Regular, GraphicsUnit.Pixel));

			this._statsTooltip?.Dispose();
			this._statsTooltip = new GameTooltip(
				this.FontService.GetFontLight(17F * scale, FontStyle.Bold, GraphicsUnit.Pixel),
				this.FontService.GetFontLight(15F * scale, FontStyle.Regular, GraphicsUnit.Pixel),
				scale,
				GameTooltipFrame.Load(this.UIService));
			panel.Tooltip = this._statsTooltip;
			this.equipmentPanel.MarkTooltip = this._statsTooltip;
			this._loadoutBar!.TextFont = this.FontService.GetFontLight(15F * scale, FontStyle.Regular, GraphicsUnit.Pixel);
			this._loadoutBar.BarScale = scale;
			this._editButton!.Font = this.FontService.GetFontLight(11F, this.UIService.FontZoom);
			foreach (var font in old)
				font.Dispose();
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

	/// <summary>What the statistics panel shows of the character, from its save: nothing can be changed here.</summary>
	private CharacterPanelView BuildStatsView()
	{
		if (this.Player is not { PlayerInfo: { } info } player || this._statsBuilder is null)
			return new CharacterPanelView();

		// The items the item requirements count (PlayerInfo's calculated attributes): an equipped item in hand is left out.
		var worn = this.equipmentPanel.WornItems;
		var skills = this.Services.CharacterEditService.CreateSkillEditor(info);
		return this._statsBuilder.Build(CharacterPanelInput.FromSave(
			player, info, worn, [.. skills.Slots.OfType<MasteryTree>()], this.Services.CharacterEditService.GetGearSkillAugments(worn, player.UseAlternate)));
	}
}
