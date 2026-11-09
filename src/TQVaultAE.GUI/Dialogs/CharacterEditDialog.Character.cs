using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Components.Character;
using TQVaultAE.Presentation;
using TQVaultAE.Presentation.Win32;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// The frame of the character edit dialog, drawn as the game's skill window (<see cref="GameTabControl"/>), and its
/// Character page: the statistics panel of the game's character window (<see cref="CharacterStatsPanel"/>) with the
/// attribute "+" buttons and the leveling editors, the gold bar and the reset buttons.
/// </summary>
internal sealed partial class CharacterEditDialog
{
	private GameTabControl _tabs = null!;
	private CharacterStatsPanel _stats = null!;
	private readonly List<Font> _ownedFonts = [];

	/// <summary>The attribute boxes in the game's order: Health, Energy, Strength, Intelligence, Dexterity.</summary>
	private NumericUpDown[] AttributeBoxes => [this.healthUpDown, this.manacUpDown, this.strengthUpDown, this.intelligenceUpDown, this.dexterityUpDown];

	/// <summary>The leveling editors, in row order under the "Leveling" title of tab I.</summary>
	private Control[] LevelingEditors => [this.levelNumericUpDown, this.xpTextBox, this.skillPointsNumericUpDown, this.difficultlyComboBox, this.levelingCheckBox];

	/// <summary>The attribute values at the start, for Shift + right click.</summary>
	private int[] _startAttributes = [];

	/// <summary>The size of a game pixel on screen: the frame is drawn at the game's size times the DPI factor.</summary>
	private float GameScale => this.DeviceDpi / 96F;

	/// <summary>Builds the game frame, its Character page and moves the editors of the designer into it.</summary>
	private void InitCharacterPage()
	{
		var scale = this.GameScale;
		this.UseMarbleBackground();
		this._tabs = new GameTabControl(this.UIService)
		{
			Name = "EditorTabs",
			FrameScale = scale,
			Location = new Point(this.LogicalToDeviceUnits(14), this.LogicalToDeviceUnits(26)),
			TabFont = this.FontService.GetFontLight(16F * scale, FontStyle.Regular, GraphicsUnit.Pixel),
		};
		this.Controls.Add(this._tabs);

		// The page shows the dialog's marble rather than the black of the frame, where the game draws its mastery panes.
		var page = new Panel { Name = "CharacterPage", BackgroundImage = this.BackgroundImage, BackgroundImageLayout = ImageLayout.Stretch };

		// The panel at 1.24 times the game size fills the height of the page with the gold bar under it.
		var panelScale = scale * 1.24F;
		this._stats = new CharacterStatsPanel(this.UIService)
		{
			Name = "CharacterStatsPanel",
			PanelScale = panelScale,
			Location = new Point((int)Math.Round(24 * scale), (int)Math.Round(8 * scale)),
			View = this.CharacterView,
			NameFont = this.FontService.GetFontLight(20F * panelScale, FontStyle.Regular, GraphicsUnit.Pixel),
			ClassFont = this.FontService.GetFontLight(16F * panelScale, FontStyle.Regular, GraphicsUnit.Pixel),
			TextFont = this.FontService.GetFontLight(15F * panelScale, FontStyle.Regular, GraphicsUnit.Pixel),
			TitleFont = this.FontService.GetFontLight(16F * panelScale, FontStyle.Bold, GraphicsUnit.Pixel),
			TabFont = this.FontService.GetFontLight(15F * panelScale, FontStyle.Regular, GraphicsUnit.Pixel),
			NumberFont = this.OwnedFont(new Font("Arial", 13F * panelScale, FontStyle.Bold, GraphicsUnit.Pixel)),
		};
		page.Controls.Add(this._stats);

		// The attribute boxes stay the model of the attributes (their events count the points); the panel shows them.
		foreach (var box in this.AttributeBoxes.Append(this.attributeNumericUpDown))
			box.Visible = false;

		var editorFont = this.OwnedFont(new Font("Arial", 12F * panelScale, FontStyle.Bold, GraphicsUnit.Pixel));
		var editors = this.LevelingEditors;
		for (int row = 0; row < editors.Length; row++)
		{
			var editor = editors[row];
			editor.Parent = this._stats;
			editor.Bounds = this._stats.LevelingRow(row);
			editor.Font = editorFont;
			editor.BackColor = Color.FromArgb(24, 24, 24);
			editor.ForeColor = Color.White;
			this._stats.Tab1Controls.Add(editor);
			if (editor is CheckBox check)
			{
				check.Text = string.Empty;
				check.BackColor = Color.Black;
				check.AutoSize = true;
			}

			// A disabled box ignores its colours: it is hidden and the panel writes its value, as the game writes numbers.
			editor.EnabledChanged += (_, _) => this.ShowLevelingEditor(editor);
			editor.TextChanged += (_, _) => this._stats.Invalidate();
			this.ShowLevelingEditor(editor);
		}

		// The reset buttons, right of the panel.
		var buttonX = this._stats.Right + (int)Math.Round(40 * scale);
		this.ResetAttributesScalingButton.Parent = page;
		this.ResetAttributesScalingButton.Location = new Point(buttonX, this._stats.Top + (int)Math.Round(60 * scale));
		this.ResetMasteriesScalingButton.Parent = page;
		this.ResetMasteriesScalingButton.Location = new Point(buttonX, this.ResetAttributesScalingButton.Bottom + (int)Math.Round(16 * scale));

		this._tabs.AddPage(Resources.CECharacter, page);
		this.AddMedallion(page, scale);

		this._stats.AttributeClicked += (_, click) => this.CharacterAttribute_Clicked(click.Attribute, click.Step, click.ToEnd);
		this._stats.UndoClicked += (_, _) => this.CharacterAttributes_Undo();
		foreach (var box in this.AttributeBoxes.Append(this.attributeNumericUpDown))
			box.ValueChanged += (_, _) => this._stats.Invalidate();

		this.FormClosed += (_, _) =>
		{
			foreach (var font in this._ownedFonts)
				font.Dispose();
		};
	}

