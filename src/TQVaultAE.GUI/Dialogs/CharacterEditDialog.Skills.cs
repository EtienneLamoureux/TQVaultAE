using TQVaultAE.Application.Contracts.Services;
using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// The mastery pages of the character edit dialog: one page per mastery slot, with the mastery, its level and its pane
/// drawn as the game's skill window. The rules are in <see cref="CharacterSkillEditor"/>; this part only shows its state
/// and passes the user's changes.
/// </summary>
internal sealed partial class CharacterEditDialog
{
	private CharacterSkillEditor? _skills;

	/// <summary>The skill bonuses of the equipped items.</summary>
	private IReadOnlyList<SkillAugment> _gearAugments = [];

	private readonly List<SkillTab> _skillTabs = [];

	/// <summary>The tool tips of the mastery level boxes.</summary>
	private readonly ToolTip _headerToolTip = new();

	/// <summary>True while the dialog writes the points left of the editor into the skill points box.</summary>
	private bool _syncingSkillPoints;

	/// <summary>The controls of the page of a mastery slot.</summary>
	private sealed class SkillTab
	{
		public required int Slot { get; init; }

		/// <summary>The index of its page in the frame.</summary>
		public required int PageIndex { get; init; }
		public required ComboBox Mastery { get; init; }
		public required NumericUpDown Level { get; init; }
		public required SkillPaneControl Pane { get; init; }

		/// <summary>True while the dialog fills the controls, so their events are not user changes.</summary>
		public bool Updating { get; set; }
	}

	/// <summary>An entry of the mastery list.</summary>
	private sealed record MasteryChoice(Masteries? Mastery, string Text)
	{
		public override string ToString() => this.Text;
	}

	/// <summary>Builds a page per mastery slot of the character, after the Character page.</summary>
	private void InitSkills()
	{
		this._skills = this.Services.CharacterEditService.CreateSkillEditor(this.Info);
		this._gearAugments = this.Services.CharacterEditService.GetGearSkillAugments(this.PlayerCollection);

		var choices = new List<MasteryChoice> { new(null, Resources.CEMasteryNone) };
		foreach (var tree in this._skills.KnownMasteries)
			choices.Add(new MasteryChoice(tree.Mastery, this.TranslationService.TranslateOr(tree.NameTag, tree.Mastery.ToString())));

		for (int slot = 0; slot < this._skills.Slots.Count; slot++)
			this._skillTabs.Add(this.CreateSkillTab(slot, choices));

		this.skillPointsNumericUpDown.ValueChanged += this.SkillPointsNumericUpDown_ValueChanged;

		this.RefreshSkills();
	}

	private SkillTab CreateSkillTab(int slot, List<MasteryChoice> choices)
	{
		var font = this.FontService.GetFontLight(11F);

		var mastery = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = this.LogicalToDeviceUnits(170), Name = $"SkillsMastery{slot + 1}ComboBox", Font = font };
		mastery.Items.AddRange([.. choices]);
		var level = new NumericUpDown { Minimum = 0, Maximum = 40, Width = this.LogicalToDeviceUnits(55), Name = $"SkillsMastery{slot + 1}LevelUpDown", Font = font };

		// The mastery and its level go on the marble strip right of the tabs, as the game's Undo button.
		var header = new FlowLayoutPanel { AutoSize = true, WrapContents = false, BackColor = Color.Transparent, Name = $"SkillsMastery{slot + 1}Header" };
		header.Controls.AddRange([mastery, level]);
		this._headerToolTip.SetToolTip(level, Resources.CEMasteryLevel);

		var pane = new SkillPaneControl(this.UIService)
		{
			Name = $"SkillsMastery{slot + 1}Pane",
			PaneScale = this.DeviceDpi / 96F,
			Translate = tag => this.TranslationService.TranslateOr(tag, tag),
			NumberFont = this.FontService.GetFontLight(12F, FontStyle.Regular, GraphicsUnit.Pixel),
			TextFont = this.FontService.GetFontLight(15F, FontStyle.Regular, GraphicsUnit.Pixel),
			DescriptionFont = this.FontService.GetFontLight(13F, FontStyle.Regular, GraphicsUnit.Pixel),
			Location = Point.Empty,
			// An empty slot shows the skill window of the first mastery without its content, not a black page.
			EmptyArt = this._skills?.KnownMasteries.Select(tree => tree.Pane).FirstOrDefault(art => art is not null),
		};
		var paneHost = new Panel { BackColor = Color.Black, Name = $"SkillsMastery{slot + 1}Page" };
		paneHost.Controls.Add(pane);

