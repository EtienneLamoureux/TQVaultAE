using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Config;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Views;

using System;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using System.Linq;
using Microsoft.Extensions.Logging;

/// <summary>
/// The character editor: the character's statistics, leveling and masteries drawn as the game's character and skill
/// windows, with OK and Cancel. Its host shows a character with <see cref="ShowCharacter"/> once the control is in a
/// window (it is built for that character only), and leaves it on <see cref="Applied"/> (OK, the edits applied to the
/// character in memory) or <see cref="Cancelled"/> (Cancel, the edits dropped).
/// </summary>
internal sealed partial class CharacterEditorUserControl : UserControl, IMainWindowView
{
	private struct DifficultData
	{
		public string Text;
		public int Id;

		public override string ToString() => Text;
	}

	private readonly ILogger Log;
	private readonly GuiServices Services;
	private readonly UserSettings USettings;
	private readonly IUIService UIService;
	private readonly IFontService FontService;
	private readonly ITranslationService TranslationService;

	/// <summary>Character edited, given by <see cref="ShowCharacter"/>.</summary>
	internal PlayerCollection PlayerCollection { get; private set; } = null!;

	/// <summary>The editor is only shown for a loaded character, which has its player info.</summary>
	private PlayerInfo Info => this.PlayerCollection.PlayerInfo
		?? throw new InvalidOperationException("The character has no player info.");

	/// <summary>OK was pressed and the edits applied to the character, in memory (the Save button writes them).</summary>
	public event EventHandler? Applied;

	/// <summary>Cancel was pressed: the edits are dropped.</summary>
	public event EventHandler? Cancelled;

	/// <summary>The Cancel button, which a host may press for Escape.</summary>
	public IButtonControl CancelButton => this.cancel;

