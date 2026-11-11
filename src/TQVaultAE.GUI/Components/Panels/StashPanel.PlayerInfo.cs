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
	/// Calculates the stat bonuses for the skills and updates the player
	/// </summary>
	private void GetSkillStatBonus()
	{
		if (this.Player is not null)
			this.Services.PlayerService.UpdateSkillStatBonuses(this.Player);
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
			this._statsTooltip?.Hide();
			return;
		}

		var panel = this.EnsureStatsPanel();
		this.LayoutStatsPanel(panel);
		panel.Visible = true;
		this._editButton!.Visible = this.USettings.AllowCharacterEdit;

		if (DisplayPlayerInfoLastName == this.Player!.PlayerName)
			return;

		// Another character, or its equipment or skills changed: the panel is built again.
		DisplayPlayerInfoLastName = this.Player.PlayerName;
		this._statsView = null;
		panel.Invalidate();
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
			Size = new Size(137, 30),
			SizeToGraphic = false,
			Text = Resources.CharacterEditBtn,
			UpBitmap = Images.MainButtonUp,
			UseCustomGraphic = true,
			UseVisualStyleBackColor = false,
			Font = FontService.GetFontLight(11F),
			Padding = new Padding(10, 0, 10, 0),
			Visible = false,
		};
		editButton.FlatAppearance.BorderSize = 0;
		editButton.FlatAppearance.MouseDownBackColor = Theme.TransparentButton;
		editButton.FlatAppearance.MouseOverBackColor = Theme.TransparentButton;
		editButton.Click += EditButton_Click;
		this._editButton = editButton;

		this.Controls.Add(this._statsPanel);
		this.Controls.Add(editButton);
		this._statsPanel.BringToFront();
		editButton.BringToFront();
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
	/// Edit button under its gold bar; its fonts follow the scale.
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
			foreach (var font in old)
				font.Dispose();
		}

		var location = new Point(PLAYERINFO_TOPRIGHT - PLAYERINFO_WIDTH, PLAYERINFO_TOPHEIGHT);
		if (panel.Location != location)
			panel.Location = location;

		var button = this._editButton!;
		var buttonAt = new Point(location.X, panel.Bottom + Convert.ToInt32(4 * scale));
		if (button.Location != buttonAt)
			button.Location = buttonAt;
	}

	/// <summary>What the statistics panel shows of the character, from its save: nothing can be changed here.</summary>
	private CharacterPanelView BuildStatsView()
	{
		if (this.Player is not { PlayerInfo: { } info } player || this._statsBuilder is null)
			return new CharacterPanelView();

		var skills = this.Services.CharacterEditService.CreateSkillEditor(info);
		return this._statsBuilder.Build(new CharacterPanelInput
		{
			Player = player,
			Info = info,
			Attributes = [info.BaseHealth, info.BaseMana, info.BaseStrength, info.BaseIntelligence, info.BaseDexterity],
			AttributePoints = info.AttributesPoints,
			Level = info.CurrentLevel,
			Xp = info.CurrentXP,
			Skills = info.SkillRecordList,
			Trees = skills.Slots.OfType<MasteryTree>().ToList(),
			Augments = this.Services.CharacterEditService.GetGearSkillAugments(player),
			Difficulty = Math.Clamp(info.DifficultyUnlocked, 0, 2),
		});
	}
}
