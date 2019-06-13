namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.ComponentModel;
	using System.Drawing;
	using System.Globalization;
	using System.Windows.Forms;
	using TQVaultAE.DAL;
	using TQVaultData;

	/// <summary>
	/// Dialog box class for the Item Seed Dialog
	/// </summary>
	internal partial class CharacterEditDialog : VaultForm
	{

		private struct DifficultData
		{
			public string Text;
			public int Id;

			public override string ToString()
			{
				return Text;
			}
		}

		/// <summary>
		/// MessageBoxOptions for right to left reading.
		/// </summary>
		private static MessageBoxOptions rightToLeftOptions = (MessageBoxOptions)0;

		/// <summary>
		/// Selected Item
		/// </summary>
		private Item selectedItem;

		private PlayerCollection _playerCollection;

		/// <summary>
		/// Initializes a new instance of the ItemSeedDialog class.
		/// </summary>
		public CharacterEditDialog(PlayerCollection playerCollection)
		{
			this.InitializeComponent();

			_playerCollection = playerCollection;

			#region Apply custom font

			this.ok.Font = Program.GetFontAlbertusMTLight(12F);
			this.cancel.Font = Program.GetFontAlbertusMTLight(12F);
			this.Font = Program.GetFontAlbertusMTLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

			#endregion

			this.DrawCustomBorder = true;

			this.cancel.Text = Resources.GlobalCancel;
			this.ok.Text = Resources.GlobalOK;

			// Set options for Right to Left reading.
			if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
			{
				rightToLeftOptions = MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading;
			}
		}

		private void setDifficultly()
		{
			switch (_playerCollection.PlayerInfo.DifficultyUnlocked)
			{
				default:
				case 0:
					difficultlyComboBox.SelectedIndex = 0;
					break;
				case 1:
					difficultlyComboBox.SelectedIndex = 1;
					break;
				case 2:
					difficultlyComboBox.SelectedIndex = 2;
					break;
			}
		}

		/// <summary>
		/// Gets or sets the selected item
		/// </summary>
		public Item SelectedItem
		{
			get
			{
				return this.selectedItem;
			}

			set
			{
				this.selectedItem = value;
			}
		}


		/// <summary>
		/// Cancel button handler
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void CancelButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

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
					{
						playerInfoDst.Money = 5000000;
					}
				}
				if (playerInfoDst.DifficultyUnlocked == 2)
				{
					if (playerInfoDst.Money < 7500000)
					{
						playerInfoDst.Money = 7500000;
					}
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
			if (_playerCollection.PlayerInfo == null) return;
			try
			{
				var playerInfo = new PlayerInfo();
				playerInfo.CurrentLevel = Convert.ToInt32(levelNumericUpDown.Value);
				playerInfo.CurrentXP = int.Parse(xpTextBox.Text);
				playerInfo.Money = _playerCollection.PlayerInfo.Money > 0 ? _playerCollection.PlayerInfo.Money : 0;
				playerInfo.DifficultyUnlocked = difficultlyComboBox.SelectedIndex;
				playerInfo.AttributesPoints = Convert.ToInt32(attributeNumericUpDown.Value);
				playerInfo.SkillPoints = Convert.ToInt32(skillPointsNumericUpDown.Value);
				playerInfo.BaseStrength = Convert.ToInt32(strengthUpDown.Value);
				playerInfo.BaseDexterity = Convert.ToInt32(dexterityUpDown.Value);
				playerInfo.BaseIntelligence = Convert.ToInt32(intelligenceUpDown.Value);
				playerInfo.BaseHealth = Convert.ToInt32(healthUpDown.Value);
				playerInfo.BaseMana = Convert.ToInt32(manacUpDown.Value);
				UpdateMoneySituation(_playerCollection.PlayerInfo,playerInfo);
				_playerCollection.CommitPlayerInfo(playerInfo);
				this.Close();
			}catch(Exception ex)
			{
				MessageBox.Show(string.Format("{0}",ex.Message));
			}
		}

		private bool _loaded = false;

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
			redistrbuteCheckbox.Text = Resources.CEEnableRedistribute;
			levelingGroupBox.Text = Resources.CELeveling;
			attribGroupBox.Text = Resources.CEAttributes;
			difficultyLabel.Text = Resources.CEDifficulty;


			if (_playerCollection.PlayerInfo.BaseStrength > Convert.ToInt32(strengthUpDown.Maximum))
			{
				strengthUpDown.Maximum = _playerCollection.PlayerInfo.BaseStrength;
			}
			strengthUpDown.Value = _playerCollection.PlayerInfo.BaseStrength;
			var attrTag = new TagData { IncrementValue = PlayerLevel.AtrributeIncrementPerPoint };
			strengthUpDown.Tag = attrTag;

			if (_playerCollection.PlayerInfo.BaseDexterity > Convert.ToInt32(dexterityUpDown.Maximum))
			{
				dexterityUpDown.Maximum = _playerCollection.PlayerInfo.BaseDexterity;
			}
			dexterityUpDown.Value = _playerCollection.PlayerInfo.BaseDexterity;
			dexterityUpDown.Tag = attrTag;

			if (_playerCollection.PlayerInfo.BaseIntelligence > Convert.ToInt32(intelligenceUpDown.Maximum))
			{
				intelligenceUpDown.Maximum = _playerCollection.PlayerInfo.BaseIntelligence;
			}
			intelligenceUpDown.Value = _playerCollection.PlayerInfo.BaseIntelligence;
			intelligenceUpDown.Tag = attrTag;

			if (_playerCollection.PlayerInfo.BaseHealth > Convert.ToInt32(healthUpDown.Maximum))
			{
				healthUpDown.Maximum = _playerCollection.PlayerInfo.BaseHealth;
			}
			healthUpDown.Value = _playerCollection.PlayerInfo.BaseHealth;
			var attrHMTag = new TagData { IncrementValue = PlayerLevel.HealthAndManaIncrementPerPoint };
			healthUpDown.Tag = attrHMTag;

			if (_playerCollection.PlayerInfo.BaseMana > Convert.ToInt32(manacUpDown.Maximum))
			{
				manacUpDown.Maximum = _playerCollection.PlayerInfo.BaseMana;
			}
			manacUpDown.Value = _playerCollection.PlayerInfo.BaseMana;
			manacUpDown.Tag = attrHMTag;

			if (_playerCollection.PlayerInfo.CurrentLevel > Convert.ToInt32(levelNumericUpDown.Maximum))
			{
				levelNumericUpDown.Maximum = _playerCollection.PlayerInfo.CurrentLevel;
			}
			levelNumericUpDown.Value = _playerCollection.PlayerInfo.CurrentLevel;

			xpTextBox.Text = string.Format("{0}", _playerCollection.PlayerInfo.CurrentXP);

			if (_playerCollection.PlayerInfo.AttributesPoints > Convert.ToInt32(attributeNumericUpDown.Maximum))
			{
				attributeNumericUpDown.Maximum = _playerCollection.PlayerInfo.AttributesPoints;
			}
			attributeNumericUpDown.Value = _playerCollection.PlayerInfo.AttributesPoints;

			if (_playerCollection.PlayerInfo.SkillPoints > Convert.ToInt32(skillPointsNumericUpDown.Maximum))
			{
				skillPointsNumericUpDown.Maximum = _playerCollection.PlayerInfo.SkillPoints;
			}
			skillPointsNumericUpDown.Value = _playerCollection.PlayerInfo.SkillPoints;

			difficultlyComboBox.Items.Clear();
			difficultlyComboBox.Items.Add(new DifficultData { Text = Resources.Difficulty0, Id = 0 });
			difficultlyComboBox.Items.Add(new DifficultData { Text = Resources.Difficulty1, Id = 1 });
			difficultlyComboBox.Items.Add(new DifficultData { Text = Resources.Difficulty2, Id = 2 });

			setDifficultly();

			redistrbuteCheckbox.Checked = true;
			levelingCheckBox.Checked = false;

			if (_playerCollection.PlayerInfo.HasBeenInGame == 0)
			{
				levelingCheckBox.Enabled = false;
				levelingCheckBox.Visible = false;
			}

			_loaded = true;
		}

		private void GroupBox1_Enter(object sender, EventArgs e)
		{

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
				if ((newAttr) <= Convert.ToInt32(attributeNumericUpDown.Maximum)
					&& (newAttr) >= Convert.ToInt32(attributeNumericUpDown.Minimum))
				{
					attributeNumericUpDown.Value = newAttr;
				}

				var newSkills = skillPointsNumericUpDown.Value + skills;
				if ((newSkills) <= Convert.ToInt32(skillPointsNumericUpDown.Maximum)
					&& (newSkills) >= Convert.ToInt32(skillPointsNumericUpDown.Minimum))
				{
					skillPointsNumericUpDown.Value = newSkills;
				}


			}
			catch
			{

			}


		}

		private void StatsUpDown_ValueChanged(object sender, EventArgs e)
		{
			if (!_loaded) return;
			if (sender == null) return;
			var upDwnCtrl = (NumericUpDown)sender;
			if (upDwnCtrl.Tag == null) return;

			if (!redistrbuteCheckbox.Checked) return;

			var prevValue = Convert.ToInt32(((UpDownBase)sender).Text);
			var value = Convert.ToInt32(upDwnCtrl.Value);

			var dif = prevValue - value;

			var tag = (TagData)upDwnCtrl.Tag;
			var newValue = Convert.ToInt32(dif/tag.IncrementValue);
			var newAttr = attributeNumericUpDown.Value + newValue;
			if (newAttr < 0)
			{
				((NumericUpDown)sender).Value = prevValue;
				return;
			}
			if ((newAttr) <= Convert.ToInt32(attributeNumericUpDown.Maximum)
				&& (newAttr) >= Convert.ToInt32(attributeNumericUpDown.Minimum))
			{
				attributeNumericUpDown.Value = newAttr;
			}

		}

		private void RedistrbuteCheckbox_CheckedChanged(object sender, EventArgs e)
		{
			if (sender == null) return;

			var chkbx = (CheckBox)sender;
			if (chkbx.Checked)
			{

			}
			else
			{

			}

		}

		private void LevelingCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (sender == null) return;

			var chkbx = (CheckBox)sender;
			if (chkbx.Checked)
			{
				//this.manacUpDown 
				//this.healthUpDown
				//this.intelligenceUpDown 
				//this.dexterityUpDown 
				//this.strengthUpDown 
				this.difficultlyComboBox.Enabled = true;
				this.skillPointsNumericUpDown.Enabled = true;
				this.attributeNumericUpDown.Enabled = true;
				this.levelNumericUpDown.Enabled = true; 
			}
			else
			{
				this.difficultlyComboBox.Enabled = false;
				this.skillPointsNumericUpDown.Enabled = false;
				this.attributeNumericUpDown.Enabled = false;
				this.levelNumericUpDown.Enabled = false;
			}

		}
	}
}