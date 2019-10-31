//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Components;
using TQVaultAE.Presentation;

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
            this.allowItemEditCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.allowItemCopyCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.skipTitleCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.loadLastCharacterCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.loadLastVaultCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.vaultPathTextBox = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.vaultPathLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.cancelButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.okayButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.resetButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.vaultPathBrowseButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.enableCustomMapsCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.loadAllFilesCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.suppressWarningsCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.playerReadonlyCheckbox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.characterEditCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.EnableDetailedTooltipViewCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.ItemBGColorOpacityLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.EnableCharacterRequierementBGColorCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.languageComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.languageLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.detectLanguageCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.titanQuestPathTextBox = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.titanQuestPathLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.immortalThronePathLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.immortalThronePathTextBox = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.detectGamePathsCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.titanQuestPathBrowseButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.immortalThronePathBrowseButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.customMapLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.mapListComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.baseFontLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.baseFontComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.tableLayoutPanelButtons = new System.Windows.Forms.TableLayoutPanel();
            this.ItemBGColorOpacityTrackBar = new System.Windows.Forms.TrackBar();
            this.tableLayoutPanelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ItemBGColorOpacityTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // allowItemEditCheckBox
            // 
            this.allowItemEditCheckBox.AutoSize = true;
            this.allowItemEditCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.allowItemEditCheckBox.Location = new System.Drawing.Point(621, 261);
            this.allowItemEditCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.allowItemEditCheckBox.Name = "allowItemEditCheckBox";
            this.allowItemEditCheckBox.Size = new System.Drawing.Size(318, 32);
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
            this.allowItemCopyCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.allowItemCopyCheckBox.Location = new System.Drawing.Point(621, 295);
            this.allowItemCopyCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.allowItemCopyCheckBox.Name = "allowItemCopyCheckBox";
            this.allowItemCopyCheckBox.Size = new System.Drawing.Size(240, 32);
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
            this.skipTitleCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.skipTitleCheckBox.Location = new System.Drawing.Point(622, 54);
            this.skipTitleCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.skipTitleCheckBox.Name = "skipTitleCheckBox";
            this.skipTitleCheckBox.Size = new System.Drawing.Size(376, 32);
            this.skipTitleCheckBox.TabIndex = 2;
            this.skipTitleCheckBox.Text = "Automatically Bypass Title Screen";
            this.toolTip.SetToolTip(this.skipTitleCheckBox, "Ticking this box will automatically hit\r\nthe Enter key on the title screen.");
            this.skipTitleCheckBox.UseVisualStyleBackColor = true;
            this.skipTitleCheckBox.CheckedChanged += new System.EventHandler(this.SkipTitleCheckBoxCheckedChanged);
            // 
            // loadLastCharacterCheckBox
            // 
            this.loadLastCharacterCheckBox.AutoSize = true;
            this.loadLastCharacterCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.loadLastCharacterCheckBox.Location = new System.Drawing.Point(622, 104);
            this.loadLastCharacterCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.loadLastCharacterCheckBox.Name = "loadLastCharacterCheckBox";
            this.loadLastCharacterCheckBox.Size = new System.Drawing.Size(501, 32);
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
            this.loadLastVaultCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.loadLastVaultCheckBox.Location = new System.Drawing.Point(622, 140);
            this.loadLastVaultCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.loadLastVaultCheckBox.Name = "loadLastVaultCheckBox";
            this.loadLastVaultCheckBox.Size = new System.Drawing.Size(457, 32);
            this.loadLastVaultCheckBox.TabIndex = 6;
            this.loadLastVaultCheckBox.Text = "Automatically Load the last opened Vault";
            this.toolTip.SetToolTip(this.loadLastVaultCheckBox, "Selecting this item will automatically load the\r\nlast opened vault when TQVault w" +
        "as closed.\r\nTQVault will automatically open Main Vault\r\nif nothing is chosen.");
            this.loadLastVaultCheckBox.UseVisualStyleBackColor = true;
            this.loadLastVaultCheckBox.CheckedChanged += new System.EventHandler(this.LoadLastVaultCheckBoxCheckedChanged);
            // 
            // vaultPathTextBox
            // 
            this.vaultPathTextBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.vaultPathTextBox.Location = new System.Drawing.Point(15, 58);
            this.vaultPathTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.vaultPathTextBox.Name = "vaultPathTextBox";
            this.vaultPathTextBox.Size = new System.Drawing.Size(495, 35);
            this.vaultPathTextBox.TabIndex = 0;
            this.toolTip.SetToolTip(this.vaultPathTextBox, resources.GetString("vaultPathTextBox.ToolTip"));
            this.vaultPathTextBox.Leave += new System.EventHandler(this.VaultPathTextBoxLeave);
            // 
            // vaultPathLabel
            // 
            this.vaultPathLabel.AutoSize = true;
            this.vaultPathLabel.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.vaultPathLabel.Location = new System.Drawing.Point(15, 30);
            this.vaultPathLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.vaultPathLabel.Name = "vaultPathLabel";
            this.vaultPathLabel.Size = new System.Drawing.Size(118, 28);
            this.vaultPathLabel.TabIndex = 14;
            this.vaultPathLabel.Text = "Vault Path";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cancelButton.BackColor = System.Drawing.Color.Transparent;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.DownBitmap")));
            this.cancelButton.FlatAppearance.BorderSize = 0;
            this.cancelButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
            this.cancelButton.Location = new System.Drawing.Point(561, 10);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.OverBitmap")));
            this.cancelButton.Size = new System.Drawing.Size(171, 38);
            this.cancelButton.SizeToGraphic = false;
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.UpBitmap")));
            this.cancelButton.UseCustomGraphic = true;
            this.cancelButton.UseVisualStyleBackColor = false;
            // 
            // okayButton
            // 
            this.okayButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okayButton.BackColor = System.Drawing.Color.Transparent;
            this.okayButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okayButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.DownBitmap")));
            this.okayButton.FlatAppearance.BorderSize = 0;
            this.okayButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.okayButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.okayButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.Image = ((System.Drawing.Image)(resources.GetObject("okayButton.Image")));
            this.okayButton.Location = new System.Drawing.Point(357, 10);
            this.okayButton.Margin = new System.Windows.Forms.Padding(4);
            this.okayButton.Name = "okayButton";
            this.okayButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.OverBitmap")));
            this.okayButton.Size = new System.Drawing.Size(171, 38);
            this.okayButton.SizeToGraphic = false;
            this.okayButton.TabIndex = 12;
            this.okayButton.Text = "OK";
            this.okayButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.UpBitmap")));
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
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.resetButton.BackColor = System.Drawing.Color.Transparent;
            this.resetButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("resetButton.DownBitmap")));
            this.resetButton.FlatAppearance.BorderSize = 0;
            this.resetButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.resetButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.resetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.resetButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.resetButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.resetButton.Image = ((System.Drawing.Image)(resources.GetObject("resetButton.Image")));
            this.resetButton.Location = new System.Drawing.Point(915, 10);
            this.resetButton.Margin = new System.Windows.Forms.Padding(4);
            this.resetButton.Name = "resetButton";
            this.resetButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("resetButton.OverBitmap")));
            this.resetButton.Size = new System.Drawing.Size(171, 38);
            this.resetButton.SizeToGraphic = false;
            this.resetButton.TabIndex = 11;
            this.resetButton.Text = "Reset";
            this.toolTip.SetToolTip(this.resetButton, "Causes the configuration to Reset to the\r\nlast saved configuration.");
            this.resetButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("resetButton.UpBitmap")));
            this.resetButton.UseCustomGraphic = true;
            this.resetButton.UseVisualStyleBackColor = false;
            this.resetButton.Click += new System.EventHandler(this.ResetButtonClick);
            // 
            // vaultPathBrowseButton
            // 
            this.vaultPathBrowseButton.BackColor = System.Drawing.Color.Transparent;
            this.vaultPathBrowseButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("vaultPathBrowseButton.DownBitmap")));
            this.vaultPathBrowseButton.FlatAppearance.BorderSize = 0;
            this.vaultPathBrowseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.vaultPathBrowseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.vaultPathBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.vaultPathBrowseButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.vaultPathBrowseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.vaultPathBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("vaultPathBrowseButton.Image")));
            this.vaultPathBrowseButton.Location = new System.Drawing.Point(519, 54);
            this.vaultPathBrowseButton.Margin = new System.Windows.Forms.Padding(4);
            this.vaultPathBrowseButton.Name = "vaultPathBrowseButton";
            this.vaultPathBrowseButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("vaultPathBrowseButton.OverBitmap")));
            this.vaultPathBrowseButton.Size = new System.Drawing.Size(59, 38);
            this.vaultPathBrowseButton.SizeToGraphic = false;
            this.vaultPathBrowseButton.TabIndex = 1;
            this.vaultPathBrowseButton.Text = "...";
            this.vaultPathBrowseButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("vaultPathBrowseButton.UpBitmap")));
            this.vaultPathBrowseButton.UseCustomGraphic = true;
            this.vaultPathBrowseButton.UseVisualStyleBackColor = false;
            this.vaultPathBrowseButton.Click += new System.EventHandler(this.VaultPathBrowseButtonClick);
            // 
            // enableCustomMapsCheckBox
            // 
            this.enableCustomMapsCheckBox.AutoSize = true;
            this.enableCustomMapsCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.enableCustomMapsCheckBox.Location = new System.Drawing.Point(15, 442);
            this.enableCustomMapsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.enableCustomMapsCheckBox.Name = "enableCustomMapsCheckBox";
            this.enableCustomMapsCheckBox.Size = new System.Drawing.Size(248, 32);
            this.enableCustomMapsCheckBox.TabIndex = 25;
            this.enableCustomMapsCheckBox.Text = "Enable Custom Maps";
            this.toolTip.SetToolTip(this.enableCustomMapsCheckBox, "Selecting this item will \r\nenable the dropdown\r\nto select custom maps.");
            this.enableCustomMapsCheckBox.UseVisualStyleBackColor = true;
            this.enableCustomMapsCheckBox.CheckedChanged += new System.EventHandler(this.EnableCustomMapsCheckBoxCheckedChanged);
            // 
            // loadAllFilesCheckBox
            // 
            this.loadAllFilesCheckBox.AutoSize = true;
            this.loadAllFilesCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.loadAllFilesCheckBox.Location = new System.Drawing.Point(622, 174);
            this.loadAllFilesCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.loadAllFilesCheckBox.Name = "loadAllFilesCheckBox";
            this.loadAllFilesCheckBox.Size = new System.Drawing.Size(437, 32);
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
            this.suppressWarningsCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.suppressWarningsCheckBox.Location = new System.Drawing.Point(621, 352);
            this.suppressWarningsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.suppressWarningsCheckBox.Name = "suppressWarningsCheckBox";
            this.suppressWarningsCheckBox.Size = new System.Drawing.Size(343, 32);
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
            this.playerReadonlyCheckbox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.playerReadonlyCheckbox.Location = new System.Drawing.Point(621, 387);
            this.playerReadonlyCheckbox.Margin = new System.Windows.Forms.Padding(4);
            this.playerReadonlyCheckbox.Name = "playerReadonlyCheckbox";
            this.playerReadonlyCheckbox.Size = new System.Drawing.Size(319, 32);
            this.playerReadonlyCheckbox.TabIndex = 33;
            this.playerReadonlyCheckbox.Text = "Player Equipment ReadOnly";
            this.toolTip.SetToolTip(this.playerReadonlyCheckbox, "Avoid save game corruption that occurs (randomly). When enabled, player equipment" +
        " will be read-only,  you won\'t be able to select or move any item.");
            this.playerReadonlyCheckbox.UseVisualStyleBackColor = true;
            this.playerReadonlyCheckbox.CheckedChanged += new System.EventHandler(this.PlayerReadonlyCheckboxCheckedChanged);
            // 
            // characterEditCheckBox
            // 
            this.characterEditCheckBox.AutoSize = true;
            this.characterEditCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.characterEditCheckBox.Location = new System.Drawing.Point(621, 225);
            this.characterEditCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.characterEditCheckBox.Name = "characterEditCheckBox";
            this.characterEditCheckBox.Size = new System.Drawing.Size(372, 32);
            this.characterEditCheckBox.TabIndex = 34;
            this.characterEditCheckBox.Text = "Allow Character Editing Features";
            this.toolTip.SetToolTip(this.characterEditCheckBox, "Turns on the editing features in the context menu.\r\nThese include item creation a" +
        "nd modification.");
            this.characterEditCheckBox.UseVisualStyleBackColor = true;
            this.characterEditCheckBox.CheckedChanged += new System.EventHandler(this.CharacterEditCheckBox_CheckedChanged);
            // 
            // EnableDetailedTooltipViewCheckBox
            // 
            this.EnableDetailedTooltipViewCheckBox.AutoSize = true;
            this.EnableDetailedTooltipViewCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.EnableDetailedTooltipViewCheckBox.Location = new System.Drawing.Point(620, 421);
            this.EnableDetailedTooltipViewCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.EnableDetailedTooltipViewCheckBox.Name = "EnableDetailedTooltipViewCheckBox";
            this.EnableDetailedTooltipViewCheckBox.Size = new System.Drawing.Size(329, 32);
            this.EnableDetailedTooltipViewCheckBox.TabIndex = 38;
            this.EnableDetailedTooltipViewCheckBox.Text = "Enable Detailed Tooltip View";
            this.toolTip.SetToolTip(this.EnableDetailedTooltipViewCheckBox, "Split tooltip attributes into Prefix/Base/Suffix categories");
            this.EnableDetailedTooltipViewCheckBox.UseVisualStyleBackColor = true;
            this.EnableDetailedTooltipViewCheckBox.CheckedChanged += new System.EventHandler(this.EnableDetailedTooltipViewCheckBox_CheckedChanged);
            // 
            // ItemBGColorOpacityLabel
            // 
            this.ItemBGColorOpacityLabel.AutoSize = true;
            this.ItemBGColorOpacityLabel.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.ItemBGColorOpacityLabel.Location = new System.Drawing.Point(615, 476);
            this.ItemBGColorOpacityLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ItemBGColorOpacityLabel.Name = "ItemBGColorOpacityLabel";
            this.ItemBGColorOpacityLabel.Size = new System.Drawing.Size(244, 28);
            this.ItemBGColorOpacityLabel.TabIndex = 40;
            this.ItemBGColorOpacityLabel.Text = "Item BG Alpha Color :";
            this.toolTip.SetToolTip(this.ItemBGColorOpacityLabel, "Item background color opacity level");
            // 
            // EnableCharacterRequierementBGColorCheckBox
            // 
            this.EnableCharacterRequierementBGColorCheckBox.AutoSize = true;
            this.EnableCharacterRequierementBGColorCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.EnableCharacterRequierementBGColorCheckBox.ForeColor = System.Drawing.Color.Orange;
            this.EnableCharacterRequierementBGColorCheckBox.Location = new System.Drawing.Point(619, 508);
            this.EnableCharacterRequierementBGColorCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.EnableCharacterRequierementBGColorCheckBox.Name = "EnableCharacterRequierementBGColorCheckBox";
            this.EnableCharacterRequierementBGColorCheckBox.Size = new System.Drawing.Size(460, 32);
            this.EnableCharacterRequierementBGColorCheckBox.TabIndex = 41;
            this.EnableCharacterRequierementBGColorCheckBox.Text = "Enable Character Requierement BG Color";
            this.toolTip.SetToolTip(this.EnableCharacterRequierementBGColorCheckBox, resources.GetString("EnableCharacterRequierementBGColorCheckBox.ToolTip"));
            this.EnableCharacterRequierementBGColorCheckBox.UseVisualStyleBackColor = true;
            this.EnableCharacterRequierementBGColorCheckBox.CheckedChanged += new System.EventHandler(this.EnableCharacterRequierementBGColorCheckBox_CheckedChanged);
            // 
            // languageComboBox
            // 
            this.languageComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageComboBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Location = new System.Drawing.Point(15, 309);
            this.languageComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(495, 36);
            this.languageComboBox.TabIndex = 15;
            this.languageComboBox.SelectedIndexChanged += new System.EventHandler(this.LanguageComboBoxSelectedIndexChanged);
            // 
            // languageLabel
            // 
            this.languageLabel.AutoSize = true;
            this.languageLabel.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.languageLabel.Location = new System.Drawing.Point(15, 281);
            this.languageLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.languageLabel.Name = "languageLabel";
            this.languageLabel.Size = new System.Drawing.Size(172, 28);
            this.languageLabel.TabIndex = 16;
            this.languageLabel.Text = "Game Language";
            // 
            // detectLanguageCheckBox
            // 
            this.detectLanguageCheckBox.AutoSize = true;
            this.detectLanguageCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.detectLanguageCheckBox.Location = new System.Drawing.Point(15, 344);
            this.detectLanguageCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.detectLanguageCheckBox.Name = "detectLanguageCheckBox";
            this.detectLanguageCheckBox.Size = new System.Drawing.Size(250, 32);
            this.detectLanguageCheckBox.TabIndex = 17;
            this.detectLanguageCheckBox.Text = "Autodetect Language";
            this.detectLanguageCheckBox.UseVisualStyleBackColor = true;
            this.detectLanguageCheckBox.CheckedChanged += new System.EventHandler(this.DetectLanguageCheckBoxCheckedChanged);
            // 
            // titanQuestPathTextBox
            // 
            this.titanQuestPathTextBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.titanQuestPathTextBox.Location = new System.Drawing.Point(15, 135);
            this.titanQuestPathTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.titanQuestPathTextBox.Name = "titanQuestPathTextBox";
            this.titanQuestPathTextBox.Size = new System.Drawing.Size(495, 35);
            this.titanQuestPathTextBox.TabIndex = 18;
            this.titanQuestPathTextBox.Leave += new System.EventHandler(this.TitanQuestPathTextBoxLeave);
            // 
            // titanQuestPathLabel
            // 
            this.titanQuestPathLabel.AutoSize = true;
            this.titanQuestPathLabel.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.titanQuestPathLabel.Location = new System.Drawing.Point(15, 108);
            this.titanQuestPathLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.titanQuestPathLabel.Name = "titanQuestPathLabel";
            this.titanQuestPathLabel.Size = new System.Drawing.Size(164, 28);
            this.titanQuestPathLabel.TabIndex = 19;
            this.titanQuestPathLabel.Text = "TQ Game Path";
            // 
            // immortalThronePathLabel
            // 
            this.immortalThronePathLabel.AutoSize = true;
            this.immortalThronePathLabel.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.immortalThronePathLabel.Location = new System.Drawing.Point(15, 171);
            this.immortalThronePathLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.immortalThronePathLabel.Name = "immortalThronePathLabel";
            this.immortalThronePathLabel.Size = new System.Drawing.Size(151, 28);
            this.immortalThronePathLabel.TabIndex = 20;
            this.immortalThronePathLabel.Text = "IT Game Path";
            // 
            // immortalThronePathTextBox
            // 
            this.immortalThronePathTextBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.immortalThronePathTextBox.Location = new System.Drawing.Point(15, 199);
            this.immortalThronePathTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.immortalThronePathTextBox.Name = "immortalThronePathTextBox";
            this.immortalThronePathTextBox.Size = new System.Drawing.Size(495, 35);
            this.immortalThronePathTextBox.TabIndex = 21;
            this.immortalThronePathTextBox.Leave += new System.EventHandler(this.ImmortalThronePathTextBoxLeave);
            // 
            // detectGamePathsCheckBox
            // 
            this.detectGamePathsCheckBox.AutoSize = true;
            this.detectGamePathsCheckBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.detectGamePathsCheckBox.Location = new System.Drawing.Point(15, 234);
            this.detectGamePathsCheckBox.Margin = new System.Windows.Forms.Padding(4);
            this.detectGamePathsCheckBox.Name = "detectGamePathsCheckBox";
            this.detectGamePathsCheckBox.Size = new System.Drawing.Size(276, 32);
            this.detectGamePathsCheckBox.TabIndex = 22;
            this.detectGamePathsCheckBox.Text = "Autodetect Game Paths";
            this.detectGamePathsCheckBox.UseVisualStyleBackColor = true;
            this.detectGamePathsCheckBox.CheckedChanged += new System.EventHandler(this.DetectGamePathsCheckBoxCheckedChanged);
            // 
            // titanQuestPathBrowseButton
            // 
            this.titanQuestPathBrowseButton.BackColor = System.Drawing.Color.Transparent;
            this.titanQuestPathBrowseButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("titanQuestPathBrowseButton.DownBitmap")));
            this.titanQuestPathBrowseButton.FlatAppearance.BorderSize = 0;
            this.titanQuestPathBrowseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.titanQuestPathBrowseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.titanQuestPathBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.titanQuestPathBrowseButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.titanQuestPathBrowseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.titanQuestPathBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("titanQuestPathBrowseButton.Image")));
            this.titanQuestPathBrowseButton.Location = new System.Drawing.Point(519, 128);
            this.titanQuestPathBrowseButton.Margin = new System.Windows.Forms.Padding(4);
            this.titanQuestPathBrowseButton.Name = "titanQuestPathBrowseButton";
            this.titanQuestPathBrowseButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("titanQuestPathBrowseButton.OverBitmap")));
            this.titanQuestPathBrowseButton.Size = new System.Drawing.Size(59, 38);
            this.titanQuestPathBrowseButton.SizeToGraphic = false;
            this.titanQuestPathBrowseButton.TabIndex = 23;
            this.titanQuestPathBrowseButton.Text = "...";
            this.titanQuestPathBrowseButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("titanQuestPathBrowseButton.UpBitmap")));
            this.titanQuestPathBrowseButton.UseCustomGraphic = true;
            this.titanQuestPathBrowseButton.UseVisualStyleBackColor = false;
            this.titanQuestPathBrowseButton.Click += new System.EventHandler(this.TitanQuestPathBrowseButtonClick);
            // 
            // immortalThronePathBrowseButton
            // 
            this.immortalThronePathBrowseButton.BackColor = System.Drawing.Color.Transparent;
            this.immortalThronePathBrowseButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("immortalThronePathBrowseButton.DownBitmap")));
            this.immortalThronePathBrowseButton.FlatAppearance.BorderSize = 0;
            this.immortalThronePathBrowseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.immortalThronePathBrowseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.immortalThronePathBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.immortalThronePathBrowseButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.immortalThronePathBrowseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.immortalThronePathBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("immortalThronePathBrowseButton.Image")));
            this.immortalThronePathBrowseButton.Location = new System.Drawing.Point(519, 191);
            this.immortalThronePathBrowseButton.Margin = new System.Windows.Forms.Padding(4);
            this.immortalThronePathBrowseButton.Name = "immortalThronePathBrowseButton";
            this.immortalThronePathBrowseButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("immortalThronePathBrowseButton.OverBitmap")));
            this.immortalThronePathBrowseButton.Size = new System.Drawing.Size(59, 38);
            this.immortalThronePathBrowseButton.SizeToGraphic = false;
            this.immortalThronePathBrowseButton.TabIndex = 24;
            this.immortalThronePathBrowseButton.Text = "...";
            this.immortalThronePathBrowseButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("immortalThronePathBrowseButton.UpBitmap")));
            this.immortalThronePathBrowseButton.UseCustomGraphic = true;
            this.immortalThronePathBrowseButton.UseVisualStyleBackColor = false;
            this.immortalThronePathBrowseButton.Click += new System.EventHandler(this.ImmortalThronePathBrowseButtonClick);
            // 
            // customMapLabel
            // 
            this.customMapLabel.AutoSize = true;
            this.customMapLabel.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.customMapLabel.Location = new System.Drawing.Point(15, 380);
            this.customMapLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.customMapLabel.Name = "customMapLabel";
            this.customMapLabel.Size = new System.Drawing.Size(143, 28);
            this.customMapLabel.TabIndex = 27;
            this.customMapLabel.Text = "Custom Map";
            // 
            // mapListComboBox
            // 
            this.mapListComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.mapListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mapListComboBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.mapListComboBox.FormattingEnabled = true;
            this.mapListComboBox.Location = new System.Drawing.Point(15, 408);
            this.mapListComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.mapListComboBox.Name = "mapListComboBox";
            this.mapListComboBox.Size = new System.Drawing.Size(495, 36);
            this.mapListComboBox.TabIndex = 26;
            this.mapListComboBox.SelectedIndexChanged += new System.EventHandler(this.MapListComboBoxSelectedIndexChanged);
            // 
            // baseFontLabel
            // 
            this.baseFontLabel.AutoSize = true;
            this.baseFontLabel.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.baseFontLabel.Location = new System.Drawing.Point(15, 476);
            this.baseFontLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.baseFontLabel.Name = "baseFontLabel";
            this.baseFontLabel.Size = new System.Drawing.Size(58, 28);
            this.baseFontLabel.TabIndex = 36;
            this.baseFontLabel.Text = "Font";
            // 
            // baseFontComboBox
            // 
            this.baseFontComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.baseFontComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.baseFontComboBox.Font = new System.Drawing.Font("Albertus MT Light", 14.0625F);
            this.baseFontComboBox.FormattingEnabled = true;
            this.baseFontComboBox.Location = new System.Drawing.Point(15, 504);
            this.baseFontComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.baseFontComboBox.Name = "baseFontComboBox";
            this.baseFontComboBox.Size = new System.Drawing.Size(495, 36);
            this.baseFontComboBox.TabIndex = 35;
            this.baseFontComboBox.SelectedIndexChanged += new System.EventHandler(this.FontComboBoxBase_SelectedIndexChanged);
            // 
            // tableLayoutPanelButtons
            // 
            this.tableLayoutPanelButtons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelButtons.ColumnCount = 5;
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelButtons.Controls.Add(this.okayButton, 1, 0);
            this.tableLayoutPanelButtons.Controls.Add(this.cancelButton, 3, 0);
            this.tableLayoutPanelButtons.Controls.Add(this.resetButton, 4, 0);
            this.tableLayoutPanelButtons.Location = new System.Drawing.Point(32, 547);
            this.tableLayoutPanelButtons.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanelButtons.Name = "tableLayoutPanelButtons";
            this.tableLayoutPanelButtons.RowCount = 1;
            this.tableLayoutPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelButtons.Size = new System.Drawing.Size(1090, 52);
            this.tableLayoutPanelButtons.TabIndex = 37;
            // 
            // ItemBGColorOpacityTrackBar
            // 
            this.ItemBGColorOpacityTrackBar.Location = new System.Drawing.Point(866, 475);
            this.ItemBGColorOpacityTrackBar.Maximum = 255;
            this.ItemBGColorOpacityTrackBar.Name = "ItemBGColorOpacityTrackBar";
            this.ItemBGColorOpacityTrackBar.Size = new System.Drawing.Size(256, 56);
            this.ItemBGColorOpacityTrackBar.TabIndex = 39;
            this.ItemBGColorOpacityTrackBar.TickFrequency = 5;
            this.ItemBGColorOpacityTrackBar.Value = 15;
            this.ItemBGColorOpacityTrackBar.Scroll += new System.EventHandler(this.ItemBGColorOpacityTrackBar_Scroll);
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.ClientSize = new System.Drawing.Size(1152, 614);
            this.Controls.Add(this.EnableCharacterRequierementBGColorCheckBox);
            this.Controls.Add(this.ItemBGColorOpacityLabel);
            this.Controls.Add(this.ItemBGColorOpacityTrackBar);
            this.Controls.Add(this.EnableDetailedTooltipViewCheckBox);
            this.Controls.Add(this.tableLayoutPanelButtons);
            this.Controls.Add(this.baseFontLabel);
            this.Controls.Add(this.baseFontComboBox);
            this.Controls.Add(this.characterEditCheckBox);
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
            this.Controls.Add(this.immortalThronePathBrowseButton);
            this.Controls.Add(this.languageLabel);
            this.Controls.Add(this.allowItemEditCheckBox);
            this.Controls.Add(this.allowItemCopyCheckBox);
            this.Controls.Add(this.skipTitleCheckBox);
            this.Controls.Add(this.immortalThronePathTextBox);
            this.Controls.Add(this.immortalThronePathLabel);
            this.Controls.Add(this.vaultPathLabel);
            this.Controls.Add(this.languageComboBox);
            this.Controls.Add(this.vaultPathBrowseButton);
            this.Controls.Add(this.vaultPathTextBox);
            this.Controls.Add(this.loadLastVaultCheckBox);
            this.Controls.Add(this.loadLastCharacterCheckBox);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Albertus MT Light", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5);
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
            this.Controls.SetChildIndex(this.loadLastVaultCheckBox, 0);
            this.Controls.SetChildIndex(this.vaultPathTextBox, 0);
            this.Controls.SetChildIndex(this.vaultPathBrowseButton, 0);
            this.Controls.SetChildIndex(this.languageComboBox, 0);
            this.Controls.SetChildIndex(this.vaultPathLabel, 0);
            this.Controls.SetChildIndex(this.immortalThronePathLabel, 0);
            this.Controls.SetChildIndex(this.immortalThronePathTextBox, 0);
            this.Controls.SetChildIndex(this.skipTitleCheckBox, 0);
            this.Controls.SetChildIndex(this.allowItemCopyCheckBox, 0);
            this.Controls.SetChildIndex(this.allowItemEditCheckBox, 0);
            this.Controls.SetChildIndex(this.languageLabel, 0);
            this.Controls.SetChildIndex(this.immortalThronePathBrowseButton, 0);
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
            this.Controls.SetChildIndex(this.characterEditCheckBox, 0);
            this.Controls.SetChildIndex(this.baseFontComboBox, 0);
            this.Controls.SetChildIndex(this.baseFontLabel, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanelButtons, 0);
            this.Controls.SetChildIndex(this.EnableDetailedTooltipViewCheckBox, 0);
            this.Controls.SetChildIndex(this.ItemBGColorOpacityTrackBar, 0);
            this.Controls.SetChildIndex(this.ItemBGColorOpacityLabel, 0);
            this.Controls.SetChildIndex(this.EnableCharacterRequierementBGColorCheckBox, 0);
            this.tableLayoutPanelButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ItemBGColorOpacityTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ScalingCheckBox playerReadonlyCheckbox;
		private ScalingCheckBox characterEditCheckBox;
		private ScalingLabel baseFontLabel;
		private ScalingComboBox baseFontComboBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelButtons;
        private ScalingCheckBox EnableDetailedTooltipViewCheckBox;
		private System.Windows.Forms.TrackBar ItemBGColorOpacityTrackBar;
		private ScalingLabel ItemBGColorOpacityLabel;
		private ScalingCheckBox EnableCharacterRequierementBGColorCheckBox;
	}
}