	/// <summary>
	/// TQVault's medallion picture (<see cref="Images.tqmedallion"/>) in the free part of the Character page, right of the
	/// statistics panel and under the reset buttons, at most twice its size so that it stays sharp.
	/// </summary>
	private void AddMedallion(Panel page, float scale)
	{
		var medallion = Images.tqmedallion;
		var area = Rectangle.FromLTRB(
			this._stats.Right + (int)Math.Round(40 * scale),
			this.ResetMasteriesScalingButton.Bottom + (int)Math.Round(30 * scale),
			page.Width - (int)Math.Round(24 * scale),
			page.Height - (int)Math.Round(24 * scale));
		var zoom = Math.Min(2F * scale, Math.Min(area.Width / (float)medallion.Width, area.Height / (float)medallion.Height));
		var size = new Size((int)Math.Round(medallion.Width * zoom), (int)Math.Round(medallion.Height * zoom));
		var picture = new PictureBox
		{
			Name = "CharacterMedallion",
			Image = medallion,
			SizeMode = PictureBoxSizeMode.Zoom,
			BackColor = Color.Transparent,
			Size = size,
			Location = new Point(area.X + (area.Width - size.Width) / 2, area.Y + (area.Height - size.Height) / 2),
		};
		page.Controls.Add(picture);
		this.FormClosed += (_, _) => medallion.Dispose();
	}

	/// <summary>Rows at the top of <see cref="Images.MainForm_NewBackground"/> taken by its dark band, left out.</summary>
	private const int MarbleTop = 60;

	/// <summary>Width of the black border around <see cref="Images.MainForm_NewBackground"/>, left out.</summary>
	private const int MarbleBorder = 6;

	/// <summary>
	/// The dialog's background: the plain marble of TQVault's own background picture (without its dark top band and
	/// its border), stretched to the dialog, around the game frame and under OK and Cancel.
	/// </summary>
	private void UseMarbleBackground()
	{
		using var source = Images.MainForm_NewBackground;
		var marble = source.Clone(
			new Rectangle(MarbleBorder, MarbleTop, source.Width - 2 * MarbleBorder, source.Height - MarbleTop - MarbleBorder),
			source.PixelFormat);
		this.BackgroundImage = marble;
		this.BackgroundImageLayout = ImageLayout.Stretch;
		this.FormClosed += (_, _) => marble.Dispose();
	}

	/// <summary>A font the dialog made itself, disposed with it (never one of the font service's, which are shared).</summary>
	private Font OwnedFont(Font font)
	{
		this._ownedFonts.Add(font);
		return font;
	}

	/// <summary>Shows a leveling editor only while it can be changed and tab I is shown.</summary>
	private void ShowLevelingEditor(Control editor)
	{
		editor.Visible = editor.Enabled && this._stats.Tab == 0;
		this._stats.Invalidate();
	}