		var pageIndex = this._tabs.Pages.Count;
		this._tabs.AddPage(string.Format(Resources.CEMasterySlot, slot + 1), paneHost, header);

		var tab = new SkillTab { Slot = slot, PageIndex = pageIndex, Mastery = mastery, Level = level, Pane = pane };
		pane.SkillView = skill => this.SkillView(tab, skill);
		pane.ToolTipLines = skill => this.SkillToolTip(tab, skill);
		pane.Tooltip = this.SkillTooltipWindow();

		mastery.SelectedIndexChanged += (_, _) => this.SkillMastery_Changed(tab);
		level.ValueChanged += (_, _) => this.SkillMasteryLevel_Changed(tab);
		pane.ButtonClicked += (_, click) => this.SkillPane_ButtonClicked(tab, click);

		return tab;
	}

	/// <summary>What the pane shows of a skill: its level, the levels of the equipped items, its border.</summary>
	private SkillPaneSkillView SkillView(SkillTab tab, MasterySkill skill)
	{
		var skills = this._skills!;
		var tree = skills.Slots[tab.Slot];
		var level = skills.GetLevel(skill.Id);
		var masteryLevel = skills.GetMasteryLevel(tab.Slot);
		var bonus = tree is null ? 0 : SkillBonus.For(this._gearAugments, tree, skill.Id).Total;

		// The game writes the level up to the ultimate one: Call of the Wild 15 + 6 reads "20 / 16".
		if (skill.UltimateLevel > 0)
			bonus = Math.Min(bonus, Math.Max(0, skill.UltimateLevel - level));

		var parentHasPoints = skill.ParentId is null || skills.GetLevel(skill.ParentId) > 0;
		var state = SkillPaneLayout.State(level, skill.MaxLevel, masteryLevel, skill.RequiredMasteryLevel, parentHasPoints, skills.AvailablePoints);
		return new SkillPaneSkillView(level, bonus, state, masteryLevel >= skill.RequiredMasteryLevel);
	}

	// Colours read on the game's skill window.
	private static readonly Color TipText = Color.FromArgb(245, 243, 236);
	private static readonly Color TipLevel = Color.FromArgb(240, 238, 115);
	private static readonly Color TipNext = Color.FromArgb(165, 164, 158);
	private static readonly Color TipAbility = Color.FromArgb(70, 175, 255);
	private static readonly Color TipCanAdd = Color.FromArgb(100, 230, 90);
	private static readonly Color TipRefused = Color.FromArgb(235, 24, 18);
	private static readonly Color TipGear = Color.FromArgb(120, 200, 255);
	private static readonly Color TipHint = Color.FromArgb(150, 148, 135);

	/// <summary>The game-style tool tip of the skill panes, shared by the pages.</summary>
	private GameTooltip? _skillTooltip;

	private GameTooltip SkillTooltipWindow()
	{
		if (this._skillTooltip is null)
		{
			var scale = this.DeviceDpi / 96F;
			this._skillTooltip = new GameTooltip(
				this.FontService.GetFontLight(17F * scale, FontStyle.Bold, GraphicsUnit.Pixel),
				this.FontService.GetFontLight(15F * scale, FontStyle.Regular, GraphicsUnit.Pixel),
				scale,
				GameTooltipFrame.Load(this.UIService));
			var tooltip = this._skillTooltip;
			this.FormClosed += (_, _) => tooltip.Dispose();
		}

		return this._skillTooltip;
	}

	/// <summary>Builds the skill tool tips; the dialog only gives the colours.</summary>
	private SkillTooltipContentBuilder? _skillTipBuilder;

	/// <summary>The tool tip of a skill, or of the mastery button, in the colours of the game's skill window.</summary>
	private List<GameTooltipLine> SkillToolTip(SkillTab tab, MasterySkill? skill)
	{
		if (this._skills is not { } skills)
			return [];

		this._skillTipBuilder ??= new SkillTooltipContentBuilder(this.Services.CharacterEditService, this.Services.CharacterStatsService, this.TranslationService);
		var difficulty = CharacterPanelLayout.ShownDifficulty(this.Info.DifficultyUnlocked, this._statsDifficulty?.SelectedIndex ?? -1);
		return [.. this._skillTipBuilder.Build(skills, tab.Slot, skill, this._gearAugments, () => this.ComputeStats().CooldownReduction, difficulty)
			.Select(ToGameLine)];
	}

	private static GameTooltipLine ToGameLine(SkillTipLine line) => line.Kind switch
	{
		SkillTipKind.Blank => GameTooltipLine.Blank,
		SkillTipKind.Title => new GameTooltipLine(line.Text, Color.White, Title: true),
		SkillTipKind.Level => new GameTooltipLine(line.Text, TipLevel),
		SkillTipKind.Ability => new GameTooltipLine(line.Text, TipAbility),
		SkillTipKind.Next => new GameTooltipLine(line.Text, TipNext),
		SkillTipKind.Refused => new GameTooltipLine(line.Text, TipRefused),
		SkillTipKind.CanAdd => new GameTooltipLine(line.Text, TipCanAdd),
		SkillTipKind.Gear => new GameTooltipLine(line.Text, TipGear),
		SkillTipKind.Hint => new GameTooltipLine(line.Text, TipHint, Wrap: line.Wrap),
		_ => new GameTooltipLine(line.Text, TipText, Wrap: line.Wrap),
	};

	/// <summary>Shows the state of the editor in every page and in the skill points box.</summary>
	private void RefreshSkills()
	{
		if (this._skills is not { } skills)
			return;

		foreach (var tab in this._skillTabs)
		{
			tab.Updating = true;
			try
			{
				var tree = skills.Slots[tab.Slot];
				this._tabs.SetPageText(tab.PageIndex, tree is null ? string.Format(Resources.CEMasterySlot, tab.Slot + 1) : this.TranslationService.TranslateOr(tree.NameTag, tree.Mastery.ToString()));
				tab.Mastery.SelectedItem = tab.Mastery.Items.Cast<MasteryChoice>().First(c => c.Mastery == tree?.Mastery);

				var masteryLevel = skills.GetMasteryLevel(tab.Slot);
				tab.Level.Enabled = tree is not null;
				tab.Level.Minimum = tree is null ? 0 : 1;
				tab.Level.Maximum = Math.Max(tree?.MaxLevel ?? 0, masteryLevel);
				// A save may hold a mastery at level 0: within the box's bounds.
				tab.Level.Value = Math.Clamp(masteryLevel, tab.Level.Minimum, tab.Level.Maximum);

				tab.Pane.Tree = tree;
				tab.Pane.MasteryLevel = masteryLevel;
				tab.Pane.AvailablePoints = skills.AvailablePoints;
				tab.Pane.Invalidate();
			}
			finally
			{
				tab.Updating = false;
			}
		}

		this.InvalidateStats();

		this._syncingSkillPoints = true;
		try
		{
			if (skills.AvailablePoints > this.skillPointsNumericUpDown.Maximum)
				this.skillPointsNumericUpDown.Maximum = skills.AvailablePoints;

			this.skillPointsNumericUpDown.Value = Math.Max(skills.AvailablePoints, 0);
		}
		finally
		{
			this._syncingSkillPoints = false;
		}
	}

	private void SkillMastery_Changed(SkillTab tab)
	{
		if (tab.Updating || this._skills is null || tab.Mastery.SelectedItem is not MasteryChoice choice)
			return;

		// A refused choice (mastery in the other slot, no point left) is put back by the refresh.
		this._skills.SetMastery(tab.Slot, choice.Mastery);
		this.RefreshSkills();
	}

	private void SkillMasteryLevel_Changed(SkillTab tab)
	{
		if (tab.Updating || this._skills is null)
			return;

		this._skills.SetMasteryLevel(tab.Slot, Convert.ToInt32(tab.Level.Value));
		this.RefreshSkills();
	}

	/// <summary>
	/// A click on the pane: left raises the skill or the mastery by one, right lowers it; with Shift up to the highest
	/// level (within the points left) or down to the lowest. The editor applies its rules (tier, parent, points).
	/// </summary>
	private void SkillPane_ButtonClicked(SkillTab tab, SkillPaneClick click)
	{
		if (this._skills is not { } skills || skills.Slots[tab.Slot] is not { } tree)
			return;

		if (click.Skill is null)
		{
			var current = skills.GetMasteryLevel(tab.Slot);
			var level = click.ToEnd ? (click.Step > 0 ? tree.MaxLevel : 1) : current + click.Step;
			skills.SetMasteryLevel(tab.Slot, level);
		}
		else
		{
			var current = skills.GetLevel(click.Skill.Id);
			var level = click.ToEnd ? (click.Step > 0 ? click.Skill.MaxLevel : 0) : current + click.Step;
			skills.SetSkillLevel(tab.Slot, click.Skill.Id, level);
		}

		this.RefreshSkills();
		tab.Pane.RefreshToolTip();
	}

	private void SkillPointsNumericUpDown_ValueChanged(object? sender, EventArgs e)
	{
		if (this._syncingSkillPoints || this._skills is null)
			return;

		this._skills.AvailablePoints = Convert.ToInt32(this.skillPointsNumericUpDown.Value);
		this.RefreshSkills();
	}
}
