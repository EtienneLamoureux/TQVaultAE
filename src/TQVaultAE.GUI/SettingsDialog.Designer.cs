//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Properties;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// Form Designer Class for SettingsDialog
	/// </summary>
	internal partial class SettingsDialog
	{
		/// <summary>
		/// Check box to allow item editing
		/// </summary>
		private ScalingCheckBox allowItemEditCheckBox;

		/// <summary>
		/// Check box to allow item copying
		/// </summary>
		private ScalingCheckBox allowItemCopyCheckBox;

		/// <summary>
		/// Check box for skipping the splash screen
		/// </summary>
		private ScalingCheckBox skipTitleCheckBox;

		/// <summary>
		/// Check box to auto load the last opened character when launched
		/// </summary>
		private ScalingCheckBox loadLastCharacterCheckBox;

		/// <summary>
		/// Check box to auto load the last opened vault when launched
		/// </summary>
		private ScalingCheckBox loadLastVaultCheckBox;

		/// <summary>
		/// Text box for entering the vault save path
		/// </summary>
		private ScalingTextBox vaultPathTextBox;

		/// <summary>
		/// Label for the vault path text box
		/// </summary>
		private ScalingLabel vaultPathLabel;

		/// <summary>
		/// Cancel button control
		/// </summary>
		private ScalingButton cancelButton;

		/// <summary>
		/// OK button control
		/// </summary>
		private ScalingButton okayButton;

		/// <summary>
		/// Browser Dialog Box
		/// </summary>
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;

		/// <summary>
		/// Reset settings button
		/// </summary>
		private ScalingButton resetButton;

		/// <summary>
		/// Browse button for the vault path
		/// </summary>
		private ScalingButton vaultPathBrowseButton;

		/// <summary>
		/// Tool Tip instance
		/// </summary>
		private System.Windows.Forms.ToolTip toolTip;

		/// <summary>
		/// Game and TQVault language combo box
		/// </summary>
		private ScalingComboBox languageComboBox;

		/// <summary>
		/// Label for game lanuguage combo box
		/// </summary>
		private ScalingLabel languageLabel;

		/// <summary>
		/// Check box to auto detect the language
		/// </summary>
		private ScalingCheckBox detectLanguageCheckBox;

		/// <summary>
		/// Text box for entering the Titan Quest game path
		/// </summary>
		private ScalingTextBox titanQuestPathTextBox;

		/// <summary>
		/// Label for Titan Quest game path text box
		/// </summary>
		private ScalingLabel titanQuestPathLabel;

		/// <summary>
		/// Label for Immortal Throne game path text box
		/// </summary>
		private ScalingLabel immortalThronePathLabel;

		/// <summary>
		/// Text box for entering the Immortal Throne game path
		/// </summary>
		private ScalingTextBox immortalThronePathTextBox;

		/// <summary>
		/// Check box for auto detecting the game paths
		/// </summary>
		private ScalingCheckBox detectGamePathsCheckBox;

		/// <summary>
		/// Browse button for Titan Quest game path
		/// </summary>
		private ScalingButton titanQuestPathBrowseButton;

		/// <summary>
		/// Browse button for Immortal Throne game path
		/// </summary>
		private ScalingButton immortalThronePathBrowseButton;

		/// <summary>
		/// Check box to enable custom map support
		/// </summary>
		private ScalingCheckBox enableCustomMapsCheckBox;

		/// <summary>
		/// Label for map list combo box
		/// </summary>
		private ScalingLabel customMapLabel;

		/// <summary>
		/// Combo box for listing the available custom maps
		/// </summary>
		private ScalingComboBox mapListComboBox;

		/// <summary>
		/// Check box to load all available data files on startup
		/// </summary>
		private ScalingCheckBox loadAllFilesCheckBox;

		/// <summary>
		/// Check box to suppress warning messages
		/// </summary>
		private ScalingCheckBox suppressWarningsCheckBox;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (this.components != null))
			{
				this.components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
            this.allowItemEditCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.allowItemCopyCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.skipTitleCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.loadLastCharacterCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.loadLastVaultCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.vaultPathTextBox = new TQVaultAE.GUI.ScalingTextBox();
            this.vaultPathLabel = new TQVaultAE.GUI.ScalingLabel();
            this.cancelButton = new TQVaultAE.GUI.ScalingButton();
            this.okayButton = new TQVaultAE.GUI.ScalingButton();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.resetButton = new TQVaultAE.GUI.ScalingButton();
            this.vaultPathBrowseButton = new TQVaultAE.GUI.ScalingButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.enableCustomMapsCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.loadAllFilesCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.suppressWarningsCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.playerReadonlyCheckbox = new TQVaultAE.GUI.ScalingCheckBox();
            this.languageComboBox = new TQVaultAE.GUI.ScalingComboBox();
            this.languageLabel = new TQVaultAE.GUI.ScalingLabel();
            this.detectLanguageCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.titanQuestPathTextBox = new TQVaultAE.GUI.ScalingTextBox();
            this.titanQuestPathLabel = new TQVaultAE.GUI.ScalingLabel();
            this.immortalThronePathLabel = new TQVaultAE.GUI.ScalingLabel();
            this.immortalThronePathTextBox = new TQVaultAE.GUI.ScalingTextBox();
            this.detectGamePathsCheckBox = new TQVaultAE.GUI.ScalingCheckBox();
            this.titanQuestPathBrowseButton = new TQVaultAE.GUI.ScalingButton();
            this.immortalThronePathBrowseButton = new TQVaultAE.GUI.ScalingButton();
            this.customMapLabel = new TQVaultAE.GUI.ScalingLabel();
            this.mapListComboBox = new TQVaultAE.GUI.ScalingComboBox();
            this.SuspendLayout();
            // 
            // allowItemEditCheckBox
            // 
            this.allowItemEditCheckBox.AutoSize = true;
            this.allowItemEditCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.allowItemEditCheckBox.Location = new System.Drawing.Point(498, 222);
            this.allowItemEditCheckBox.Name = "allowItemEditCheckBox";
            this.allowItemEditCheckBox.Size = new System.Drawing.Size(202, 21);
            this.allowItemEditCheckBox.TabIndex = 3;
            this.allowItemEditCheckBox.Text = "Allow Item Editing Features";
            this.toolTip.SetToolTip(this.allowItemEditCheckBox, "Turns on the editing features in the context menu.\r\nThese include item creation a" +
        "nd modification.");
            this.allowItemEditCheckBox.UseVisualStyleBackColor = true;
            this.allowItemEditCheckBox.CheckedChanged += new System.EventHandler(this.AllowItemEditCheckBoxCheckedChanged);
            // 
            // allowItemCopyCheckBox
            // 
            this.allowItemCopyCheckBox.AutoSize = true;
            this.allowItemCopyCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.allowItemCopyCheckBox.Location = new System.Drawing.Point(498, 250);
            this.allowItemCopyCheckBox.Name = "allowItemCopyCheckBox";
            this.allowItemCopyCheckBox.Size = new System.Drawing.Size(154, 21);
            this.allowItemCopyCheckBox.TabIndex = 4;
            this.allowItemCopyCheckBox.Text = "Allow Item Copying";
            this.toolTip.SetToolTip(this.allowItemCopyCheckBox, "Enables copy selection in the context menu.");
            this.allowItemCopyCheckBox.UseVisualStyleBackColor = true;
            this.allowItemCopyCheckBox.CheckedChanged += new System.EventHandler(this.AllowItemCopyCheckBoxCheckedChanged);
            // 
            // skipTitleCheckBox
            // 
            this.skipTitleCheckBox.AutoSize = true;
            this.skipTitleCheckBox.Checked = true;
            this.skipTitleCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.skipTitleCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.skipTitleCheckBox.Location = new System.Drawing.Point(498, 43);
            this.skipTitleCheckBox.Name = "skipTitleCheckBox";
            this.skipTitleCheckBox.Size = new System.Drawing.Size(239, 21);
            this.skipTitleCheckBox.TabIndex = 2;
            this.skipTitleCheckBox.Text = "Automatically Bypass Title Screen";
            this.toolTip.SetToolTip(this.skipTitleCheckBox, "Ticking this box will automatically hit\r\nthe Enter key on the title screen.");
            this.skipTitleCheckBox.UseVisualStyleBackColor = true;
            this.skipTitleCheckBox.CheckedChanged += new System.EventHandler(this.SkipTitleCheckBoxCheckedChanged);
            // 
            // loadLastCharacterCheckBox
            // 
            this.loadLastCharacterCheckBox.AutoSize = true;
            this.loadLastCharacterCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.loadLastCharacterCheckBox.Location = new System.Drawing.Point(498, 90);
            this.loadLastCharacterCheckBox.Name = "loadLastCharacterCheckBox";
            this.loadLastCharacterCheckBox.Size = new System.Drawing.Size(314, 21);
            this.loadLastCharacterCheckBox.TabIndex = 5;
            this.loadLastCharacterCheckBox.Text = "Automatically Load the last opened Character";
            this.toolTip.SetToolTip(this.loadLastCharacterCheckBox, "Selecting this option will automatically load\r\nthe last open character when TQVau" +
        "lt was closed.");
            this.loadLastCharacterCheckBox.UseVisualStyleBackColor = true;
            this.loadLastCharacterCheckBox.CheckedChanged += new System.EventHandler(this.LoadLastCharacterCheckBoxCheckedChanged);
            // 
            // loadLastVaultCheckBox
            // 
            this.loadLastVaultCheckBox.AutoSize = true;
            this.loadLastVaultCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.loadLastVaultCheckBox.Location = new System.Drawing.Point(498, 118);
            this.loadLastVaultCheckBox.Name = "loadLastVaultCheckBox";
            this.loadLastVaultCheckBox.Size = new System.Drawing.Size(286, 21);
            this.loadLastVaultCheckBox.TabIndex = 6;
            this.loadLastVaultCheckBox.Text = "Automatically Load the last opened Vault";
            this.toolTip.SetToolTip(this.loadLastVaultCheckBox, "Selecting this item will automatically load the\r\nlast opened vault when TQVault w" +
        "as closed.\r\nTQVault will automatically open Main Vault\r\nif nothing is chosen.");
            this.loadLastVaultCheckBox.UseVisualStyleBackColor = true;
            this.loadLastVaultCheckBox.CheckedChanged += new System.EventHandler(this.LoadLastVaultCheckBoxCheckedChanged);
            // 
            // vaultPathTextBox
            // 
            this.vaultPathTextBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.vaultPathTextBox.Location = new System.Drawing.Point(12, 46);
            this.vaultPathTextBox.Name = "vaultPathTextBox";
            this.vaultPathTextBox.Size = new System.Drawing.Size(397, 25);
            this.vaultPathTextBox.TabIndex = 0;
            this.toolTip.SetToolTip(this.vaultPathTextBox, resources.GetString("vaultPathTextBox.ToolTip"));
            this.vaultPathTextBox.Leave += new System.EventHandler(this.VaultPathTextBoxLeave);
            // 
            // vaultPathLabel
            // 
            this.vaultPathLabel.AutoSize = true;
            this.vaultPathLabel.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.vaultPathLabel.Location = new System.Drawing.Point(12, 24);
            this.vaultPathLabel.Name = "vaultPathLabel";
            this.vaultPathLabel.Size = new System.Drawing.Size(72, 17);
            this.vaultPathLabel.TabIndex = 14;
            this.vaultPathLabel.Text = "Vault Path";
            // 
            // cancelButton
            // 
            this.cancelButton.BackColor = System.Drawing.Color.Transparent;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.DownBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonDown;
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.Image = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.cancelButton.Location = new System.Drawing.Point(481, 419);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.OverBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonOver;
            this.cancelButton.Size = new System.Drawing.Size(137, 30);
            this.cancelButton.SizeToGraphic = false;
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UpBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.cancelButton.UseCustomGraphic = true;
            this.cancelButton.UseVisualStyleBackColor = false;
            // 
            // okayButton
            // 
            this.okayButton.BackColor = System.Drawing.Color.Transparent;
            this.okayButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okayButton.DownBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonDown;
            this.okayButton.FlatAppearance.BorderSize = 0;
            this.okayButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.okayButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.okayButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.Image = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.okayButton.Location = new System.Drawing.Point(326, 419);
            this.okayButton.Name = "okayButton";
            this.okayButton.OverBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonOver;
            this.okayButton.Size = new System.Drawing.Size(137, 30);
            this.okayButton.SizeToGraphic = false;
            this.okayButton.TabIndex = 12;
            this.okayButton.Text = "OK";
            this.okayButton.UpBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.okayButton.UseCustomGraphic = true;
            this.okayButton.UseVisualStyleBackColor = false;
            this.okayButton.Click += new System.EventHandler(this.OkayButtonClick);
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyDocuments;
            // 
            // resetButton
            // 
            this.resetButton.BackColor = System.Drawing.Color.Transparent;
            this.resetButton.DownBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonDown;
            this.resetButton.FlatAppearance.BorderSize = 0;
            this.resetButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.resetButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.resetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.resetButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.resetButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.resetButton.Image = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.resetButton.Location = new System.Drawing.Point(763, 419);
            this.resetButton.Name = "resetButton";
            this.resetButton.OverBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonOver;
            this.resetButton.Size = new System.Drawing.Size(137, 30);
            this.resetButton.SizeToGraphic = false;
            this.resetButton.TabIndex = 11;
            this.resetButton.Text = "Reset";
            this.toolTip.SetToolTip(this.resetButton, "Causes the configuration to Reset to the\r\nlast saved configuration.");
            this.resetButton.UpBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.resetButton.UseCustomGraphic = true;
            this.resetButton.UseVisualStyleBackColor = false;
            this.resetButton.Click += new System.EventHandler(this.ResetButtonClick);
            // 
            // vaultPathBrowseButton
            // 
            this.vaultPathBrowseButton.BackColor = System.Drawing.Color.Transparent;
            this.vaultPathBrowseButton.DownBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonDown;
            this.vaultPathBrowseButton.FlatAppearance.BorderSize = 0;
            this.vaultPathBrowseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.vaultPathBrowseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.vaultPathBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.vaultPathBrowseButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.vaultPathBrowseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.vaultPathBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("vaultPathBrowseButton.Image")));
            this.vaultPathBrowseButton.Location = new System.Drawing.Point(415, 43);
            this.vaultPathBrowseButton.Name = "vaultPathBrowseButton";
            this.vaultPathBrowseButton.OverBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonOver;
            this.vaultPathBrowseButton.Size = new System.Drawing.Size(47, 30);
            this.vaultPathBrowseButton.SizeToGraphic = false;
            this.vaultPathBrowseButton.TabIndex = 1;
            this.vaultPathBrowseButton.Text = "...";
            this.vaultPathBrowseButton.UpBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.vaultPathBrowseButton.UseCustomGraphic = true;
            this.vaultPathBrowseButton.UseVisualStyleBackColor = false;
            this.vaultPathBrowseButton.Click += new System.EventHandler(this.VaultPathBrowseButtonClick);
            // 
            // enableCustomMapsCheckBox
            // 
            this.enableCustomMapsCheckBox.AutoSize = true;
            this.enableCustomMapsCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.enableCustomMapsCheckBox.Location = new System.Drawing.Point(12, 354);
            this.enableCustomMapsCheckBox.Name = "enableCustomMapsCheckBox";
            this.enableCustomMapsCheckBox.Size = new System.Drawing.Size(160, 21);
            this.enableCustomMapsCheckBox.TabIndex = 25;
            this.enableCustomMapsCheckBox.Text = "Enable Custom Maps";
            this.toolTip.SetToolTip(this.enableCustomMapsCheckBox, "Selecting this item will \r\nenable the dropdown\r\nto select custom maps.");
            this.enableCustomMapsCheckBox.UseVisualStyleBackColor = true;
            this.enableCustomMapsCheckBox.CheckedChanged += new System.EventHandler(this.EnableCustomMapsCheckBoxCheckedChanged);
            // 
            // loadAllFilesCheckBox
            // 
            this.loadAllFilesCheckBox.AutoSize = true;
            this.loadAllFilesCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.loadAllFilesCheckBox.Location = new System.Drawing.Point(498, 146);
            this.loadAllFilesCheckBox.Name = "loadAllFilesCheckBox";
            this.loadAllFilesCheckBox.Size = new System.Drawing.Size(273, 21);
            this.loadAllFilesCheckBox.TabIndex = 28;
            this.loadAllFilesCheckBox.Text = "Pre-Load All Vault And Character Files";
            this.toolTip.SetToolTip(this.loadAllFilesCheckBox, "Selecting this item will automatically load all\r\nof the available character, stas" +
        "h and vault files\r\non startup.  This aids the search function, but\r\nincreases st" +
        "artup time.");
            this.loadAllFilesCheckBox.UseVisualStyleBackColor = true;
            this.loadAllFilesCheckBox.CheckedChanged += new System.EventHandler(this.LoadAllFilesCheckBoxCheckedChanged);
            // 
            // suppressWarningsCheckBox
            // 
            this.suppressWarningsCheckBox.AutoSize = true;
            this.suppressWarningsCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.suppressWarningsCheckBox.Location = new System.Drawing.Point(498, 300);
            this.suppressWarningsCheckBox.Name = "suppressWarningsCheckBox";
            this.suppressWarningsCheckBox.Size = new System.Drawing.Size(221, 21);
            this.suppressWarningsCheckBox.TabIndex = 30;
            this.suppressWarningsCheckBox.Text = "Bypass Confirmation Messages";
            this.toolTip.SetToolTip(this.suppressWarningsCheckBox, "When enabled, confirmation messages will no\r\nlonger be shown for item deletion an" +
        "d\r\nrelic removal or if there are items in the trash\r\nwhen TQVault is closed.");
            this.suppressWarningsCheckBox.UseVisualStyleBackColor = true;
            this.suppressWarningsCheckBox.CheckedChanged += new System.EventHandler(this.SuppressWarningsCheckBoxCheckedChanged);
            // 
            // playerReadonlyCheckbox
            // 
            this.playerReadonlyCheckbox.AutoSize = true;
            this.playerReadonlyCheckbox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.playerReadonlyCheckbox.Location = new System.Drawing.Point(498, 328);
            this.playerReadonlyCheckbox.Name = "playerReadonlyCheckbox";
            this.playerReadonlyCheckbox.Size = new System.Drawing.Size(203, 21);
            this.playerReadonlyCheckbox.TabIndex = 33;
            this.playerReadonlyCheckbox.Text = "Player Equipment ReadOnly";
            this.toolTip.SetToolTip(this.playerReadonlyCheckbox, "Avoid save game corruption that occurs (randomly). When enabled, player equipment" +
        " will be read-only,  you won\'t be able to select or move any item.");
            this.playerReadonlyCheckbox.UseVisualStyleBackColor = true;
            this.playerReadonlyCheckbox.CheckedChanged += new System.EventHandler(this.PlayerReadonlyCheckboxCheckedChanged);
			// 
            // languageComboBox
            // 
            this.languageComboBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Location = new System.Drawing.Point(12, 247);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(397, 25);
            this.languageComboBox.TabIndex = 15;
            this.languageComboBox.SelectedIndexChanged += new System.EventHandler(this.LanguageComboBoxSelectedIndexChanged);
            // 
            // languageLabel
            // 
            this.languageLabel.AutoSize = true;
            this.languageLabel.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.languageLabel.Location = new System.Drawing.Point(12, 225);
            this.languageLabel.Name = "languageLabel";
            this.languageLabel.Size = new System.Drawing.Size(108, 17);
            this.languageLabel.TabIndex = 16;
            this.languageLabel.Text = "Game Language";
            // 
            // detectLanguageCheckBox
            // 
            this.detectLanguageCheckBox.AutoSize = true;
            this.detectLanguageCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.detectLanguageCheckBox.Location = new System.Drawing.Point(12, 275);
            this.detectLanguageCheckBox.Name = "detectLanguageCheckBox";
            this.detectLanguageCheckBox.Size = new System.Drawing.Size(162, 21);
            this.detectLanguageCheckBox.TabIndex = 17;
            this.detectLanguageCheckBox.Text = "Autodetect Language";
            this.detectLanguageCheckBox.UseVisualStyleBackColor = true;
            this.detectLanguageCheckBox.CheckedChanged += new System.EventHandler(this.DetectLanguageCheckBoxCheckedChanged);
            // 
            // titanQuestPathTextBox
            // 
            this.titanQuestPathTextBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.titanQuestPathTextBox.Location = new System.Drawing.Point(12, 108);
            this.titanQuestPathTextBox.Name = "titanQuestPathTextBox";
            this.titanQuestPathTextBox.Size = new System.Drawing.Size(397, 25);
            this.titanQuestPathTextBox.TabIndex = 18;
            this.titanQuestPathTextBox.Leave += new System.EventHandler(this.TitanQuestPathTextBoxLeave);
            // 
            // titanQuestPathLabel
            // 
            this.titanQuestPathLabel.AutoSize = true;
            this.titanQuestPathLabel.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.titanQuestPathLabel.Location = new System.Drawing.Point(12, 86);
            this.titanQuestPathLabel.Name = "titanQuestPathLabel";
            this.titanQuestPathLabel.Size = new System.Drawing.Size(103, 17);
            this.titanQuestPathLabel.TabIndex = 19;
            this.titanQuestPathLabel.Text = "TQ Game Path";
            // 
            // immortalThronePathLabel
            // 
            this.immortalThronePathLabel.AutoSize = true;
            this.immortalThronePathLabel.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.immortalThronePathLabel.Location = new System.Drawing.Point(12, 137);
            this.immortalThronePathLabel.Name = "immortalThronePathLabel";
            this.immortalThronePathLabel.Size = new System.Drawing.Size(94, 17);
            this.immortalThronePathLabel.TabIndex = 20;
            this.immortalThronePathLabel.Text = "IT Game Path";
            // 
            // immortalThronePathTextBox
            // 
            this.immortalThronePathTextBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.immortalThronePathTextBox.Location = new System.Drawing.Point(12, 159);
            this.immortalThronePathTextBox.Name = "immortalThronePathTextBox";
            this.immortalThronePathTextBox.Size = new System.Drawing.Size(397, 25);
            this.immortalThronePathTextBox.TabIndex = 21;
            this.immortalThronePathTextBox.Leave += new System.EventHandler(this.ImmortalThronePathTextBoxLeave);
            // 
            // detectGamePathsCheckBox
            // 
            this.detectGamePathsCheckBox.AutoSize = true;
            this.detectGamePathsCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.detectGamePathsCheckBox.Location = new System.Drawing.Point(12, 187);
            this.detectGamePathsCheckBox.Name = "detectGamePathsCheckBox";
            this.detectGamePathsCheckBox.Size = new System.Drawing.Size(178, 21);
            this.detectGamePathsCheckBox.TabIndex = 22;
            this.detectGamePathsCheckBox.Text = "Autodetect Game Paths";
            this.detectGamePathsCheckBox.UseVisualStyleBackColor = true;
            this.detectGamePathsCheckBox.CheckedChanged += new System.EventHandler(this.DetectGamePathsCheckBoxCheckedChanged);
            // 
            // titanQuestPathBrowseButton
            // 
            this.titanQuestPathBrowseButton.BackColor = System.Drawing.Color.Transparent;
            this.titanQuestPathBrowseButton.DownBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonDown;
            this.titanQuestPathBrowseButton.FlatAppearance.BorderSize = 0;
            this.titanQuestPathBrowseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.titanQuestPathBrowseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.titanQuestPathBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.titanQuestPathBrowseButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.titanQuestPathBrowseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.titanQuestPathBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("titanQuestPathBrowseButton.Image")));
            this.titanQuestPathBrowseButton.Location = new System.Drawing.Point(415, 102);
            this.titanQuestPathBrowseButton.Name = "titanQuestPathBrowseButton";
            this.titanQuestPathBrowseButton.OverBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonOver;
            this.titanQuestPathBrowseButton.Size = new System.Drawing.Size(47, 30);
            this.titanQuestPathBrowseButton.SizeToGraphic = false;
            this.titanQuestPathBrowseButton.TabIndex = 23;
            this.titanQuestPathBrowseButton.Text = "...";
            this.titanQuestPathBrowseButton.UpBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.titanQuestPathBrowseButton.UseCustomGraphic = true;
            this.titanQuestPathBrowseButton.UseVisualStyleBackColor = false;
            this.titanQuestPathBrowseButton.Click += new System.EventHandler(this.TitanQuestPathBrowseButtonClick);
            // 
            // immortalThronePathBrowseButton
            // 
            this.immortalThronePathBrowseButton.BackColor = System.Drawing.Color.Transparent;
            this.immortalThronePathBrowseButton.DownBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonDown;
            this.immortalThronePathBrowseButton.FlatAppearance.BorderSize = 0;
            this.immortalThronePathBrowseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.immortalThronePathBrowseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.immortalThronePathBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.immortalThronePathBrowseButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.immortalThronePathBrowseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.immortalThronePathBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("immortalThronePathBrowseButton.Image")));
            this.immortalThronePathBrowseButton.Location = new System.Drawing.Point(415, 153);
            this.immortalThronePathBrowseButton.Name = "immortalThronePathBrowseButton";
            this.immortalThronePathBrowseButton.OverBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonOver;
            this.immortalThronePathBrowseButton.Size = new System.Drawing.Size(47, 30);
            this.immortalThronePathBrowseButton.SizeToGraphic = false;
            this.immortalThronePathBrowseButton.TabIndex = 24;
            this.immortalThronePathBrowseButton.Text = "...";
            this.immortalThronePathBrowseButton.UpBitmap = global::TQVaultAE.GUI.Properties.Resources.MainButtonUp;
            this.immortalThronePathBrowseButton.UseCustomGraphic = true;
            this.immortalThronePathBrowseButton.UseVisualStyleBackColor = false;
            this.immortalThronePathBrowseButton.Click += new System.EventHandler(this.ImmortalThronePathBrowseButtonClick);
            // 
            // customMapLabel
            // 
            this.customMapLabel.AutoSize = true;
            this.customMapLabel.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.customMapLabel.Location = new System.Drawing.Point(12, 304);
            this.customMapLabel.Name = "customMapLabel";
            this.customMapLabel.Size = new System.Drawing.Size(90, 17);
            this.customMapLabel.TabIndex = 27;
            this.customMapLabel.Text = "Custom Map";
            // 
            // mapListComboBox
            // 
            this.mapListComboBox.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.mapListComboBox.FormattingEnabled = true;
            this.mapListComboBox.Location = new System.Drawing.Point(12, 326);
            this.mapListComboBox.Name = "mapListComboBox";
            this.mapListComboBox.Size = new System.Drawing.Size(397, 25);
            this.mapListComboBox.TabIndex = 26;
            this.mapListComboBox.SelectedIndexChanged += new System.EventHandler(this.MapListComboBoxSelectedIndexChanged);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(922, 461);
            this.Controls.Add(this.playerReadonlyCheckbox);
            this.Controls.Add(this.titanQuestPathTextBox);
            this.Controls.Add(this.titanQuestPathLabel);
            this.Controls.Add(this.detectGamePathsCheckBox);
            this.Controls.Add(this.suppressWarningsCheckBox);
            this.Controls.Add(this.titanQuestPathBrowseButton);
            this.Controls.Add(this.customMapLabel);
            this.Controls.Add(this.mapListComboBox);
            this.Controls.Add(this.enableCustomMapsCheckBox);
            this.Controls.Add(this.detectLanguageCheckBox);
            this.Controls.Add(this.loadAllFilesCheckBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.immortalThronePathBrowseButton);
            this.Controls.Add(this.languageLabel);
            this.Controls.Add(this.allowItemEditCheckBox);
            this.Controls.Add(this.allowItemCopyCheckBox);
            this.Controls.Add(this.skipTitleCheckBox);
            this.Controls.Add(this.immortalThronePathTextBox);
            this.Controls.Add(this.immortalThronePathLabel);
            this.Controls.Add(this.vaultPathLabel);
            this.Controls.Add(this.okayButton);
            this.Controls.Add(this.languageComboBox);
            this.Controls.Add(this.vaultPathBrowseButton);
            this.Controls.Add(this.vaultPathTextBox);
            this.Controls.Add(this.loadLastVaultCheckBox);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.loadLastCharacterCheckBox);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Albertus MT Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Settings";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SettingsDialogLoad);
            this.Controls.SetChildIndex(this.loadLastCharacterCheckBox, 0);
            this.Controls.SetChildIndex(this.resetButton, 0);
            this.Controls.SetChildIndex(this.loadLastVaultCheckBox, 0);
            this.Controls.SetChildIndex(this.vaultPathTextBox, 0);
            this.Controls.SetChildIndex(this.vaultPathBrowseButton, 0);
            this.Controls.SetChildIndex(this.languageComboBox, 0);
            this.Controls.SetChildIndex(this.okayButton, 0);
            this.Controls.SetChildIndex(this.vaultPathLabel, 0);
            this.Controls.SetChildIndex(this.immortalThronePathLabel, 0);
            this.Controls.SetChildIndex(this.immortalThronePathTextBox, 0);
            this.Controls.SetChildIndex(this.skipTitleCheckBox, 0);
            this.Controls.SetChildIndex(this.allowItemCopyCheckBox, 0);
            this.Controls.SetChildIndex(this.allowItemEditCheckBox, 0);
            this.Controls.SetChildIndex(this.languageLabel, 0);
            this.Controls.SetChildIndex(this.immortalThronePathBrowseButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.loadAllFilesCheckBox, 0);
            this.Controls.SetChildIndex(this.detectLanguageCheckBox, 0);
            this.Controls.SetChildIndex(this.enableCustomMapsCheckBox, 0);
            this.Controls.SetChildIndex(this.mapListComboBox, 0);
            this.Controls.SetChildIndex(this.customMapLabel, 0);
            this.Controls.SetChildIndex(this.titanQuestPathBrowseButton, 0);
            this.Controls.SetChildIndex(this.suppressWarningsCheckBox, 0);
            this.Controls.SetChildIndex(this.detectGamePathsCheckBox, 0);
            this.Controls.SetChildIndex(this.titanQuestPathLabel, 0);
            this.Controls.SetChildIndex(this.titanQuestPathTextBox, 0);
            this.Controls.SetChildIndex(this.playerReadonlyCheckbox, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ScalingCheckBox playerReadonlyCheckbox;
	}
}