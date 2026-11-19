using TQVaultAE.GUI.Models;
using TQVaultAE.Application.Contracts.Services;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Application.Session;
using TQVaultAE.Config;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.Presentation.Win32.Contracts;

namespace TQVaultAE.GUI.Views;

using System;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using TQVaultAE.GUI.Components.Scaling;
using System.Linq;
using Microsoft.Extensions.Logging;

/// <summary>
/// The character editor: the character's statistics, leveling and masteries drawn as the game's character and skill
/// windows, with OK and Cancel. Its host shows a character with <see cref="ShowCharacter"/> once the control is in a
/// window (it is built for that character only), and leaves it on <see cref="Applied"/> (OK, the edits applied to the
/// character in memory) or <see cref="Cancelled"/> (Cancel, the edits dropped).
/// </summary>
internal sealed partial class CharacterEditorUserControl : MainWindowView, IMainWindowView, IIgnoresZoom
{
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

	// Applied: OK was pressed and the edits applied to the character, in memory (the Save button writes them).

	/// <summary>The Cancel button, which a host may press for Escape.</summary>
	public IButtonControl CancelButton => this.cancel;

	/// <summary>Escape presses Cancel.</summary>
	protected override IButtonControl EscapeButton => this.cancel;

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
		&& (this.CurrentState() != this._startState || this._skills?.Changed == true || this._skills?.MasteriesChanged == true || this._classSets.Changed);

	/// <inheritdoc/>
	public string DiscardChangesQuestion => Resources.CharacterViewDiscardChanges;

	/// <summary>The values of the editors, to tell whether the user changed any.</summary>
	private readonly record struct EditorState(int Level, string Xp, int Difficulty, int SkillPoints, int AttributePoints, int Health, int Mana, int Strength, int Intelligence, int Dexterity);

	/// <summary>The values of the editors once the character is shown.</summary>
	private EditorState _startState;

	/// <summary>Shows <paramref name="value"/> in <paramref name="box"/>, its maximum raised to it when lower.</summary>
	private static void Fill(NumericUpDown box, int value)
	{
		if (value > box.Maximum)
			box.Maximum = value;
		box.Value = value;
	}

	/// <summary>The whole number a numeric box shows.</summary>
	private static int ValueOf(NumericUpDown box) => Convert.ToInt32(box.Value);

	private EditorState CurrentState() => new(
		ValueOf(this.levelNumericUpDown),
		this.xpTextBox.Text,
		// Ticking Enable leveling selects the difficulty unlocked: no change until another one is chosen.
		this.difficultlyComboBox.Enabled ? this.difficultlyComboBox.SelectedIndex : this.Info.DifficultyUnlocked,
		ValueOf(this.skillPointsNumericUpDown),
		ValueOf(this.attributeNumericUpDown),
		ValueOf(this.healthUpDown),
		ValueOf(this.manacUpDown),
		ValueOf(this.strengthUpDown),
		ValueOf(this.intelligenceUpDown),
		ValueOf(this.dexterityUpDown));


	/// <summary>
	/// Scrolled (an area smaller than the editor), the editor stays where the user put it when a control takes the
	/// focus: it jumped to the first control at each opening, and to any control clicked.
	/// </summary>
	protected override Point ScrollToControl(Control activeControl) => this.DisplayRectangle.Location;

	/// <summary>
	/// The editor is drawn at the game's size times the DPI and its fit to the room it is given (FitToRoom), whatever the
	/// main window's zoom: a zoom does not scale it, the new room fits it. A move to a monitor of another DPI scales it, as Windows Forms scales any window: the controls of the
	/// designer and their fonts here, the drawn parts laid out again afterwards (<see cref="OnDpiChangedAfterParent"/>).
	/// </summary>
	protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
	{
		if (!this._loaded || !ZoomScope.IsZooming)
			base.ScaleControl(factor, specified);
	}

	/// <inheritdoc cref="ScaleControl"/>
	protected override bool ScaleChildren => !this._loaded || !ZoomScope.IsZooming;


