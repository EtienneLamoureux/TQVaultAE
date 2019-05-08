//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace TQVaultAE.GUI
{
	using Properties;
	using System;
	using System.Collections.Generic;
	using System.Drawing;
	using System.Globalization;
	using System.Linq;
	using System.Windows.Forms;
	using TQVaultData;

	/// <summary>
	/// Class for the Settings/Configuration Dialog
	/// </summary>
	internal partial class SettingsDialog : VaultForm
	{
		/// <summary>
		/// Indicates whether the title screen will be skipped on startup
		/// </summary>
		private bool skipTitle;

		/// <summary>
		/// Save path for the vault files
		/// </summary>
		private string vaultPath;

		/// <summary>
		/// Indicates whether item copying is allowed
		/// </summary>
		private bool allowItemCopy;

		/// <summary>
		/// Indicates whether item editing is allowed
		/// </summary>
		private bool allowItemEdit;

		/// <summary>
		/// Indicates whether the last opened character will be loaded at startup
		/// </summary>
		private bool loadLastCharacter;

		/// <summary>
		/// Indicates whether the last opened vault will be loaded at startup
		/// </summary>
		private bool loadLastVault;

		/// <summary>
		/// Indicates whether the language will be auto detected
		/// </summary>
		private bool detectLanguage;

		/// <summary>
		/// The language we will be using.
		/// </summary>
		private string titanQuestLanguage;

		/// <summary>
		/// Indicates whether the game paths will be auto detected
		/// </summary>
		private bool detectGamePath;

		/// <summary>
		/// Titan Quest game path
		/// </summary>
		private string titanQuestPath;

		/// <summary>
		/// Immortal Throne game path
		/// </summary>
		private string immortalThronePath;

		/// <summary>
		/// Indicates whether custom maps have been enabled
		/// </summary>
		private bool enableMods;

		/// <summary>
		/// Current custom map
		/// </summary>
		private string customMap;

		/// <summary>
		/// Indicates whether we are loading all data files on startup
		/// </summary>
		private bool loadAllFiles;

		/// <summary>
		/// Indicates whether warning messages are suppressed
		/// </summary>
		private bool suppressWarnings;

		/// <summary>
		/// Indicates whether player items will be readonly.
		/// </summary>
		private bool playerReadonly;

		/// <summary>
		/// Indicates whether the vault path has been changed
		/// </summary>
		private bool vaultPathChanged;

		/// <summary>
		/// Indicates whether the play list filter has been changed
		/// </summary>
		private bool playerFilterChanged;

		/// <summary>
		/// Indicates that any configuration item has been changed
		/// </summary>
		private bool configurationChanged;

		/// <summary>
		/// Indicates that the UI setting has changed.
		/// </summary>
		private bool uiSettingChanged;

		/// <summary>
		/// Indicates that the settings have been loaded
		/// </summary>
		private bool settingsLoaded;

		/// <summary>
		/// Indicates that the language setting has been changed
		/// </summary>
		private bool languageChanged;

		/// <summary>
		/// Indicates that the game language has been changed
		/// </summary>
		private bool gamePathChanged;

		/// <summary>
		/// Indicates that the custom map selection has changed
		/// </summary>
		private bool customMapsChanged;

		/// <summary>
		/// Initializes a new instance of the SettingsDialog class.
		/// </summary>
		public SettingsDialog()
		{
			this.InitializeComponent();

			#region Apply custom font

			this.allowItemEditCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.allowItemCopyCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.skipTitleCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.loadLastCharacterCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.loadLastVaultCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.vaultPathTextBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.vaultPathLabel.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.cancelButton.Font = Program.GetFontAlbertusMTLight(12F);
			this.okayButton.Font = Program.GetFontAlbertusMTLight(12F);
			this.resetButton.Font = Program.GetFontAlbertusMTLight(12F);
			this.vaultPathBrowseButton.Font = Program.GetFontAlbertusMTLight(12F);
			this.enableCustomMapsCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.loadAllFilesCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.suppressWarningsCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.playerReadonlyCheckbox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.languageComboBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.languageLabel.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.detectLanguageCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.titanQuestPathTextBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.titanQuestPathLabel.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.immortalThronePathLabel.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.immortalThronePathTextBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.detectGamePathsCheckBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.titanQuestPathBrowseButton.Font = Program.GetFontAlbertusMTLight(12F);
			this.immortalThronePathBrowseButton.Font = Program.GetFontAlbertusMTLight(12F);
			this.customMapLabel.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.mapListComboBox.Font = Program.GetFontAlbertusMTLight(11.25F);
			this.Font = Program.GetFontAlbertusMTLight(11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)(0));

			#endregion

			this.vaultPathLabel.Text = Resources.SettingsLabel1;
			this.languageLabel.Text = Resources.SettingsLabel2;
			this.titanQuestPathLabel.Text = Resources.SettingsLabel3;
			this.immortalThronePathLabel.Text = Resources.SettingsLabel4;
			this.customMapLabel.Text = Resources.SettingsLabel5;
			this.detectGamePathsCheckBox.Text = Resources.SettingsDetectGamePath;
			this.detectLanguageCheckBox.Text = Resources.SettingsDetectLanguage;
			this.enableCustomMapsCheckBox.Text = Resources.SettingsEnableMod;
			this.toolTip.SetToolTip(this.enableCustomMapsCheckBox, Resources.SettingsEnableModTT);
			this.skipTitleCheckBox.Text = Resources.SettingsSkipTitle;
			this.toolTip.SetToolTip(this.skipTitleCheckBox, Resources.SettingsSkipTitleTT);
			this.allowItemCopyCheckBox.Text = Resources.SettingsAllowCopy;
			this.toolTip.SetToolTip(this.allowItemCopyCheckBox, Resources.SettingsAllowCopyTT);
			this.allowItemEditCheckBox.Text = Resources.SettingsAllowEdit;
			this.toolTip.SetToolTip(this.allowItemEditCheckBox, Resources.SettingsAllowEditTT);
			this.loadLastCharacterCheckBox.Text = Resources.SettingsLoadChar;
			this.toolTip.SetToolTip(this.loadLastCharacterCheckBox, Resources.SettingsLoadCharTT);
			this.loadLastVaultCheckBox.Text = Resources.SettingsLoadVault;
			this.toolTip.SetToolTip(this.loadLastVaultCheckBox, Resources.SettingsLoadVaultTT);
			this.loadAllFilesCheckBox.Text = Resources.SettingsPreLoad;
			this.toolTip.SetToolTip(this.loadAllFilesCheckBox, Resources.SettingsPreLoadTT);
			this.suppressWarningsCheckBox.Text = Resources.SettingsNoWarning;
			this.toolTip.SetToolTip(this.suppressWarningsCheckBox, Resources.SettingsNoWarningTT);
			this.playerReadonlyCheckbox.Text = Resources.SettingsPlayerReadonly;
			this.toolTip.SetToolTip(this.playerReadonlyCheckbox, Resources.SettingsPlayerReadonlyTT);
			this.resetButton.Text = Resources.SettingsReset;
			this.toolTip.SetToolTip(this.resetButton, Resources.SettingsResetTT);
			this.cancelButton.Text = Resources.GlobalCancel;
			this.okayButton.Text = Resources.GlobalOK;
			this.Text = Resources.SettingsTitle;

			this.DrawCustomBorder = true;

			this.mapListComboBox.Items.Clear();
			this.mapListComboBox.Items.Add(string.Empty);

			string[] maps = TQData.GetCustomMapList();

			if (maps != null && maps.Length > 0)
			{
				this.mapListComboBox.Items.AddRange(maps);
			}
			if (!IniProperties.ShowEditingCopyFeatures)
			{
				this.allowItemEditCheckBox.Visible = false;
				this.allowItemCopyCheckBox.Visible = false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the game path has been changed
		/// </summary>
		public bool GamePathChanged
		{
			get
			{
				return this.gamePathChanged;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the language setting has been changed
		/// </summary>
		public bool LanguageChanged
		{
			get
			{
				return this.languageChanged;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the UI setting was changed.
		/// </summary>
		public bool UISettingChanged
		{
			get
			{
				return this.uiSettingChanged;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the custom map selection has changed
		/// </summary>
		public bool CustomMapsChanged
		{
			get
			{
				return this.customMapsChanged;
			}
		}

		/// <summary>
		/// Gets a value indicating whether any configuration item has been changed
		/// </summary>
		public bool ConfigurationChanged
		{
			get
			{
				return this.configurationChanged;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the vault save path has been changed
		/// </summary>
		public bool VaultPathChanged
		{
			get
			{
				return this.vaultPathChanged;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the player list filter has been changed
		/// </summary>
		public bool PlayerFilterChanged
		{
			get
			{
				return this.playerFilterChanged;
			}
		}

		/// <summary>
		/// Gets the vault save path
		/// </summary>
		public string VaultPath
		{
			get
			{
				return this.vaultPath;
			}
		}

		/// <summary>
		/// Override of ScaleControl which supports font scaling.
		/// </summary>
		/// <param name="factor">SizeF for the scale factor</param>
		/// <param name="specified">BoundsSpecified value.</param>
		protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
		{
			this.Font = new Font(this.Font.FontFamily, this.Font.SizeInPoints * factor.Height, this.Font.Style);
			base.ScaleControl(factor, specified);
		}

		/// <summary>
		/// Handler for clicking the vault path browse button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void VaultPathBrowseButtonClick(object sender, EventArgs e)
		{
			this.folderBrowserDialog.Description = Resources.SettingsBrowseVault;
			this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
			this.folderBrowserDialog.SelectedPath = this.vaultPath;
			this.folderBrowserDialog.ShowDialog();
			if (this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant() != this.vaultPath.Trim().ToUpperInvariant())
			{
				this.vaultPath = this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant();
				this.vaultPathTextBox.Text = this.vaultPath.Trim().ToUpperInvariant();
				this.vaultPathTextBox.Invalidate();
				this.configurationChanged = true;
				this.vaultPathChanged = true;
			}
		}

		/// <summary>
		/// Handler for clicking the reset button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ResetButtonClick(object sender, EventArgs e)
		{
			if (this.configurationChanged)
			{
				this.LoadSettings();
				this.UpdateDialogSettings();
				this.Invalidate();
			}
		}

		/// <summary>
		/// Handler for clicking the skip title check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SkipTitleCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.skipTitleCheckBox.Checked)
			{
				if (this.skipTitle == false)
				{
					this.skipTitle = true;
					this.configurationChanged = true;
				}
			}
			else
			{
				if (this.skipTitle == true)
				{
					this.skipTitle = false;
					this.configurationChanged = true;
				}
			}
		}

		/// <summary>
		/// Handler for loading this dialog box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SettingsDialogLoad(object sender, EventArgs e)
		{
			this.settingsLoaded = false;

			this.LoadSettings();
			this.UpdateDialogSettings();
		}



		/// <summary>
		/// Loads the settings from the config file
		/// </summary>
		private void LoadSettings()
		{
			this.skipTitle = Settings.Default.SkipTitle;
			this.vaultPath = Settings.Default.VaultPath;
			this.allowItemCopy = Settings.Default.AllowItemCopy;
			this.allowItemEdit = Settings.Default.AllowItemEdit;
			this.loadLastCharacter = Settings.Default.LoadLastCharacter;
			this.loadLastVault = Settings.Default.LoadLastVault;
			this.detectLanguage = Settings.Default.AutoDetectLanguage;

			// Force English since there was some issue with getting the proper language setting.
			var gl = Database.DB.GameLanguage;
			if (gl == null)
			{
				this.titanQuestLanguage = "English";
			}
			else
			{
				this.titanQuestLanguage = gl;
			}

			this.detectGamePath = Settings.Default.AutoDetectGamePath;
			this.titanQuestPath = TQData.TQPath;
			this.immortalThronePath = TQData.ImmortalThronePath;
			this.enableMods = Settings.Default.ModEnabled;
			this.customMap = Settings.Default.CustomMap;
			this.loadAllFiles = Settings.Default.LoadAllFiles;
			this.suppressWarnings = Settings.Default.SuppressWarnings;
			this.playerReadonly = Settings.Default.PlayerReadonly;

			this.settingsLoaded = true;
			this.configurationChanged = false;
			this.vaultPathChanged = false;
			this.playerFilterChanged = false;
			this.languageChanged = false;
			this.gamePathChanged = false;
			this.uiSettingChanged = false;
		}

		/// <summary>
		/// Updates the dialog settings display
		/// </summary>
		private void UpdateDialogSettings()
		{
			// Check to see that we can update things
			if (!this.settingsLoaded)
			{
				return;
			}

			// Build language combo box
			this.languageComboBox.Items.Clear();

			// Read the languages from the config file
			ComboBoxItem[] languages = Settings.Default.GameLanguages.Split(',').Select(iso =>
			{
				CultureInfo ci = new CultureInfo(iso.ToUpperInvariant(), true);
				return new ComboBoxItem() { Value = ci.EnglishName, DisplayName = ci.DisplayName };// to keep EnglishName as baseline value
			}).OrderBy(cb => cb.DisplayName).ToArray();

			// Reading failed so we default to English
			if (!languages.Any()) languages = new ComboBoxItem[] { new ComboBoxItem() { Value = "English", DisplayName = "English" } };

			this.languageComboBox.Items.AddRange(languages);

			this.vaultPathTextBox.Text = this.vaultPath;
			this.skipTitleCheckBox.Checked = this.skipTitle;
			this.allowItemEditCheckBox.Checked = this.allowItemEdit;
			this.allowItemCopyCheckBox.Checked = this.allowItemCopy;
			this.loadLastCharacterCheckBox.Checked = this.loadLastCharacter;
			this.loadLastVaultCheckBox.Checked = this.loadLastVault;
			this.detectLanguageCheckBox.Checked = this.detectLanguage;
			this.detectGamePathsCheckBox.Checked = this.detectGamePath;
			this.immortalThronePathTextBox.Text = this.immortalThronePath;
			this.immortalThronePathTextBox.Enabled = !this.detectGamePath;
			this.immortalThronePathBrowseButton.Enabled = !this.detectGamePath;
			this.titanQuestPathTextBox.Text = this.titanQuestPath;
			this.titanQuestPathTextBox.Enabled = !this.detectGamePath;
			this.titanQuestPathBrowseButton.Enabled = !this.detectGamePath;
			this.loadAllFilesCheckBox.Checked = this.loadAllFiles;
			this.suppressWarningsCheckBox.Checked = this.suppressWarnings;
			this.playerReadonlyCheckbox.Checked = this.playerReadonly;

			this.enableCustomMapsCheckBox.Checked = this.enableMods;
			int ind = this.mapListComboBox.FindStringExact(this.customMap);
			if (ind != -1)
			{
				this.mapListComboBox.SelectedIndex = ind;
			}

			this.mapListComboBox.Enabled = this.enableMods;

			this.languageComboBox.SelectedItem = languages.FirstOrDefault(cb => cb.Value.Equals(this.titanQuestLanguage, StringComparison.InvariantCultureIgnoreCase));

			this.languageComboBox.Enabled = !this.detectLanguage;
		}

		/// <summary>
		/// Handler for clicking the OK button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void OkayButtonClick(object sender, EventArgs e)
		{
			if (this.configurationChanged)
			{
				Settings.Default.SkipTitle = this.skipTitle;
				Settings.Default.VaultPath = this.vaultPath;
				Settings.Default.AllowItemCopy = this.allowItemCopy;
				Settings.Default.AllowItemEdit = this.allowItemEdit;
				Settings.Default.LoadLastCharacter = this.loadLastCharacter;
				Settings.Default.LoadLastVault = this.loadLastVault;
				Settings.Default.AutoDetectLanguage = this.detectLanguage;
				Settings.Default.TQLanguage = this.titanQuestLanguage;
				Settings.Default.AutoDetectGamePath = this.detectGamePath;
				Settings.Default.TQITPath = this.immortalThronePath;
				Settings.Default.TQPath = this.titanQuestPath;
				Settings.Default.ModEnabled = this.enableMods;
				Settings.Default.CustomMap = this.customMap;
				Settings.Default.LoadAllFiles = this.loadAllFiles;
				Settings.Default.SuppressWarnings = this.suppressWarnings;
				Settings.Default.PlayerReadonly = this.playerReadonly;
			}
		}

		/// <summary>
		/// Handler for clicking the allow item editing check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void AllowItemEditCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.allowItemEditCheckBox.Checked)
			{
				if (this.allowItemEdit == false)
				{
					this.allowItemEdit = true;
					this.configurationChanged = true;
				}
			}
			else
			{
				if (this.allowItemEdit == true)
				{
					this.allowItemEdit = false;
					this.configurationChanged = true;
				}
			}
		}

		/// <summary>
		/// Handler for clicking the allow item copy check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void AllowItemCopyCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.allowItemCopyCheckBox.Checked)
			{
				if (this.allowItemCopy == false)
				{
					this.allowItemCopy = true;
					this.configurationChanged = true;
				}
			}
			else
			{
				if (this.allowItemCopy == true)
				{
					this.allowItemCopy = false;
					this.configurationChanged = true;
				}
			}
		}

		/// <summary>
		/// Handler for clicking the load last character check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void LoadLastCharacterCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.loadLastCharacterCheckBox.Checked)
			{
				if (this.loadLastCharacter == false)
				{
					this.loadLastCharacter = true;
					this.configurationChanged = true;
				}
			}
			else
			{
				if (this.loadLastCharacter == true)
				{
					this.loadLastCharacter = false;
					this.configurationChanged = true;
				}
			}
		}

		/// <summary>
		/// Handler for clicking the load last vault check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void LoadLastVaultCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.loadLastVaultCheckBox.Checked)
			{
				if (this.loadLastVault == false)
				{
					this.loadLastVault = true;
					this.configurationChanged = true;
				}
			}
			else
			{
				if (this.loadLastVault == true)
				{
					this.loadLastVault = false;
					this.configurationChanged = true;
				}
			}
		}

		/// <summary>
		/// Handler for clicking the detect language check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void DetectLanguageCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.detectLanguageCheckBox.Checked)
			{
				if (this.detectLanguage == false)
				{
					this.detectLanguage = true;
					this.languageComboBox.Enabled = false;
					this.configurationChanged = true;

					// Force TQVault to restart to autodetect the language
					this.languageChanged = true;
				}
			}
			else
			{
				if (this.detectLanguage == true)
				{
					this.detectLanguage = false;
					this.languageComboBox.Enabled = true;
					this.configurationChanged = true;
				}
			}
		}

		/// <summary>
		/// Handler for clicking the detect game paths check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void DetectGamePathsCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.detectGamePathsCheckBox.Checked)
			{
				if (this.detectGamePath == false)
				{
					this.detectGamePath = true;
					this.immortalThronePathTextBox.Enabled = false;
					this.titanQuestPathTextBox.Enabled = false;
					this.titanQuestPathBrowseButton.Enabled = false;
					this.immortalThronePathBrowseButton.Enabled = false;
					this.configurationChanged = true;

					// Force TQVault to restart to autodetect the game path
					this.gamePathChanged = true;
				}
			}
			else
			{
				if (this.detectGamePath == true)
				{
					this.detectGamePath = false;
					this.immortalThronePathTextBox.Enabled = true;
					this.titanQuestPathTextBox.Enabled = true;
					this.titanQuestPathBrowseButton.Enabled = true;
					this.immortalThronePathBrowseButton.Enabled = true;
					this.configurationChanged = true;
				}
			}
		}

		/// <summary>
		/// Handler for changing the language selection
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void LanguageComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			// There was some problem getting the game language so we ignore changing it.
			var gl = Database.DB.GameLanguage;
			if (gl == null)
			{
				return;
			}

			this.titanQuestLanguage = ((ComboBoxItem)this.languageComboBox.SelectedItem).Value;
			if (this.titanQuestLanguage.ToUpperInvariant() != gl.ToUpperInvariant())
			{
				this.languageChanged = true;
			}

			this.configurationChanged = true;
		}

		/// <summary>
		/// Handler for leaving the vault text box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void VaultPathTextBoxLeave(object sender, EventArgs e)
		{
			if (this.vaultPath.ToUpperInvariant() != this.vaultPathTextBox.Text.Trim().ToUpperInvariant())
			{
				this.vaultPath = this.vaultPathTextBox.Text.Trim();
				this.vaultPathTextBox.Invalidate();
				this.configurationChanged = true;
				this.vaultPathChanged = true;
			}
		}

		/// <summary>
		/// Handler for leaving the Titan Quest game path text box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void TitanQuestPathTextBoxLeave(object sender, EventArgs e)
		{
			if (this.titanQuestPath.ToUpperInvariant() != this.titanQuestPathTextBox.Text.Trim().ToUpperInvariant())
			{
				this.titanQuestPath = this.titanQuestPathTextBox.Text.Trim();
				this.titanQuestPathTextBox.Invalidate();
				this.configurationChanged = true;
				this.gamePathChanged = true;
			}
		}

		/// <summary>
		/// Handler for leaving the Immortal Throne game path text box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ImmortalThronePathTextBoxLeave(object sender, EventArgs e)
		{
			if (this.immortalThronePath.ToUpperInvariant() != this.immortalThronePathTextBox.Text.Trim().ToUpperInvariant())
			{
				this.immortalThronePath = this.immortalThronePathTextBox.Text.Trim();
				this.immortalThronePathTextBox.Invalidate();
				this.configurationChanged = true;
				this.gamePathChanged = true;
			}
		}

		/// <summary>
		/// Handler for clicking the Titan Quest game path browse button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void TitanQuestPathBrowseButtonClick(object sender, EventArgs e)
		{
			this.folderBrowserDialog.Description = Resources.SettingsBrowseTQ;
			this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
			this.folderBrowserDialog.SelectedPath = this.titanQuestPath;
			this.folderBrowserDialog.ShowDialog();
			if (this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant() != this.titanQuestPath.Trim().ToUpperInvariant())
			{
				this.titanQuestPath = this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant();
				this.titanQuestPathTextBox.Text = this.titanQuestPath.Trim().ToUpperInvariant();
				this.titanQuestPathTextBox.Invalidate();
				this.configurationChanged = true;
				this.gamePathChanged = true;
			}
		}

		/// <summary>
		/// Handler for clicking the Immortal Throne game path browse button
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void ImmortalThronePathBrowseButtonClick(object sender, EventArgs e)
		{
			this.folderBrowserDialog.Description = Resources.SettingsBrowseIT;
			this.folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
			this.folderBrowserDialog.SelectedPath = this.immortalThronePath;
			this.folderBrowserDialog.ShowDialog();
			if (this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant() != this.immortalThronePath.Trim().ToUpperInvariant())
			{
				this.immortalThronePath = this.folderBrowserDialog.SelectedPath.Trim().ToUpperInvariant();
				this.immortalThronePathTextBox.Text = this.immortalThronePath.Trim().ToUpperInvariant();
				this.immortalThronePathTextBox.Invalidate();
				this.configurationChanged = true;
				this.gamePathChanged = true;
			}
		}

		/// <summary>
		/// Handler for clicking enable custom maps
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void EnableCustomMapsCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.enableCustomMapsCheckBox.Checked)
			{
				if (this.enableMods == false)
				{
					this.enableMods = true;
					this.configurationChanged = true;
					this.customMapsChanged = true;
					this.mapListComboBox.Enabled = true;
				}
			}
			else
			{
				if (this.enableMods == true)
				{
					this.enableMods = false;
					this.configurationChanged = true;
					this.customMapsChanged = true;
					this.mapListComboBox.Enabled = false;
				}
			}
		}

		/// <summary>
		/// Handler for changing the selected custom map
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void MapListComboBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!this.settingsLoaded)
			{
				return;
			}

			if (this.mapListComboBox.SelectedItem.ToString() != Settings.Default.CustomMap)
			{
				this.customMap = this.mapListComboBox.SelectedItem.ToString();
				this.configurationChanged = true;
				this.customMapsChanged = true;
			}
		}

		/// <summary>
		/// Handler for clicking the load all files check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void LoadAllFilesCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.loadAllFilesCheckBox.Checked)
			{
				if (this.loadAllFiles == false)
				{
					this.loadAllFiles = true;
					this.configurationChanged = true;
				}
			}
			else
			{
				if (this.loadAllFiles == true)
				{
					this.loadAllFiles = false;
					this.configurationChanged = true;
				}
			}
		}

		/// <summary>
		/// Handler for clicking the suppress warnings check box
		/// </summary>
		/// <param name="sender">sender object</param>
		/// <param name="e">EventArgs data</param>
		private void SuppressWarningsCheckBoxCheckedChanged(object sender, EventArgs e)
		{
			if (this.suppressWarningsCheckBox.Checked)
			{
				if (this.suppressWarnings == false)
				{
					this.suppressWarnings = true;
					this.configurationChanged = true;
				}
			}
			else
			{
				if (this.suppressWarnings == true)
				{
					this.suppressWarnings = false;
					this.configurationChanged = true;
				}
			}
		}

		private void PlayerReadonlyCheckboxCheckedChanged(object sender, EventArgs e)
		{
			if (this.playerReadonlyCheckbox.Checked)
			{
				if (this.playerReadonly == false)
				{
					this.playerReadonly = true;
					this.configurationChanged = true;
				}
			}
			else
			{
				if (this.playerReadonly == true)
				{
					this.playerReadonly = false;
					this.configurationChanged = true;
				}
			}

		}
	}
}