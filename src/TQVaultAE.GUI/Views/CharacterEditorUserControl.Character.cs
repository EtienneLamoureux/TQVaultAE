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

namespace TQVaultAE.GUI.Views;

/// <summary>
/// The statistics column of the character editor: the statistics panel of the game's character window
/// (<see cref="CharacterStatsPanel"/>) with the attribute "+" buttons and the gold bar, and the side panel with the
/// leveling editors, the masteries and the statistics options; right of it the frame of the mastery pages, drawn as the
/// game's skill window (<see cref="GameTabControl"/>), with the reset buttons, OK and Cancel under it.
/// </summary>
internal sealed partial class CharacterEditorUserControl
{
	private GameTabControl _tabs = null!;
	private CharacterStatsPanel _stats = null!;
	private CharacterSidePanel _side = null!;
	private readonly List<Font> _ownedFonts = [];

	/// <summary>The font of the editors on the side panel, in pixels at the editor's DPI.</summary>
	private Font _editorFont = null!;

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

	/// <summary>The leveling editors, in row order under the "Leveling" title of the side panel.</summary>
	private Control[] LevelingEditors => [this.levelNumericUpDown, this.xpTextBox, this.skillPointsNumericUpDown, this.difficultlyComboBox, this.levelingCheckBox];

	/// <summary>The attribute values at the start, for Shift + right click.</summary>
	private int[] _startAttributes = [];

	/// <summary>The size of a game pixel on screen: the frame is drawn at the game's size times the DPI factor.</summary>
	private float GameScale => this.DeviceDpi / 96F;

	/// <summary>
	/// The game frame with OK and Cancel under it, sized to them; laid at the top left of the control, or in its middle
	/// when the host gives it a larger area (<see cref="OnLayout"/>).
	/// </summary>
	private readonly Panel _content = new() { Name = "CharacterEditorContent", BackColor = Color.Transparent };

	/// <summary>The size of the frame with OK and Cancel under it and the margins around: the editor's natural size.</summary>
	internal Size ContentSize => this._content.Size;

	/// <summary>
	/// The statistics column's panels against the game size: at 1.05 times, the column (statistics panel and side panel,
	/// one above the other) and the frame beside it fit in the main window at zoom 1, with a margin.
	/// </summary>
	private const float ColumnScale = 1.05F;

