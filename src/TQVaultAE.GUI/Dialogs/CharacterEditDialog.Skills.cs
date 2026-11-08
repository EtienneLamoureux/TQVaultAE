using System.Drawing;
using System.Windows.Forms;
using TQVaultAE.Application;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation;

namespace TQVaultAE.GUI.Dialogs;

/// <summary>
/// The skills tab of the character edit dialog: one page per mastery slot, with the mastery and the skills of its pane.
/// The rules are in <see cref="CharacterSkillEditor"/>; this part only shows its state and passes the user's changes.
/// </summary>
internal sealed partial class CharacterEditDialog
{
	private const int ColumnIcon = 0;
	private const int ColumnName = 1;
	private const int ColumnRequires = 2;
	private const int ColumnMinus = 3;
	private const int ColumnLevel = 4;
	private const int ColumnPlus = 5;
	private const int ColumnMax = 6;

	private static readonly Color LockedColor = Color.Gray;

	private CharacterSkillEditor? _skills;

	private readonly List<SkillTab> _skillTabs = [];

	/// <summary>Icons at the size of the grid, made once per resource.</summary>
	private readonly Dictionary<string, Bitmap> _skillIcons = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>True while the dialog writes the points left of the editor into the skill points box.</summary>
	private bool _syncingSkillPoints;

	/// <summary>The controls of the page of a mastery slot.</summary>
	private sealed class SkillTab
	{
		public required int Slot { get; init; }
		public required TabPage Page { get; init; }
		public required PictureBox Icon { get; init; }
		public required ComboBox Mastery { get; init; }
		public required NumericUpDown Level { get; init; }
		public required Label Points { get; init; }
		public required DataGridView Grid { get; init; }

		/// <summary>The mastery whose skills are in the grid.</summary>
		public MasteryTree? Tree { get; set; }

		/// <summary>True while the dialog fills the controls, so their events are not user changes.</summary>
		public bool Updating { get; set; }
	}

	/// <summary>An entry of the mastery list.</summary>
	private sealed record MasteryChoice(Masteries? Mastery, string Text)
	{
		public override string ToString() => this.Text;
	}

	private int SkillIconSize => (int)Math.Round(32 * this.UIService.Scale);

	/// <summary>Builds a page per mastery slot of the character.</summary>
	private void InitSkills()
	{
		this.SkillsGroupBox.Text = Resources.CESkills;
		this._skills = this.Services.CharacterEditService.CreateSkillEditor(this.Info);

		var choices = new List<MasteryChoice> { new(null, Resources.CEMasteryNone) };
		foreach (var tree in this._skills.KnownMasteries)
			choices.Add(new MasteryChoice(tree.Mastery, this.TranslateTag(tree.NameTag, tree.Mastery.ToString())));

		for (int slot = 0; slot < this._skills.Slots.Count; slot++)
			this._skillTabs.Add(this.CreateSkillTab(slot, choices));

		this.skillPointsNumericUpDown.ValueChanged += this.SkillPointsNumericUpDown_ValueChanged;
		this.FormClosed += (_, _) =>
		{
			foreach (var icon in this._skillIcons.Values)
				icon.Dispose();
		};

		this.RefreshSkills();
	}

	private string TranslateTag(string? tag, string fallback = "")
		=> string.IsNullOrEmpty(tag)
			? fallback
			: this.TranslationService.TranslateXTag(tag, removeAllTQTags: true) is { Length: > 0 } text ? text : fallback;

	private SkillTab CreateSkillTab(int slot, List<MasteryChoice> choices)
	{
		var page = new TabPage { BackColor = this.BackColor, ForeColor = Color.White, Padding = new Padding(4) };

		var icon = new PictureBox { Size = new Size(this.SkillIconSize, this.SkillIconSize), SizeMode = PictureBoxSizeMode.Zoom, Margin = new Padding(2) };
		var mastery = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = (int)Math.Round(170 * this.UIService.Scale), Name = $"SkillsMastery{slot + 1}ComboBox" };
		mastery.Items.AddRange([.. choices]);
		var levelLabel = new Label { Text = Resources.CEMasteryLevel, AutoSize = true, Margin = new Padding(6, 8, 2, 0) };
		var level = new NumericUpDown { Minimum = 0, Maximum = 40, Width = (int)Math.Round(55 * this.UIService.Scale), Name = $"SkillsMastery{slot + 1}LevelUpDown" };
		var points = new Label { AutoSize = true, Margin = new Padding(6, 8, 2, 0), Name = $"SkillsMastery{slot + 1}PointsLabel" };

