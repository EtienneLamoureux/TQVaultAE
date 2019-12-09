namespace TQVaultAE.GUI
{
	using System;
	using System.Windows.Forms;
	using TQVaultAE.Domain.Entities;
	using TQVaultAE.Presentation;
	using TQVaultAE.Logs;
	using log4net;
	using System.Linq;

	/// <summary>
	/// Dialog box class for the Item Seed Dialog
	/// </summary>
	internal partial class CharacterEditDialog : VaultForm
	{

		private struct DifficultData
		{
			public string Text;
			public int Id;

			public override string ToString() => Text;
		}

		private readonly ILog Log;

		internal PlayerCollection PlayerCollection { get; set; }

		/// <summary>
		/// Initializes a new instance of the ItemSeedDialog class.
		/// </summary>
		public CharacterEditDialog(IServiceProvider serviceProvider, ILogger<CharacterEditDialog> log) : base(serviceProvider)
		{
			this.Log = log.Logger;

			this.InitializeComponent();

			#region Apply custom font

			this.ResetMasteriesScalingButton.Font = FontService.GetFontAlbertusMTLight(12F);
			this.ok.Font = FontService.GetFontAlbertusMTLight(12F);
			this.cancel.Font = FontService.GetFontAlbertusMTLight(12F);
			this.Font = FontService.GetFontAlbertusMTLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			new[] {
				this.attribGroupBox,
				this.levelingGroupBox,
				this.MasteriesGroupBox
			}.SelectMany(gb => gb.Controls.OfType<Control>(), (gb, child) => (gb, child))
				.ToList()
				.ForEach(tup => tup.gb.Font = tup.child.Font = FontService.GetFontAlbertusMTLight(11F));

			#endregion

			this.NormalizeBox = false;
			this.DrawCustomBorder = true;

			this.cancel.Text = Resources.GlobalCancel;
			this.ok.Text = Resources.GlobalOK;
			this.ResetMasteriesScalingButton.Text = Resources.ResetMasteriesButton;
		}

		private void SetDifficultly()
		{
			var pd = PlayerCollection.PlayerInfo.DifficultyUnlocked;
			difficultlyComboBox.SelectedIndex = Enumerable.Range(0, 2).Contains(pd) ? pd : 0;
		}

		/// <summary>
		/// Cancel button handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButton_Click(object sender, EventArgs e) => this.Close();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="playerInfo"></param>
		private void UpdateMoneySituation(PlayerInfo playerInfoSrc, PlayerInfo playerInfoDst)
		{
			if (playerInfoDst.DifficultyUnlocked != playerInfoSrc.DifficultyUnlocked)
			{
				if (playerInfoDst.DifficultyUnlocked == 1)
				{
					if (playerInfoDst.Money < 5000000)
						playerInfoDst.Money = 5000000;
				}
				if (playerInfoDst.DifficultyUnlocked == 2)
				{
					if (playerInfoDst.Money < 7500000)
						playerInfoDst.Money = 7500000;
				}
			}
		}

		/// <summary>
		/// OK button handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void OKButton_Click(object sender, EventArgs e)
		{
			UpdatePlayerInfo();
		}

		private void UpdatePlayerInfo()
		{
			if (!Config.Settings.Default.AllowCharacterEdit) return;
			if (PlayerCollection.PlayerInfo == null) return;
			try
			{
				var playerInfo = new PlayerInfo();
				playerInfo.CurrentLevel = Convert.ToInt32(levelNumericUpDown.Value);

				if (playerInfo.CurrentLevel < 2)
					playerInfo.MasteriesAllowed = 0;
				else if (playerInfo.CurrentLevel < 8)
					playerInfo.MasteriesAllowed = 1;
				else
					playerInfo.MasteriesAllowed = 2;

				playerInfo.CurrentXP = int.Parse(xpTextBox.Text);
				playerInfo.Money = PlayerCollection.PlayerInfo.Money > 0 ? PlayerCollection.PlayerInfo.Money : 0;
				playerInfo.DifficultyUnlocked = difficultlyComboBox.SelectedIndex;
				playerInfo.AttributesPoints = Convert.ToInt32(attributeNumericUpDown.Value);
				playerInfo.SkillPoints = Convert.ToInt32(skillPointsNumericUpDown.Value);
				playerInfo.BaseStrength = Convert.ToInt32(strengthUpDown.Value);
				playerInfo.BaseDexterity = Convert.ToInt32(dexterityUpDown.Value);
				playerInfo.BaseIntelligence = Convert.ToInt32(intelligenceUpDown.Value);
				playerInfo.BaseHealth = Convert.ToInt32(healthUpDown.Value);
				playerInfo.BaseMana = Convert.ToInt32(manacUpDown.Value);
				playerInfo.MasteriesResetRequiered = this._MasteriesResetRequiered;
				UpdateMoneySituation(PlayerCollection.PlayerInfo, playerInfo);
				PlayerCollectionProvider.CommitPlayerInfo(PlayerCollection, playerInfo);

				this.Close();
			}
			catch (Exception ex)
			{
				this.Log.ErrorException(ex);
				MessageBox.Show(ex.Message);
			}
		}

		private bool _loaded = false;
		private bool _MasteriesResetRequiered;

		private struct TagData
		{
			public int IncrementValue;
		}

		/// <summary>
		/// Dialog load handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CharacterEditDlg_Load(object sender, EventArgs e)
		{

			strengthLabel.Text = Resources.CEStrength;
			dexterityLabel.Text = Resources.CEDexterity;
			IntelligenceLabel.Text = Resources.CEIntelligence;
			healthLabel.Text = Resources.CEHealth;
			manaLabel.Text = Resources.CEMana;
			levelLabel1.Text = Resources.CELevel;
			xpLabel1.Text = Resources.CEXp;
			attributeLabel1.Text = Resources.CEAttributePoints;
			skillPointsLabel1.Text = Resources.CESkillPoints;
			levelingCheckBox.Text = Resources.CEEnableLeveling;
			levelingGroupBox.Text = Resources.CELeveling;
			attribGroupBox.Text = Resources.CEAttributes;
			difficultyLabel.Text = Resources.CEDifficulty;

			this.MasteriesGroupBox.Text = Resources.Masteries;

			this.Mastery1NameScalingLabel.Visible
				= this.Mastery1ValueScalingLabel.Visible
				= this.Mastery2NameScalingLabel.Visible
				= this.Mastery2ValueScalingLabel.Visible = false;

			var dbr = PlayerCollection.PlayerInfo.ActiveMasteriesRecordNames;
			if (dbr.Any())
			{
				for (int i = 0; i < dbr.Length; i++)
				{
					var recId = dbr[i];
					var relatedSkills = PlayerCollection.PlayerInfo.GetSkillsByBaseRecordName(recId);
					var relatedPoints = relatedSkills.Sum(s => s.skillLevel);
					var masteryInfo = this.Database.GetInfo(recId);
					var masteryName = this.Database.GetFriendlyName(masteryInfo.DescriptionTag);
					var label = this.Controls.Find($"Mastery{i + 1}NameScalingLabel", true).First();
					label.Text = string.Format(label.Tag.ToString(), masteryName);
					label.Visible = true;
					var value = this.Controls.Find($"Mastery{i + 1}ValueScalingLabel", true).First();
					value.Text = string.Format(value.Tag.ToString(), relatedSkills.Count(), relatedPoints);
					value.Visible = true;
				}
			}

			if (PlayerCollection.PlayerInfo.BaseStrength > Convert.ToInt32(strengthUpDown.Maximum))
				strengthUpDown.Maximum = PlayerCollection.PlayerInfo.BaseStrength;

			strengthUpDown.Value = PlayerCollection.PlayerInfo.BaseStrength;
			var attrTag = new TagData { IncrementValue = PlayerLevel.AtrributeIncrementPerPoint };
			strengthUpDown.Tag = attrTag;

			if (PlayerCollection.PlayerInfo.BaseDexterity > Convert.ToInt32(dexterityUpDown.Maximum))
				dexterityUpDown.Maximum = PlayerCollection.PlayerInfo.BaseDexterity;

			dexterityUpDown.Value = PlayerCollection.PlayerInfo.BaseDexterity;
			dexterityUpDown.Tag = attrTag;

			if (PlayerCollection.PlayerInfo.BaseIntelligence > Convert.ToInt32(intelligenceUpDown.Maximum))
				intelligenceUpDown.Maximum = PlayerCollection.PlayerInfo.BaseIntelligence;

			intelligenceUpDown.Value = PlayerCollection.PlayerInfo.BaseIntelligence;
			intelligenceUpDown.Tag = attrTag;

			if (PlayerCollection.PlayerInfo.BaseHealth > Convert.ToInt32(healthUpDown.Maximum))
				healthUpDown.Maximum = PlayerCollection.PlayerInfo.BaseHealth;

			healthUpDown.Value = PlayerCollection.PlayerInfo.BaseHealth;
			var attrHMTag = new TagData { IncrementValue = PlayerLevel.HealthAndManaIncrementPerPoint };
			healthUpDown.Tag = attrHMTag;

			if (PlayerCollection.PlayerInfo.BaseMana > Convert.ToInt32(manacUpDown.Maximum))
				manacUpDown.Maximum = PlayerCollection.PlayerInfo.BaseMana;

			manacUpDown.Value = PlayerCollection.PlayerInfo.BaseMana;
			manacUpDown.Tag = attrHMTag;

			if (PlayerCollection.PlayerInfo.CurrentLevel > Convert.ToInt32(levelNumericUpDown.Maximum))
				levelNumericUpDown.Maximum = PlayerCollection.PlayerInfo.CurrentLevel;

			levelNumericUpDown.Value = PlayerCollection.PlayerInfo.CurrentLevel;

			xpTextBox.Text = string.Format("{0}", PlayerCollection.PlayerInfo.CurrentXP);

			if (PlayerCollection.PlayerInfo.AttributesPoints > Convert.ToInt32(attributeNumericUpDown.Maximum))
				attributeNumericUpDown.Maximum = PlayerCollection.PlayerInfo.AttributesPoints;

			attributeNumericUpDown.Value = PlayerCollection.PlayerInfo.AttributesPoints;

			if (PlayerCollection.PlayerInfo.SkillPoints > Convert.ToInt32(skillPointsNumericUpDown.Maximum))
				skillPointsNumericUpDown.Maximum = PlayerCollection.PlayerInfo.SkillPoints;

			skillPointsNumericUpDown.Value = PlayerCollection.PlayerInfo.SkillPoints;

			difficultlyComboBox.Items.Clear();
			difficultlyComboBox.Items.Add(new DifficultData { Text = Resources.Difficulty0, Id = 0 });
			difficultlyComboBox.Items.Add(new DifficultData { Text = Resources.Difficulty1, Id = 1 });
			difficultlyComboBox.Items.Add(new DifficultData { Text = Resources.Difficulty2, Id = 2 });

			SetDifficultly();

			levelingCheckBox.Checked = false;

			if (PlayerCollection.PlayerInfo.HasBeenInGame == 0)
			{
				levelingCheckBox.Enabled = false;
				levelingCheckBox.Visible = false;
			}

			_loaded = true;
		}

		private void LevelNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (!_loaded) return;

			try
			{
				var prevLvl = Convert.ToInt32(((UpDownBase)sender).Text);
				var level = Convert.ToInt32(((NumericUpDown)sender).Value);
				var xp = PlayerLevel.GetLevelMinXP(level);
				xpTextBox.Text = string.Format("{0}", (xp + 1));

				var dif = level - prevLvl;
				var attr = dif * PlayerLevel.AtrributePointsPerLevel;
				var skills = dif * PlayerLevel.SkillPointsPerLevel;

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
				Log.ErrorException(ex);
			}
		}

		private void StatsUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (!_loaded) return;
			if (sender == null) return;
			var upDwnCtrl = (NumericUpDown)sender;
			if (upDwnCtrl.Tag == null) return;

			var prevValue = Convert.ToInt32(((UpDownBase)sender).Text);
			var value = Convert.ToInt32(upDwnCtrl.Value);

			var dif = prevValue - value;

			var tag = (TagData)upDwnCtrl.Tag;
			var newValue = Convert.ToInt32(dif / tag.IncrementValue);
			var newAttr = attributeNumericUpDown.Value + newValue;

			if (newAttr < 0)
			{
				((NumericUpDown)sender).Value = prevValue;
				return;
			}

			if (newAttr <= Convert.ToInt32(attributeNumericUpDown.Maximum)
				&& newAttr >= Convert.ToInt32(attributeNumericUpDown.Minimum))
			{
				attributeNumericUpDown.Value = newAttr;
			}

		}

		private void LevelingCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (sender == null) return;

			var chkbx = (CheckBox)sender;
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

		private void ResetMasteriesScalingButton_Click(object sender, EventArgs e)
		{
			if (!_MasteriesResetRequiered) _MasteriesResetRequiered = true;
			UpdatePlayerInfo();
		}
	}
}