	/// <summary>
	/// Builds the statistics column (the statistics panel of the game's character window, and under it the side panel
	/// with the statistics options, the leveling editors and the masteries) and, right of it, the game frame of the
	/// mastery pages: the statistics stay in sight while points are spent in the masteries.
	/// </summary>
	private void InitStatisticsColumn()
	{
		var scale = this.GameScale;
		var margin = this.LogicalToDeviceUnits(14);
		this.Controls.Add(this._content);

		var panelScale = scale * ColumnScale;
		this._stats = new CharacterStatsPanel(this.UIService)
		{
			KeyBinds = this.Services.KeyBinds,
			Name = "CharacterStatsPanel",
			PanelScale = panelScale,
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

		// The attribute boxes stay the model of the attributes (their events count the points); the panel shows them.
		foreach (var box in this.AttributeBoxes.Append(this.attributeNumericUpDown))
			box.Visible = false;

		// The leveling editors, the masteries and the statistics options, in the frame of the statistics panel: the game
		// window has none, it draws defense and offense where the editor had them.
		this._side = new CharacterSidePanel(this.UIService)
		{
			Name = "CharacterSidePanel",
			PanelScale = panelScale,
			View = this.SideView,
			TextFont = this._stats.TextFont,
			TitleFont = this._stats.TitleFont,
			NumberFont = this._stats.NumberFont,
		};

		// The column at the top left: the statistics panel, its tabs level with the mastery tabs, and under its gold bar
		// the side panel, as wide as the statistics panel's frame, whose statistics options come first.
		this._stats.Location = new Point(margin, margin);
		this._side.Location = new Point(margin, this._stats.Bottom + (int)Math.Round(10 * scale));

		this._content.Controls.Add(this._stats);
		this._content.Controls.Add(this._side);

		// The frame of the mastery pages right of the column, its tabs level with the top of the column.
		this._tabs = new GameTabControl
		{
			Name = "EditorTabs",
			FrameScale = scale,
			Location = new Point(this._stats.Right + (int)Math.Round(16 * scale), margin),
			TabFont = this.FontService.GetFontLight(16F * scale, FontStyle.Regular, GraphicsUnit.Pixel),
		};
		this._content.Controls.Add(this._tabs);

		var editorFont = this._editorFont = this.OwnedFont(new Font("Arial", 12F * panelScale, FontStyle.Bold, GraphicsUnit.Pixel));
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

		this._stats.AttributeClicked += (_, click) => this.CharacterAttribute_Clicked(click.Attribute, click.Step, click.ToEnd);
		this._stats.UndoClicked += (_, _) => this.CharacterAttributes_Undo();
		foreach (var box in this.AttributeBoxes.Append(this.attributeNumericUpDown))
			box.ValueChanged += (_, _) => this.InvalidateStats();

		this.Disposed += (_, _) =>
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

			// At the right end of its row, where the game writes numbers, the label before it taking the rest; on the row's
			// bottom, clear of a drop-down list above it, a little higher than its row.
			check.Location = new Point(bounds.Right - check.Width, bounds.Bottom - check.Height);
		}
		else if (editor is ComboBox combo)
		{
			// A drop-down list ignores its colours under the visual styles (white box): it draws its items itself.
			combo.FlatStyle = FlatStyle.Flat;
			combo.DrawMode = DrawMode.OwnerDrawFixed;
			combo.DrawItem += DrawSideComboItem;
			FitComboHeight(combo, bounds.Height);
		}

		this._sideEditorBounds[editor] = (editor.Bounds, bounds.Height);
		editor.VisibleChanged += (_, _) =>
		{
			if (this._loaded && editor.Visible && this.IsHandleCreated)
				this.BeginInvoke(this.RestoreSideEditors);
		};
	}

	/// <summary>
	/// A drop-down list as high as its row: its height follows its font, and a few pixels more covered the check box of
	/// the next row.
	/// </summary>
	private static void FitComboHeight(ComboBox combo, int height)
	{
		// The box is its item's height plus its border (3 pixels above and under); 2 pixels over the row keep the text whole.
		combo.ItemHeight = Math.Max(1, height - 4);
	}

	/// <summary>Where each editor of the side panel goes, in pixels at the editor's DPI.</summary>
	private readonly Dictionary<Control, (Rectangle Bounds, int RowHeight)> _sideEditorBounds = [];

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

	/// <summary>
	/// Sizes the content around the statistics column and the frame, with the reset buttons, OK and Cancel under the frame
	/// and TQVault's medallion between them, in the room the column leaves under the frame.
	/// </summary>
	private void FitToFrame()
	{
		var margin = this.LogicalToDeviceUnits(14);
		var gap = this.LogicalToDeviceUnits(10);
		var buttonsTop = this._tabs.Bottom + gap;
		var bottom = Math.Max(this._stats.Bottom, this._side.Bottom);
		bottom = Math.Max(bottom, buttonsTop + Math.Max(this.ok.Height, this.ResetAttributesScalingButton.Height));

		// The size first: OK and Cancel are anchored to the bottom right, a later size would move them.
		this._content.Size = new Size(this._tabs.Right + margin, bottom + margin);

		// The reset buttons on the left under the frame, OK and Cancel on the right.
		this.ResetAttributesScalingButton.Parent = this.ResetMasteriesScalingButton.Parent = this._content;
		this.ResetAttributesScalingButton.Location = new Point(this._tabs.Left, buttonsTop);
		this.ResetMasteriesScalingButton.Location = new Point(this.ResetAttributesScalingButton.Right + gap, buttonsTop);
		this.ok.Parent = this.cancel.Parent = this._content;
		this.cancel.Location = new Point(this._tabs.Right - this.cancel.Width, buttonsTop);
		this.ok.Location = new Point(this.cancel.Left - gap - this.ok.Width, buttonsTop);

		this.AddMedallion(Rectangle.FromLTRB(
			this._tabs.Left,
			buttonsTop + this.ok.Height + gap,
			this._tabs.Right,
			bottom));
		this.PerformLayout();
	}

