using TQVaultAE.Application;
using TQVaultAE.Application.DTOs;
using TQVaultAE.Domain.Helpers;
using TQVaultAE.GUI.Forms;

namespace TQVaultAE.GUI.Dialogs;

using System;
using System.Windows.Forms;
using TQVaultAE.Domain.Entities;
using TQVaultAE.Presentation;
using System.Linq;
using Microsoft.Extensions.Logging;

/// <summary>
/// Dialog box class for the Item Seed Dialog
/// </summary>
internal sealed partial class CharacterEditDialog : VaultForm
{
	private struct DifficultData
	{
		public string Text;
		public int Id;

		public override string ToString() => Text;
	}

	private readonly ILogger Log;

	/// <summary>Character to edit, set before the dialog is shown.</summary>
	internal PlayerCollection PlayerCollection { get; set; } = null!;

	/// <summary>The dialog is only opened for a loaded character, which has its player info.</summary>
	private PlayerInfo Info => this.PlayerCollection.PlayerInfo
		?? throw new InvalidOperationException("The character has no player info.");

	/// <summary>
	/// Initializes a new instance of the ItemSeedDialog class.
	/// </summary>
	public CharacterEditDialog(GuiServices services, ILogger<CharacterEditDialog> log) : base(services)
	{
		this.Log = log;

		this.InitializeComponent();

		#region Apply custom font

		this.ResetMasteriesScalingButton.Font = 
			this.ResetAttributesScalingButton.Font =
				this.ok.Font = 
					this.cancel.Font = FontService.GetFontLight(12F);

		this.Font = FontService.GetFontLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));


		#endregion

		this.NormalizeBox = false;
		this.DrawCustomBorder = true;

		this.cancel.Text = Resources.GlobalCancel;
		this.ok.Text = Resources.GlobalOK;
		this.ResetMasteriesScalingButton.Text = Resources.ResetMasteriesButton;
		this.ResetAttributesScalingButton.Text = Resources.ResetAttributesButton;
	}


	/// <summary>
	/// Cancel button handler
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CancelButton_Click(object? sender, EventArgs e) => this.Close();

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

			this.Close();
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
	/// Dialog load handler
	/// </summary>
	/// <param name="sender">sender object</param>
	/// <param name="e">EventArgs data</param>
	private void CharacterEditDlg_Load(object? sender, EventArgs e)
	{
		this.InitCharacterPage();

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