	/// <summary>
	/// Cancel button handler
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CancelButton_Click(object? sender, EventArgs e) => this.RaiseCancelled();

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
				Level: ValueOf(levelNumericUpDown)
				, XpText: xpTextBox.Text
				, DifficultyUnlocked: difficultlyComboBox.Enabled ? difficultlyComboBox.SelectedIndex : null
				, SkillPoints: ValueOf(skillPointsNumericUpDown)
				, AttributePoints: ValueOf(attributeNumericUpDown)
				, Strength: ValueOf(strengthUpDown)
				, Dexterity: ValueOf(dexterityUpDown)
				, Intelligence: ValueOf(intelligenceUpDown)
				, Health: ValueOf(healthUpDown)
				, Mana: ValueOf(manacUpDown)
				, SkillRecords: this._skills?.Changed == true ? this._skills.BuildSkillRecords() : null
				, MasteriesChanged: this._skills?.MasteriesChanged == true);

			// The class sets first: when their file cannot be written the editor stays open, nothing applied.
			if (!this.SaveClassSets())
				return;

			this.Services.CharacterEditService.Apply(PlayerCollection, request);

			this.RaiseApplied();
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

		Fill(strengthUpDown, Info.BaseStrength);
		var attrTag = new TagData { IncrementValue = PlayerLevel.AtrributeIncrementPerPoint };
		strengthUpDown.Tag = attrTag;

		Fill(dexterityUpDown, Info.BaseDexterity);
		dexterityUpDown.Tag = attrTag;

		Fill(intelligenceUpDown, Info.BaseIntelligence);
		intelligenceUpDown.Tag = attrTag;

		Fill(healthUpDown, Info.BaseHealth);
		var attrHMTag = new TagData { IncrementValue = PlayerLevel.HealthAndManaIncrementPerPoint };
		healthUpDown.Tag = attrHMTag;

		Fill(manacUpDown, Info.BaseMana);
		manacUpDown.Tag = attrHMTag;

		Fill(levelNumericUpDown, Info.CurrentLevel);

		xpTextBox.Text = string.Format("{0}", Info.CurrentXP);

		Fill(attributeNumericUpDown, Info.AttributesPoints);

		Fill(skillPointsNumericUpDown, Info.SkillPoints);

		this.InitSkills();

		difficultlyComboBox.Items.Clear();
		difficultlyComboBox.Items.Add(new DisplayItem<int>(0, TranslationService.TranslateDifficulty(0)));
		difficultlyComboBox.Items.Add(new DisplayItem<int>(1, TranslationService.TranslateDifficulty(1)));
		difficultlyComboBox.Items.Add(new DisplayItem<int>(2, TranslationService.TranslateDifficulty(2)));

		levelingCheckBox.Checked = false;

		if (Info.HasBeenInGame == 0)
		{
			levelingCheckBox.Enabled = false;
			levelingCheckBox.Visible = false;
		}

		foreach (var upDown in new[] { strengthUpDown, dexterityUpDown, intelligenceUpDown, healthUpDown, manacUpDown, levelNumericUpDown })
			this._previousValues[upDown] = ValueOf(upDown);

		this._startAttributes = this.AttributeValues;
		this.InitClassSets();
		this.FitToFrame();
		_loaded = true;
		this._startState = this.CurrentState();
		this.PostFitToRoom();
	}

	private void LevelNumericUpDown_ValueChanged(object? sender, EventArgs e)
	{
		if (!_loaded) return;

		try
		{
			var upDown = (NumericUpDown)sender!;
			var prevLvl = this._previousValues[upDown];
			var level = ValueOf(upDown);
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
		var value = ValueOf(upDwnCtrl);

		var tag = (TagData)upDwnCtrl.Tag;
		if (!PlayerLevel.TryChangeAttribute(prevValue, value, tag.IncrementValue, ValueOf(attributeNumericUpDown), out var newAttr))
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
		var pool = ValueOf(this.attributeNumericUpDown) + PlayerLevel.GetInvestedAttributePoints([
			.. this.AttributeBoxes.Select(ValueOf)]);
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