		var header = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true };
		header.Controls.AddRange([icon, mastery, levelLabel, level, points]);

		var grid = this.CreateSkillGrid(slot);

		page.Controls.Add(grid);
		page.Controls.Add(header);
		this.SkillsTabControl.TabPages.Add(page);

		var tab = new SkillTab { Slot = slot, Page = page, Icon = icon, Mastery = mastery, Level = level, Points = points, Grid = grid };

		mastery.SelectedIndexChanged += (_, _) => this.SkillMastery_Changed(tab);
		level.ValueChanged += (_, _) => this.SkillMasteryLevel_Changed(tab);
		grid.CellContentClick += (_, e) => this.SkillGrid_CellContentClick(tab, e);
		grid.CellEndEdit += (_, e) => this.SkillGrid_CellEndEdit(tab, e);

		return tab;
	}

	private DataGridView CreateSkillGrid(int slot)
	{
		var back = this.BackColor;
		var grid = new DataGridView
		{
			Name = $"SkillsMastery{slot + 1}Grid",
			Dock = DockStyle.Fill,
			AllowUserToAddRows = false,
			AllowUserToDeleteRows = false,
			AllowUserToResizeRows = false,
			AllowUserToOrderColumns = false,
			RowHeadersVisible = false,
			MultiSelect = false,
			SelectionMode = DataGridViewSelectionMode.CellSelect,
			BackgroundColor = back,
			BorderStyle = BorderStyle.None,
			GridColor = Color.FromArgb(90, 70, 40),
			EnableHeadersVisualStyles = false,
			ShowCellToolTips = true,
			ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
			EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2,
		};
		grid.DefaultCellStyle.BackColor = back;
		grid.DefaultCellStyle.ForeColor = Color.White;
		grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(90, 70, 40);
		grid.DefaultCellStyle.SelectionForeColor = Color.White;
		grid.ColumnHeadersDefaultCellStyle.BackColor = back;
		grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gold;
		grid.RowTemplate.Height = this.SkillIconSize + 4;

		var narrow = (int)Math.Round(28 * this.UIService.Scale);
		var icon = new DataGridViewImageColumn { Width = this.SkillIconSize + 6, ImageLayout = DataGridViewImageCellLayout.Zoom, ReadOnly = true };
		icon.DefaultCellStyle.NullValue = null;
		grid.Columns.AddRange(
			icon,
			new DataGridViewTextBoxColumn { HeaderText = Resources.CESkillName, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true },
			new DataGridViewTextBoxColumn { HeaderText = Resources.CESkillRequires, AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, ReadOnly = true },
			new DataGridViewButtonColumn { Text = "-", UseColumnTextForButtonValue = true, Width = narrow, FlatStyle = FlatStyle.Flat },
			new DataGridViewTextBoxColumn { HeaderText = Resources.CESkillLevel, Width = (int)Math.Round(50 * this.UIService.Scale) },
			new DataGridViewButtonColumn { Text = "+", UseColumnTextForButtonValue = true, Width = narrow, FlatStyle = FlatStyle.Flat },
			new DataGridViewTextBoxColumn { HeaderText = Resources.CESkillMax, AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells, ReadOnly = true });

		foreach (DataGridViewColumn column in grid.Columns)
			column.SortMode = DataGridViewColumnSortMode.NotSortable;

		return grid;
	}

	private Bitmap? SkillIcon(string resource)
	{
		if (string.IsNullOrEmpty(resource))
			return null;

		if (!this._skillIcons.TryGetValue(resource, out var icon))
		{
			var source = this.UIService.LoadBitmap(resource.ToRecordId());
			icon = new Bitmap(source, new Size(this.SkillIconSize, this.SkillIconSize));
			this._skillIcons[resource] = icon;
		}

		return icon;
	}

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
				tab.Page.Text = tree is null ? string.Format(Resources.CEMasterySlot, tab.Slot + 1) : this.TranslateTag(tree.NameTag, tree.Mastery.ToString());
				tab.Icon.Image = tree is null ? null : this.SkillIcon(tree.Icon);
				tab.Mastery.SelectedItem = tab.Mastery.Items.Cast<MasteryChoice>().First(c => c.Mastery == tree?.Mastery);

				var masteryLevel = skills.GetMasteryLevel(tab.Slot);
				tab.Level.Enabled = tree is not null;
				tab.Level.Minimum = tree is null ? 0 : 1;
				tab.Level.Maximum = Math.Max(tree?.MaxLevel ?? 0, masteryLevel);
				tab.Level.Value = masteryLevel;
				tab.Points.Text = string.Format(Resources.CEMasteryPointsSpent, skills.GetPointsSpent(tab.Slot));

				if (!ReferenceEquals(tab.Tree, tree))
					this.FillSkillGrid(tab, tree);

				this.RefreshSkillRows(tab);
			}
			finally
			{
				tab.Updating = false;
			}
		}

		this.RefreshMasterySummary();

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

	/// <summary>Shows each mastery with its skills that have points and the points spent, in the Masteries box.</summary>
	private void RefreshMasterySummary()
	{
		var skills = this._skills!;
		var names = new[] { this.Mastery1NameScalingLabel, this.Mastery2NameScalingLabel };
		var values = new[] { this.Mastery1ValueScalingLabel, this.Mastery2ValueScalingLabel };
		for (int i = 0; i < names.Length; i++)
		{
			var tree = i < skills.Slots.Count ? skills.Slots[i] : null;
			names[i].Visible = values[i].Visible = tree is not null;
			if (tree is null)
				continue;

			var learned = tree.Skills.Count(s => skills.GetLevel(s.Id) > 0) + 1;
			names[i].Text = string.Format(names[i].Tag?.ToString() ?? "{0}", this.TranslateTag(tree.NameTag, tree.Mastery.ToString()));
			values[i].Text = string.Format(values[i].Tag?.ToString() ?? "{0} ({1})", learned, skills.GetPointsSpent(i));
		}
	}

	private void FillSkillGrid(SkillTab tab, MasteryTree? tree)
	{
		tab.Tree = tree;
		tab.Grid.Rows.Clear();
		if (tree is null)
			return;

		foreach (var skill in tree.Skills)
		{
			var row = tab.Grid.Rows[tab.Grid.Rows.Add()];
			row.Cells[ColumnIcon].Value = this.SkillIcon(skill.Icon);
			row.Cells[ColumnName].Value = this.TranslateTag(skill.NameTag, skill.Id.PrettyFileName);
			row.Cells[ColumnRequires].Value = skill.RequiredMasteryLevel;
			row.Cells[ColumnMax].Value = skill.MaxLevel;
			row.Tag = skill;
			row.Cells[ColumnIcon].ToolTipText = row.Cells[ColumnName].ToolTipText = this.TranslateTag(skill.DescriptionTag);
		}
	}

	/// <summary>Shows the level of each skill, and greys out the locked ones.</summary>
	private void RefreshSkillRows(SkillTab tab)
	{
		if (this._skills is not { } skills)
			return;

		foreach (DataGridViewRow row in tab.Grid.Rows)
		{
			if (row.Tag is not MasterySkill skill)
				continue;

			if (!row.Cells[ColumnLevel].IsInEditMode)
				row.Cells[ColumnLevel].Value = skills.GetLevel(skill.Id);

			var color = skills.IsUnlocked(tab.Slot, skill) ? Color.White : LockedColor;
			row.DefaultCellStyle.ForeColor = color;
			row.DefaultCellStyle.SelectionForeColor = color;
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

	private void SkillGrid_CellContentClick(SkillTab tab, DataGridViewCellEventArgs e)
	{
		if (this._skills is null || e.RowIndex < 0 || tab.Grid.Rows[e.RowIndex].Tag is not MasterySkill skill)
			return;

		int step = e.ColumnIndex switch
		{
			ColumnMinus => -1,
			ColumnPlus => 1,
			_ => 0,
		};
		if (step == 0)
			return;

		this._skills.SetSkillLevel(tab.Slot, skill.Id, this._skills.GetLevel(skill.Id) + step);
		this.RefreshSkills();
	}

	private void SkillGrid_CellEndEdit(SkillTab tab, DataGridViewCellEventArgs e)
	{
		if (this._skills is null || e.ColumnIndex != ColumnLevel || e.RowIndex < 0 || tab.Grid.Rows[e.RowIndex].Tag is not MasterySkill skill)
			return;

		var text = Convert.ToString(tab.Grid.Rows[e.RowIndex].Cells[ColumnLevel].Value);
		if (int.TryParse(text, out var level))
			this._skills.SetSkillLevel(tab.Slot, skill.Id, level);

		// The grid is still ending the edit: refresh once it is done.
		this.BeginInvoke(this.RefreshSkills);
	}

	private void SkillPointsNumericUpDown_ValueChanged(object? sender, EventArgs e)
	{
		if (this._syncingSkillPoints || this._skills is null)
			return;

		this._skills.AvailablePoints = Convert.ToInt32(this.skillPointsNumericUpDown.Value);
	}
}