	/// <summary>
	/// Initializes a new instance of the CharacterEditorUserControl class.
	/// </summary>
	public CharacterEditorUserControl(GuiServices services)
	{
		this.Services = services;
		this.USettings = services.Settings;
		this.UIService = services.UIService;
		this.FontService = services.FontService;
		this.TranslationService = services.TranslationService;
		this.Log = services.Logger<CharacterEditorUserControl>();

		this.InitializeComponent();

		#region Apply custom font

		this.ResetMasteriesScalingButton.Font = 
			this.ResetAttributesScalingButton.Font =
				this.ok.Font = 
					this.cancel.Font = FontService.GetFontLight(12F);

		this.Font = FontService.GetFontLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));


		#endregion

		this.cancel.Text = Resources.GlobalCancel;
		this.ok.Text = Resources.GlobalOK;
		this.ResetMasteriesScalingButton.Text = Resources.ResetMasteriesButton;
		this.ResetAttributesScalingButton.Text = Resources.ResetAttributesButton;
		this.Text = Resources.CharacterViewTitle;

		// Around the game frame, the main window's own background shows through (transparent, set by the designer), as
		// around the settings and the main content.
	}

	/// <inheritdoc/>
	public bool HasChanges => this._loaded
		&& (this.CurrentState() != this._startState || this._skills?.Changed == true || this._skills?.MasteriesChanged == true);

	/// <inheritdoc/>
	public string DiscardChangesQuestion => Resources.CharacterViewDiscardChanges;

	/// <summary>The values of the editors, to tell whether the user changed any.</summary>
	private readonly record struct EditorState(int Level, string Xp, int Difficulty, int SkillPoints, int AttributePoints, int Health, int Mana, int Strength, int Intelligence, int Dexterity);

	/// <summary>The values of the editors once the character is shown.</summary>
	private EditorState _startState;

	private EditorState CurrentState() => new(
		Convert.ToInt32(this.levelNumericUpDown.Value),
		this.xpTextBox.Text,
		// Ticking Enable leveling selects the difficulty unlocked: no change until another one is chosen.
		this.difficultlyComboBox.Enabled ? this.difficultlyComboBox.SelectedIndex : this.Info.DifficultyUnlocked,
		Convert.ToInt32(this.skillPointsNumericUpDown.Value),
		Convert.ToInt32(this.attributeNumericUpDown.Value),
		Convert.ToInt32(this.healthUpDown.Value),
		Convert.ToInt32(this.manacUpDown.Value),
		Convert.ToInt32(this.strengthUpDown.Value),
		Convert.ToInt32(this.intelligenceUpDown.Value),
		Convert.ToInt32(this.dexterityUpDown.Value));

	/// <summary>
	/// Escape presses Cancel, as in a dialog: once the focused control left it (an open drop-down list closes first).
	/// </summary>
	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (keyData == Keys.Escape)
		{
			this.cancel.PerformClick();
			return true;
		}

		return base.ProcessDialogKey(keyData);
	}

	/// <summary>
	/// Scrolled (an area smaller than the editor), the editor stays where the user put it when a control takes the
	/// focus: it jumped to the first control at each opening, and to any control clicked.
	/// </summary>
	protected override Point ScrollToControl(Control activeControl) => this.DisplayRectangle.Location;

	/// <summary>
	/// Once built, the editor keeps its size: its game frame is drawn at the DPI it was built for, which scaling its
	/// controls would not change (a zoom of the main window, or a move to a monitor of another DPI, while it is shown).
	/// It is built for one opening, so the next one takes the new scale.
	/// </summary>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		if (!this._loaded)
			base.ScaleControl(factor, specified);
	}

	/// <inheritdoc cref="ScaleControl"/>
	protected override bool ScaleChildren => !this._loaded;


	/// <summary>
	/// Cancel button handler
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CancelButton_Click(object? sender, EventArgs e) => this.Cancelled?.Invoke(this, EventArgs.Empty);

	/// <summary>
	/// OK button handler
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void OKButton_Click(object? sender, EventArgs e)
	{
		UpdatePlayerInfo();
	}

	private void UpdatePlayerInfo()
	{
		if (!USettings.AllowCharacterEdit) return;
		if (PlayerCollection.PlayerInfo is null) return;
		try
		{
			var request = new CharacterEditRequest(
				Level: Convert.ToInt32(levelNumericUpDown.Value)
				, XpText: xpTextBox.Text
				, DifficultyUnlocked: difficultlyComboBox.Enabled ? difficultlyComboBox.SelectedIndex : null
				, SkillPoints: Convert.ToInt32(skillPointsNumericUpDown.Value)
				, AttributePoints: Convert.ToInt32(attributeNumericUpDown.Value)
				, Strength: Convert.ToInt32(strengthUpDown.Value)
				, Dexterity: Convert.ToInt32(dexterityUpDown.Value)
				, Intelligence: Convert.ToInt32(intelligenceUpDown.Value)
				, Health: Convert.ToInt32(healthUpDown.Value)
				, Mana: Convert.ToInt32(manacUpDown.Value)
				, SkillRecords: this._skills?.Changed == true ? this._skills.BuildSkillRecords() : null
				, MasteriesChanged: this._skills?.MasteriesChanged == true);

			this.Services.CharacterEditService.Apply(PlayerCollection, request);

			this.Applied?.Invoke(this, EventArgs.Empty);
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to apply the character changes to {Player}", this.PlayerCollection.PlayerName);
			this.Services.MessageBoxes.Inform(ex.Message, string.Empty);
		}
	}

	private bool _loaded = false;

	/// <summary>
	/// The last value handled of each attribute and of the level, to compute the points of a change: the value
	/// changed event gives the new value only.
	/// </summary>
	private readonly Dictionary<NumericUpDown, int> _previousValues = [];

	private struct TagData
	{
		public int IncrementValue;
	}

	/// <summary>
	/// Builds the editor for <paramref name="player"/>, a loaded character. Called once, when the control is in its
	/// window: the game frame is drawn at that window's DPI.
	/// </summary>
	internal void ShowCharacter(PlayerCollection player)
	{
		if (this._loaded)
			throw new InvalidOperationException("The character editor shows one character only.");

		this.PlayerCollection = player;

		// Its handle gives the control the DPI of its window (a control without one has the primary monitor's).
		_ = this.Handle;

		this.InitStatisticsColumn();

		// A read-only character can be looked at, not changed: OK would apply nothing (CharacterEditService.Apply).
		this.ok.Enabled = !this.PlayerCollection.IsReadOnly;

		if (Info.BaseStrength > Convert.ToInt32(strengthUpDown.Maximum))
			strengthUpDown.Maximum = Info.BaseStrength;

		strengthUpDown.Value = Info.BaseStrength;
		var attrTag = new TagData { IncrementValue = PlayerLevel.AtrributeIncrementPerPoint };
		strengthUpDown.Tag = attrTag;

		if (Info.BaseDexterity > Convert.ToInt32(dexterityUpDown.Maximum))
			dexterityUpDown.Maximum = Info.BaseDexterity;

		dexterityUpDown.Value = Info.BaseDexterity;
		dexterityUpDown.Tag = attrTag;

		if (Info.BaseIntelligence > Convert.ToInt32(intelligenceUpDown.Maximum))
			intelligenceUpDown.Maximum = Info.BaseIntelligence;

		intelligenceUpDown.Value = Info.BaseIntelligence;
		intelligenceUpDown.Tag = attrTag;

		if (Info.BaseHealth > Convert.ToInt32(healthUpDown.Maximum))
			healthUpDown.Maximum = Info.BaseHealth;

		healthUpDown.Value = Info.BaseHealth;
		var attrHMTag = new TagData { IncrementValue = PlayerLevel.HealthAndManaIncrementPerPoint };
		healthUpDown.Tag = attrHMTag;

		if (Info.BaseMana > Convert.ToInt32(manacUpDown.Maximum))
			manacUpDown.Maximum = Info.BaseMana;

		manacUpDown.Value = Info.BaseMana;
		manacUpDown.Tag = attrHMTag;

		if (Info.CurrentLevel > Convert.ToInt32(levelNumericUpDown.Maximum))
			levelNumericUpDown.Maximum = Info.CurrentLevel;

		levelNumericUpDown.Value = Info.CurrentLevel;

		xpTextBox.Text = string.Format("{0}", Info.CurrentXP);

		if (Info.AttributesPoints > Convert.ToInt32(attributeNumericUpDown.Maximum))
			attributeNumericUpDown.Maximum = Info.AttributesPoints;

		attributeNumericUpDown.Value = Info.AttributesPoints;

		if (Info.SkillPoints > Convert.ToInt32(skillPointsNumericUpDown.Maximum))
			skillPointsNumericUpDown.Maximum = Info.SkillPoints;

		skillPointsNumericUpDown.Value = Info.SkillPoints;

		this.InitSkills();

		difficultlyComboBox.Items.Clear();
		difficultlyComboBox.Items.Add(new DifficultData { Text = TranslationService.TranslateDifficulty(0), Id = 0 });
		difficultlyComboBox.Items.Add(new DifficultData { Text = TranslationService.TranslateDifficulty(1), Id = 1 });
		difficultlyComboBox.Items.Add(new DifficultData { Text = TranslationService.TranslateDifficulty(2), Id = 2 });

		levelingCheckBox.Checked = false;

		if (Info.HasBeenInGame == 0)
		{
			levelingCheckBox.Enabled = false;
			levelingCheckBox.Visible = false;
		}

		foreach (var upDown in new[] { strengthUpDown, dexterityUpDown, intelligenceUpDown, healthUpDown, manacUpDown, levelNumericUpDown })
			this._previousValues[upDown] = Convert.ToInt32(upDown.Value);

		this._startAttributes = [.. this.AttributeBoxes.Select(b => Convert.ToInt32(b.Value))];
		this.FitToFrame();
		_loaded = true;
		this._startState = this.CurrentState();
	}

	private void LevelNumericUpDown_ValueChanged(object? sender, EventArgs e)
	{
		if (!_loaded) return;

		try
		{
			var upDown = (NumericUpDown)sender!;
			var prevLvl = this._previousValues[upDown];
			var level = Convert.ToInt32(upDown.Value);
			this._previousValues[upDown] = level;

			xpTextBox.Text = PlayerLevel.LevelStartXp(level).ToString();

			var (attr, skills) = PlayerLevel.GetLevelChangePoints(prevLvl, level);

			var newAttr = attributeNumericUpDown.Value + attr;

			if (newAttr <= Convert.ToInt32(attributeNumericUpDown.Maximum)
			    && newAttr >= Convert.ToInt32(attributeNumericUpDown.Minimum)
			   )
			{
				attributeNumericUpDown.Value = newAttr;
			}

			var newSkills = skillPointsNumericUpDown.Value + skills;

			if (newSkills <= Convert.ToInt32(skillPointsNumericUpDown.Maximum)
			    && newSkills >= Convert.ToInt32(skillPointsNumericUpDown.Minimum))
			{
				skillPointsNumericUpDown.Value = newSkills;
			}
		}
		catch (Exception ex)
		{
			this.Log.LogError(ex, "Failed to update the attribute and skill points for the level change of {Player}", this.PlayerCollection.PlayerName);
		}
	}

	private void StatsUpDown_ValueChanged(object? sender, EventArgs e)
	{
		if (!_loaded) return;
		if (sender is null) return;
		var upDwnCtrl = (NumericUpDown)sender!;
		if (upDwnCtrl.Tag is null) return;

		var prevValue = this._previousValues[upDwnCtrl];
		var value = Convert.ToInt32(upDwnCtrl.Value);

		var tag = (TagData)upDwnCtrl.Tag;
		if (!PlayerLevel.TryChangeAttribute(prevValue, value, tag.IncrementValue, Convert.ToInt32(attributeNumericUpDown.Value), out var newAttr))
		{
			// Not enough points: undo, which raises this handler again with no change.
			upDwnCtrl.Value = prevValue;
			return;
		}

		this._previousValues[upDwnCtrl] = value;

		if (newAttr <= Convert.ToInt32(attributeNumericUpDown.Maximum)
		    && newAttr >= Convert.ToInt32(attributeNumericUpDown.Minimum))
		{
			attributeNumericUpDown.Value = newAttr;
		}

	}

	private void LevelingCheckBox_CheckedChanged(object? sender, EventArgs e)
	{
		if (sender is null) return;

		var chkbx = (CheckBox)sender!;
		if (chkbx.Checked)
		{
			this.difficultlyComboBox.Enabled = true;
			this.levelNumericUpDown.Enabled = true;
		}
		else
		{
			this.difficultlyComboBox.Enabled = false;
			this.levelNumericUpDown.Enabled = false;
		}
	}

	/// <summary>
	/// Reset Masteries: every mastery slot is emptied in the editor and its skill points given back. Like any other edit
	/// it is applied by OK, and Cancel drops it.
	/// </summary>
	private void ResetMasteriesScalingButton_Click(object? sender, EventArgs e)
	{
		if (this._skills is null)
			return;

		this._skills.ResetMasteries();
		this.RefreshSkills();
	}

	/// <summary>
	/// Reset Attributes: every attribute goes back to its minimum in the editor, its points back to the pool (the
	/// attribute boxes count them, as for a change by hand). Applied by OK, dropped by Cancel.
	/// </summary>
	private void ResetAttributesScalingButton_Click(object? sender, EventArgs e)
	{
		var pool = Convert.ToInt32(this.attributeNumericUpDown.Value) + PlayerLevel.GetInvestedAttributePoints(
			Convert.ToInt32(this.strengthUpDown.Value), Convert.ToInt32(this.dexterityUpDown.Value), Convert.ToInt32(this.intelligenceUpDown.Value),
			Convert.ToInt32(this.healthUpDown.Value), Convert.ToInt32(this.manacUpDown.Value));
		if (pool > this.attributeNumericUpDown.Maximum)
			this.attributeNumericUpDown.Maximum = pool;

		foreach (var (box, minimum) in new[]
		{
			(this.strengthUpDown, PlayerLevel.MinStrength),
			(this.dexterityUpDown, PlayerLevel.MinDexterity),
			(this.intelligenceUpDown, PlayerLevel.MinIntelligence),
			(this.healthUpDown, PlayerLevel.MinHealth),
			(this.manacUpDown, PlayerLevel.MinMana),
		})
			box.Value = Math.Clamp(minimum, box.Minimum, box.Maximum);
	}

	private void difficultlyComboBox_EnabledChanged(object? sender, EventArgs e)
	{
		// Init difficulty combo
		if (levelingCheckBox.Checked && difficultlyComboBox.Enabled)
			difficultlyComboBox.SelectedIndex = Info.DifficultyUnlocked;
	}
}