	/// <summary>Sizes the dialog around the frame, with OK and Cancel under it.</summary>
	private void FitToFrame()
	{
		var margin = this.LogicalToDeviceUnits(14);
		var gap = this.LogicalToDeviceUnits(10);
		this.ClientSize = new Size(this._tabs.Right + margin, this._tabs.Bottom + gap + this.ok.Height + margin);
		this.cancel.Location = new Point(this._tabs.Right - this.cancel.Width, this._tabs.Bottom + gap);
		this.ok.Location = new Point(this.cancel.Left - gap - this.ok.Width, this.cancel.Top);
	}

	/// <summary>What the statistics panel shows, read from the editors and the save.</summary>
	private CharacterPanelView CharacterView()
	{
		var info = this.Info;
		var culture = CultureInfo.CurrentCulture;
		string Game(string tag, string fallback) => this.TranslateTag(tag, fallback);

		var gear = info.GearBonus;
		var skill = info.SkillBonus;
		CharacterAttributeView Attribute(int index, string tag, string label, int flat, float percent, Color color)
		{
			var value = Convert.ToInt32(this.AttributeBoxes[index].Value);
			var total = CharacterPanelLayout.AttributeTotal(value, flat, percent);
			var shown = index < 2 ? $"{total} / {total}" : total.ToString(culture);
			var tip = string.Format(culture, Resources.CEAttributeToolTip, value, total - value);
			return new CharacterAttributeView(Game(tag, label), shown, color, tip);
		}

		var points = Convert.ToInt32(this.attributeNumericUpDown.Value);
		var level = Convert.ToInt32(this.levelNumericUpDown.Value);
		var xp = long.TryParse(this.xpTextBox.Text, NumberStyles.Integer, culture, out var xpValue) ? xpValue : 0;
		var skills = this._skills;

		return new CharacterPanelView
		{
			Name = this.PlayerCollection.PlayerName,
			ClassName = this.TranslationService.TranslateXTag(info.Class ?? string.Empty, true, true) ?? string.Empty,
			LevelText = CharacterPanelLayout.FormatGameText(Game("tagCLevel", "Level : {%d0}"), level),
			XpText = CharacterPanelLayout.FormatGameText(Game("xtagCXP", "XP : {%t0}"), xp),
			AttributesTitle = Game("tagCAttributes", Resources.CEAttributes),
			PointsText = points <= 0 ? string.Empty
				: CharacterPanelLayout.FormatGameText(this.TranslationService.TranslateXTag(points == 1 ? "tagCPointsSingular" : "tagCPoints") ?? "{%d0}", points),
			CanRaise = points > 0,
			CanUndo = this.AttributeBoxes.Where((b, i) => Convert.ToInt32(b.Value) != this._startAttributes.ElementAtOrDefault(i)).Any(),
			UndoText = Game("tagSkillUndo", "Undo"),
			Attributes =
			[
				Attribute(0, "tagCHealth", Resources.CEHealth, (gear?.HealthBonus ?? 0) + (skill?.HealthBonus ?? 0), (gear?.HealthModifier ?? 0) + (skill?.HealthModifier ?? 0), Color.FromArgb(255, 40, 40)),
				Attribute(1, "tagCEnergy", Resources.CEMana, (gear?.ManaBonus ?? 0) + (skill?.ManaBonus ?? 0), (gear?.ManaModifier ?? 0) + (skill?.ManaModifier ?? 0), Color.FromArgb(90, 90, 255)),
				Attribute(2, "tagCStrength", Resources.CEStrength, (gear?.StrengthBonus ?? 0) + (skill?.StrengthBonus ?? 0), (gear?.StrengthModifier ?? 0) + (skill?.StrengthModifier ?? 0), Color.White),
				Attribute(3, "tagCIntelligence", Resources.CEIntelligence, (gear?.IntelligenceBonus ?? 0) + (skill?.IntelligenceBonus ?? 0), (gear?.IntelligenceModifier ?? 0) + (skill?.IntelligenceModifier ?? 0), Color.White),
				Attribute(4, "tagCDexterity", Resources.CEDexterity, (gear?.DexterityBonus ?? 0) + (skill?.DexterityBonus ?? 0), (gear?.DexterityModifier ?? 0) + (skill?.DexterityModifier ?? 0), Color.White),
			],
			LevelingTitle = Resources.CELeveling,
			LevelingLabels =
			[
				Resources.CELevel,
				Resources.CEXp,
				Resources.CESkillPoints,
				Resources.CEDifficulty,
				this.levelingCheckBox.Enabled ? Resources.CEEnableLeveling : string.Empty,
			],
			LevelingValues =
			[
				this.levelNumericUpDown.Enabled ? string.Empty : level.ToString(culture),
				this.xpTextBox.Enabled ? string.Empty : xp.ToString("N0", culture),
				this.skillPointsNumericUpDown.Enabled ? string.Empty : Convert.ToInt32(this.skillPointsNumericUpDown.Value).ToString(culture),
				this.difficultlyComboBox.Enabled ? string.Empty : this.TranslationService.TranslateDifficulty(info.DifficultyUnlocked) ?? string.Empty,
				string.Empty,
			],
			MasteriesTitle = Resources.Masteries,
			Masteries = skills is null ? [] : [.. Enumerable.Range(0, skills.Slots.Count)
				.Select(skills.GetMasterySummary)
				.OfType<MasterySummary>()
				.Select(s => (this.TranslateTag(s.Mastery.NameTag, s.Mastery.Mastery.ToString()),
					string.Format(culture, Resources.CEMasterySummary, s.SkillCount, s.PointsSpent)))],
			GreatestMonsterTitle = Game("tagCHighestMonster", "Greatest Monster Killed"),
			GreatestMonster = this.TranslateTag(info.GreatestMonster, info.GreatestMonster),
			GreatestMonsterLevel = info.GreatestMonsterKilledLevel == 0 ? string.Empty
				: CharacterPanelLayout.FormatGameText(Game("tagCLevel", "Level : {%d0}"), info.GreatestMonsterKilledLevel),
			Statistics =
			[
				(Game("tagCMonsterKills", "Monsters Killed"), info.NumberOfKills.ToString("N0", culture)),
				(Game("tagCHighestDamage", "Greatest Damage Dealt"), info.GreatestDamageInflicted.ToString("N0", culture)),
				(Game("tagCTime", "Elapsed Time"), CharacterPanelLayout.PlayTime(Game("tagCTimeNumber", "{%d0}:{%d1}:{%d2}"), info.PlayTimeInSeconds)),
				(Game("tagCDeaths", "Total Deaths"), info.NumberOfDeaths.ToString("N0", culture)),
			],
			ComputedInGame = Resources.CEComputedInGame,
			TabNames = [Game("tagCPrimaryStats", "I"), Game("tagCSecondaryStats", "II"), Game("x4tagCThirdStats", "III")],
			Money = info.Money.ToString("N0", culture),
			Electrum = info.Electrum?.ToString("N0", culture) ?? string.Empty,
		};
	}

