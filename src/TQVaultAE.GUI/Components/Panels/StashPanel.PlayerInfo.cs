//-----------------------------------------------------------------------
// <copyright file="StashPanel.PlayerInfo.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
/// <summary>
/// Class for handling the stash panel ui functions
/// </summary>

using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Providers;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.GUI.Components.Buttons;
using TQVaultAE.GUI.Components.Layout;
using TQVaultAE.GUI.Components.Scaling;
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
			if (this._statsPanel is not null)
				this._statsPanel.Visible = false;
			if (this._editButton is not null)
				this._editButton.Visible = false;
			if (this._loadoutBar is not null)
				this._loadoutBar.Visible = false;
			this._statsTooltip?.Hide();
			return;
		}

		var panel = this.EnsureStatsPanel();
		this.LayoutStatsPanel(panel);
		panel.Visible = true;
		this._editButton!.Visible = this.USettings.AllowCharacterEdit;
		this._loadoutBar!.Visible = true;

		// The loadouts may have changed (read again) or the character turned read-only: the bar follows, drawn again
		// only when it changes.
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

		var editButton = new ScalingButton()
		{
			BackColor = Color.Transparent,
			DialogResult = DialogResult.OK,
			DownBitmap = Images.MainButtonDown,
			FlatStyle = FlatStyle.Flat,
			ForeColor = Theme.ButtonText,
			Image = Images.MainButtonUp,
			Name = "editButton",
			OverBitmap = Images.MainButtonOver,
			SizeToGraphic = false,
			Text = Resources.CharacterEditBtn,
			UpBitmap = Images.MainButtonUp,
			UseCustomGraphic = true,
			UseVisualStyleBackColor = false,
			Padding = new Padding(10, 0, 10, 0),
			Visible = false,
		};
		editButton.FlatAppearance.BorderSize = 0;
		editButton.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
		editButton.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;
		editButton.Click += EditButton_Click;
		this._editButton = editButton;

		this._loadoutBar = new LoadoutBar(this.UIService) { Name = "loadoutBar", Visible = false };
		this._loadoutBar.ActionRequested += (_, e) => this.OnLoadoutAction?.Invoke(this, e);

		this.Controls.Add(this._statsPanel);
		this.Controls.Add(editButton);
		this.Controls.Add(this._loadoutBar);
		this._statsPanel.BringToFront();
		editButton.BringToFront();
		this._loadoutBar.BringToFront();
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
	/// Places the statistics panel on the box of the equipment background, at the game's size times the scale, with the
	/// loadout bar under its gold bar and the Edit button under the loadout bar; its fonts follow the scale.
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
			this._loadoutBar!.TextFont = this.FontService.GetFontLight(15F * scale, FontStyle.Regular, GraphicsUnit.Pixel);
			this._loadoutBar.BarScale = scale;
			this._editButton!.Font = this.FontService.GetFontLight(11F, this.UIService.FontZoom);
			foreach (var font in old)
				font.Dispose();
		}

		var location = new Point(PLAYERINFO_TOPRIGHT - PLAYERINFO_WIDTH, PLAYERINFO_TOPHEIGHT);
		if (panel.Location != location)
			panel.Location = location;

		// The loadout bar: the whole width of the statistics panel, under its gold bar; the Edit button on the line below.
		int gap = Convert.ToInt32(4 * scale);
		var barBounds = new Rectangle(location.X, panel.Bottom + gap, panel.Width, LoadoutBar.HeightAt(scale));
		if (this._loadoutBar!.Bounds != barBounds)
			this._loadoutBar.Bounds = barBounds;

		// Sized from its design at the scale, as the panels are: built in code, it was 137 x 30 whatever the DPI and zoom.
		var button = this._editButton!;
		var buttonBounds = new Rectangle(
			new Point(location.X, barBounds.Bottom + gap),
			TQVaultAE.Application.WindowLayout.Scaled(EditButtonDesignSize, scale));
		if (button.Bounds != buttonBounds)
			button.Bounds = buttonBounds;
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