	/// <summary>
	/// TQVault's medallion picture (<see cref="Images.tqmedallion"/>) in the middle of <paramref name="area"/>, at most
	/// twice its size so that it stays sharp; left out when the area is too small for half its size.
	/// </summary>
	private void AddMedallion(Rectangle area)
	{
		var medallion = Images.tqmedallion;
		if (area.Width < medallion.Width / 2 || area.Height < medallion.Height / 2)
		{
			medallion.Dispose();
			return;
		}

		var zoom = Math.Min(2F * this.GameScale, Math.Min(area.Width / (float)medallion.Width, area.Height / (float)medallion.Height));
		var size = new Size((int)Math.Round(medallion.Width * zoom), (int)Math.Round(medallion.Height * zoom));
		this._content.Controls.Add(new PictureBox
		{
			Name = "CharacterMedallion",
			Image = medallion,
			SizeMode = PictureBoxSizeMode.Zoom,
			BackColor = Color.Transparent,
			Size = size,
			Location = new Point(area.X + (area.Width - size.Width) / 2, area.Y + (area.Height - size.Height) / 2),
		});
		this.Disposed += (_, _) => medallion.Dispose();
	}

	/// <summary>
	/// The side panel's editors are put back once shown. The editor is built hidden, and an editor that cannot be changed
	/// stays hidden until it can: their windows are made when they show, and Windows Forms then scales their place and
	/// font by the monitor's DPI over the system's, though both are already in pixels at the editor's DPI (on a monitor
	/// whose DPI is not the system's, their text was too small and the level box went up on the title).
	/// </summary>
	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (this._loaded && this.Visible)
			this.RestoreSideEditors();
	}

	/// <summary>
	/// Moved to a monitor of another DPI while shown, the editor keeps its size (<see cref="ScaleControl"/>), but Windows
	/// Forms scales the place and font of each editor: the side panel's editors are put back once it is done, or their
	/// text no longer fitted their boxes.
	/// </summary>
	protected override void OnDpiChangedAfterParent(EventArgs e)
	{
		base.OnDpiChangedAfterParent(e);
		if (this._loaded && this.IsHandleCreated)
			this.BeginInvoke(this.RestoreSideEditors);
	}

	/// <summary>Puts each editor of the side panel back at its place, in its font.</summary>
	private void RestoreSideEditors()
	{
		if (this.IsDisposed)
			return;

		foreach (var (editor, (bounds, rowHeight)) in this._sideEditorBounds)
		{
			if (!ReferenceEquals(editor.Font, this._editorFont))
			{
				editor.Font = this._editorFont;
				if (editor is ComboBox combo)
					FitComboHeight(combo, rowHeight);
			}

			if (editor.Bounds != bounds)
				editor.Bounds = bounds;
		}
	}

	/// <summary>
	/// The content at the top left of the control; in the middle of a larger area, at its top left when it is smaller,
	/// scrolled (<see cref="ScrollableControl.AutoScroll"/>).
	/// </summary>
	protected override void OnLayout(LayoutEventArgs e)
	{
		base.OnLayout(e);

		var location = new Point(
			Math.Max(0, (this.ClientSize.Width - this._content.Width) / 2) + this.AutoScrollPosition.X,
			Math.Max(0, (this.ClientSize.Height - this._content.Height) / 2) + this.AutoScrollPosition.Y);
		if (this._content.Location != location)
			this._content.Location = location;
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

		// The masteries the side panel sums up change with the points spent.
		this._side.Invalidate();
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
				.Select(s => PlayerInfoText.MasterySummaryLine(s, this.TranslationService))],
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