	/// <summary>
	/// The Undo button: every attribute goes back to its value at the start. The lowered ones first, which gives their
	/// points back for the raised ones.
	/// </summary>
	private void CharacterAttributes_Undo()
	{
		var boxes = this.AttributeBoxes;
		foreach (var i in Enumerable.Range(0, boxes.Length).OrderBy(i => Convert.ToInt32(boxes[i].Value) > this._startAttributes[i] ? 0 : 1))
			boxes[i].Value = Math.Clamp(this._startAttributes[i], Convert.ToInt32(boxes[i].Minimum), Convert.ToInt32(boxes[i].Maximum));

		this._stats.Invalidate();
	}

	/// <summary>
	/// A "+" button: left click spends an attribute point on the attribute, right click takes one back; with Shift all
	/// the points left, or back to the value at the start. The attribute box counts the points, as before.
	/// </summary>
	private void CharacterAttribute_Clicked(int attribute, int step, bool toEnd)
	{
		var box = this.AttributeBoxes[attribute];
		var increment = attribute < 2 ? PlayerLevel.HealthAndManaIncrementPerPoint : PlayerLevel.AtrributeIncrementPerPoint;
		var current = Convert.ToInt32(box.Value);
		var target = step > 0
			? current + increment * (toEnd ? Convert.ToInt32(this.attributeNumericUpDown.Value) : 1)
			: toEnd ? Math.Min(current, this._startAttributes[attribute]) : current - increment;
		target = Math.Clamp(target, Convert.ToInt32(box.Minimum), Convert.ToInt32(box.Maximum));
		if (target != current)
			box.Value = target;

		this._stats.Invalidate();
	}
}
