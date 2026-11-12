using TQVaultAE.Application.Contracts.Services;
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
	private CharacterSidePanel _side = null!;
	private readonly List<Font> _ownedFonts = [];

	/// <summary>The difficulty the statistics are computed for, Normal to Legendary.</summary>
	private ComboBox _statsDifficulty = null!;

	/// <summary>Whether the auras and buffs count in the statistics.</summary>
	private CheckBox _includeBuffs = null!;

	private readonly ToolTip _statsToolTip = new() { AutoPopDelay = 30000, InitialDelay = 300 };

	/// <summary>The sums of the equipped items and of the skills, kept by <see cref="ViewBuilder"/>.</summary>
	private AttributeSum? _gearSum => this.ViewBuilder.GearSum;
	private AttributeSum? _skillSum => this.ViewBuilder.SkillSum;

	/// <summary>The weapons in hand and the other items for the attack damage, and the stance.</summary>
	private OffenseGear? _offenseGear => this.ViewBuilder.OffenseGear;
	private WeaponStance? _stance => this.ViewBuilder.Stance;

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
		this._tabs = new GameTabControl
		{
			Name = "EditorTabs",
			FrameScale = scale,
			Location = new Point(this.LogicalToDeviceUnits(14), this.LogicalToDeviceUnits(26)),
			TabFont = this.FontService.GetFontLight(16F * scale, FontStyle.Regular, GraphicsUnit.Pixel),
		};
		this.Controls.Add(this._tabs);

		// The page shows the dialog's marble rather than the black of the frame, where the game draws its mastery panes.
		var page = new BufferedPanel { Name = "CharacterPage", BackgroundImageLayout = ImageLayout.None };

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
			ResistanceFont = this.OwnedFont(new Font("Arial", 14F * panelScale, FontStyle.Regular, GraphicsUnit.Pixel)),
		};
		this._stats.Tooltip = this.SkillTooltipWindow();
		page.Controls.Add(this._stats);

		// The attribute boxes stay the model of the attributes (their events count the points); the panel shows them.
		foreach (var box in this.AttributeBoxes.Append(this.attributeNumericUpDown))
			box.Visible = false;

		// The leveling editors, the masteries and the statistics options, right of the panel where the game window has
		// none: the game draws defense and offense where the editor had them.
		this._side = new CharacterSidePanel(this.UIService)
		{
			Name = "CharacterSidePanel",
			PanelScale = panelScale,
			// Its top on the statistics panel's frame, under the tab strip (the frame's gold line is 20 pixels down).
			Location = new Point(this._stats.Right + (int)Math.Round(40 * scale), this._stats.Top + (int)Math.Round(CharacterPanelLayout.FrameTop * panelScale)),
			View = this.SideView,
			TextFont = this._stats.TextFont,
			TitleFont = this._stats.TitleFont,
			NumberFont = this._stats.NumberFont,
		};
		page.Controls.Add(this._side);

		var editorFont = this.OwnedFont(new Font("Arial", 12F * panelScale, FontStyle.Bold, GraphicsUnit.Pixel));
		var editors = this.LevelingEditors;
		for (int row = 0; row < editors.Length; row++)
		{
			var editor = editors[row];
			this.PlaceSideEditor(editor, this._side.EditorRow(row), editorFont);

			// A disabled box ignores its colours: it is hidden and the panel writes its value, as the game writes numbers.
			editor.EnabledChanged += (_, _) => this.ShowLevelingEditor(editor);
			editor.TextChanged += (_, _) => this._side.Invalidate();
			this.ShowLevelingEditor(editor);
		}

		this._statsDifficulty = new ComboBox { Name = "StatsDifficultyComboBox", DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat };
		for (int difficulty = 0; difficulty < 3; difficulty++)
			this._statsDifficulty.Items.Add(this.TranslationService.TranslateDifficulty(difficulty) ?? difficulty.ToString(CultureInfo.CurrentCulture));
		this._statsDifficulty.SelectedIndex = CharacterPanelLayout.ShownDifficulty(this.Info.DifficultyUnlocked);
		this.PlaceSideEditor(this._statsDifficulty, this._side.StatisticsRow(0), editorFont);
		this._statsDifficulty.SelectedIndexChanged += (_, _) => this.InvalidateStats();

		this._includeBuffs = new CheckBox { Name = "StatsIncludeBuffsCheckBox" };
		this.PlaceSideEditor(this._includeBuffs, this._side.StatisticsRow(1), editorFont);
		this._includeBuffs.CheckedChanged += (_, _) => this.InvalidateStats();

		this._statsToolTip.SetToolTip(this._statsDifficulty, Resources.CEStatisticsDifficultyToolTip);
		this._statsToolTip.SetToolTip(this._includeBuffs, Resources.CEIncludeBuffsToolTip);

		// The reset buttons under the side panel.
		var buttonX = this._side.Left;
		this.ResetAttributesScalingButton.Parent = page;
		this.ResetAttributesScalingButton.Location = new Point(buttonX, this._side.Bottom + (int)Math.Round(16 * scale));
		this.ResetMasteriesScalingButton.Parent = page;
		this.ResetMasteriesScalingButton.Location = new Point(this.ResetAttributesScalingButton.Right + (int)Math.Round(16 * scale), this.ResetAttributesScalingButton.Top);

		this._tabs.AddPage(Resources.CECharacter, page);
		page.BackgroundImage = this.RenderPageBackground(page.ClientSize);
		this.AddMedallion(page, scale);

		this._stats.AttributeClicked += (_, click) => this.CharacterAttribute_Clicked(click.Attribute, click.Step, click.ToEnd);
		this._stats.UndoClicked += (_, _) => this.CharacterAttributes_Undo();
		foreach (var box in this.AttributeBoxes.Append(this.attributeNumericUpDown))
			box.ValueChanged += (_, _) => this.InvalidateStats();

		this.FormClosed += (_, _) =>
		{
			foreach (var font in this._ownedFonts)
				font.Dispose();
			this._statsToolTip.Dispose();
			this._headerToolTip.Dispose();
		};
	}

	/// <summary>Puts an editor on the side panel, at its row, in the panel's colours.</summary>
	private void PlaceSideEditor(Control editor, Rectangle bounds, Font font)
	{
		editor.Parent = this._side;
		editor.Bounds = bounds;
		editor.Font = font;
		editor.BackColor = Color.FromArgb(24, 24, 24);
		editor.ForeColor = Color.White;
		this._side.Editors.Add(editor);
		if (editor is CheckBox check)
		{
			check.Text = string.Empty;
			check.BackColor = Color.Black;
			check.AutoSize = true;
		}
		else if (editor is ComboBox combo)
		{
			// A drop-down list ignores its colours under the visual styles (white box): it draws its items itself.
			combo.FlatStyle = FlatStyle.Flat;
			combo.DrawMode = DrawMode.OwnerDrawFixed;
			combo.DrawItem += DrawSideComboItem;
		}
	}

	/// <summary>Draws an entry of a side panel drop-down, and its box, in the panel's colours.</summary>
	private static void DrawSideComboItem(object? sender, DrawItemEventArgs e)
	{
		if (sender is not ComboBox combo)
			return;

		// The list highlights the entry under the pointer; the box itself keeps the panel's background.
		var highlighted = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
		using (var back = new SolidBrush(highlighted ? Color.FromArgb(70, 70, 70) : combo.BackColor))
			e.Graphics.FillRectangle(back, e.Bounds);

		if (e.Index >= 0 && e.Index < combo.Items.Count)
		{
			TextRenderer.DrawText(e.Graphics, combo.GetItemText(combo.Items[e.Index]), e.Font ?? combo.Font, e.Bounds,
				combo.Enabled ? combo.ForeColor : Color.Gray,
				TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis);
		}
	}

	/// <summary>
	/// TQVault's medallion picture (<see cref="Images.tqmedallion"/>) in the free column of the Character page, right of
	/// the side panel, at most twice its size so that it stays sharp.
	/// </summary>
	private void AddMedallion(Panel page, float scale)
	{
		var medallion = Images.tqmedallion;
		var area = Rectangle.FromLTRB(
			this._side.Right + (int)Math.Round(24 * scale),
			this._side.Top,
			page.Width - (int)Math.Round(24 * scale),
			page.Height - (int)Math.Round(24 * scale));
		if (area.Width < medallion.Width / 2 || area.Height < medallion.Height / 2)
		{
			// No room left right of the side panel.
			medallion.Dispose();
			return;
		}

		var zoom = Math.Min(2F * scale, Math.Min(area.Width / (float)medallion.Width, area.Height / (float)medallion.Height));
		var size = new Size((int)Math.Round(medallion.Width * zoom), (int)Math.Round(medallion.Height * zoom));
		var picture = new PictureBox
		{
			Name = "CharacterMedallion",
			Image = medallion,
			SizeMode = PictureBoxSizeMode.Zoom,
			BackColor = Color.Transparent,
			Size = size,
			// Centred in the column right of the side panel.
			Location = new Point(area.X + (area.Width - size.Width) / 2, area.Y + (area.Height - size.Height) / 2),
		};
		page.Controls.Add(picture);
		this.FormClosed += (_, _) => medallion.Dispose();
	}

	/// <summary>
	/// The background of the Character page, made once at its size: the dialog's marble, with the thin edge of the
	/// mastery panes (a gold and a black line with a shadow inside) around it. A picture at the page's size is copied as
	/// is when the page repaints, where stretching both at every paint made switching to the page flicker.
	/// </summary>
	private Bitmap RenderPageBackground(Size page)
	{
		var background = new Bitmap(Math.Max(1, page.Width), Math.Max(1, page.Height));
		using (var g = Graphics.FromImage(background))
		using (var edge = Images.CharacterEditorPaneEdge)
		{
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
			if (this.BackgroundImage is { } marble)
				g.DrawImage(marble, new Rectangle(Point.Empty, page));
			g.DrawImage(edge, new Rectangle(Point.Empty, page));
		}

		this.FormClosed += (_, _) => background.Dispose();
		return background;
	}

	/// <summary>
	/// The dialog's background: the plain marble of TQVault's own background picture
	/// (<see cref="Images.CharacterEditorMarble"/>), stretched to the dialog, around the game frame and under OK and Cancel.
	/// </summary>
	private void UseMarbleBackground()
	{
		var marble = Images.CharacterEditorMarble;
		this.BackgroundImage = marble;
		this.BackgroundImageLayout = ImageLayout.Stretch;
		this.FormClosed += (_, _) =>
		{
			marble.Dispose();
		};
	}

	/// <summary>A font the dialog made itself, disposed with it (never one of the font service's, which are shared).</summary>
	private Font OwnedFont(Font font)
	{
		this._ownedFonts.Add(font);
		return font;
	}

	/// <summary>Shows a leveling editor only while it can be changed; the side panel writes the value of the others.</summary>
	private void ShowLevelingEditor(Control editor)
	{
		editor.Visible = editor.Enabled;
		this._side.Invalidate();
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
	/// <summary>
	/// What the statistics panel shows for the current state of the editor. The panel asks for it several times per
	/// mouse move and once per paint, and building it reads every control and computes every statistic: one build
	/// serves all the calls of the same window message. The view is dropped by a posted message, which Windows
	/// delivers before the next mouse or keyboard input, so no change of the user can meet an old view.
	/// </summary>
	private CharacterPanelView CharacterView()
	{
		if (this._characterView is { } view)
			return view;

		view = this.ViewBuilder.Build(this.PanelInput());
		if (this.IsHandleCreated && !this.IsDisposed)
		{
			this._characterView = view;
			this.BeginInvoke(() => this._characterView = null);
		}

		return view;
	}

	private CharacterPanelView? _characterView;

	/// <summary>The state of the editor changed: the statistics panel is built and drawn again.</summary>
	private void InvalidateStats()
	{
		this._characterView = null;
		this._stats.Invalidate();
	}

	/// <summary>What the side panel shows: the leveling rows, the masteries and the statistics options.</summary>
	private CharacterSideView SideView()
	{
		var info = this.Info;
		var culture = CultureInfo.CurrentCulture;
		var level = Convert.ToInt32(this.levelNumericUpDown.Value);
		var xp = long.TryParse(this.xpTextBox.Text, NumberStyles.Integer, culture, out var xpValue) ? xpValue : 0;
		var skills = this._skills;

		return new CharacterSideView
		{
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
				.Select(s => (this.TranslationService.TranslateOr(s.Mastery.NameTag, s.Mastery.Mastery.ToString()),
					string.Format(culture, Resources.CEMasterySummary,
						string.Format(culture, s.SkillCount == 1 ? Resources.CEMasterySkill : Resources.CEMasterySkills, s.SkillCount),
						string.Format(culture, s.PointsSpent == 1 ? Resources.CEMasteryPoint : Resources.CEMasteryPoints, s.PointsSpent))))],
			StatisticsTitle = Resources.CEStatistics,
			StatisticsLabels = [Resources.CEDifficulty, Resources.CEIncludeBuffs],
		};
	}

	/// <summary>
	/// The Undo button: every attribute goes back to its value at the start. The lowered ones first, which gives their
	/// points back for the raised ones.
	/// </summary>
	private void CharacterAttributes_Undo()
	{
		var boxes = this.AttributeBoxes;
		foreach (var i in PlayerLevel.AttributeUndoOrder([.. boxes.Select(b => Convert.ToInt32(b.Value))], this._startAttributes))
			boxes[i].Value = Math.Clamp(this._startAttributes[i], Convert.ToInt32(boxes[i].Minimum), Convert.ToInt32(boxes[i].Maximum));

		this.InvalidateStats();
	}

	/// <summary>
	/// A "+" button: left click spends an attribute point on the attribute, right click takes one back; with Shift all
	/// the points left, or back to the value at the start. The attribute box counts the points, as before.
	/// </summary>
	private void CharacterAttribute_Clicked(int attribute, int step, bool toEnd)
	{
		var box = this.AttributeBoxes[attribute];
		var current = Convert.ToInt32(box.Value);
		var target = PlayerLevel.AttributeClickTarget(current, this._startAttributes[attribute],
			PlayerLevel.IncrementPerPoint(attribute is CharacterPanelInput.Health or CharacterPanelInput.Energy),
			Convert.ToInt32(this.attributeNumericUpDown.Value), step, toEnd, Convert.ToInt32(box.Minimum), Convert.ToInt32(box.Maximum));
		if (target != current)
			box.Value = target;

		this.InvalidateStats();
	}
}
