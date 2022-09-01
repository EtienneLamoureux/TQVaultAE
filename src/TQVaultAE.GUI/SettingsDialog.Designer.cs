//-----------------------------------------------------------------------
// <copyright file="SettingsDialog.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Drawing;
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
		/// Browse button for Titan Quest game path
		/// </summary>
		private ScalingButton titanQuestPathBrowseButton;

		/// <summary>
		/// Browse button for Immortal Throne game path
		/// </summary>
		private ScalingButton immortalThronePathBrowseButton;

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
            this.loadAllFilesCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.suppressWarningsCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.playerReadonlyCheckbox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.characterEditCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.EnableDetailedTooltipViewCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.ItemBGColorOpacityLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.EnableItemRequirementRestrictionCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.hotReloadCheckBox = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingCheckBoxEnableEpicLegendaryAffixes = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingCheckBoxDisableAutoStacking = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.checkGroupBoxGitBackup = new TQVaultAE.GUI.Components.CheckGroupBox();
            this.bufferedFlowLayoutPanelGitBackup = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.scalingCheckBoxDisableLegacyBackup = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingCheckBoxBackupPlayerSaves = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingLabelGitRepository = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingTextBoxGitRepository = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.checkGroupBoxOriginalTQSupport = new TQVaultAE.GUI.Components.CheckGroupBox();
            this.bufferedFlowLayoutPanelTQOriginalSupport = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.linkLabelTQOriginalSupport = new System.Windows.Forms.LinkLabel();
            this.languageComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.languageLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.titanQuestPathTextBox = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.titanQuestPathLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.immortalThronePathLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.immortalThronePathTextBox = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.titanQuestPathBrowseButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.immortalThronePathBrowseButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.customMapLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.mapListComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.baseFontLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.baseFontComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.tableLayoutPanelButtons = new System.Windows.Forms.TableLayoutPanel();
            this.ItemBGColorOpacityTrackBar = new System.Windows.Forms.TrackBar();
            this.scalingCheckBoxEnableSounds = new TQVaultAE.GUI.Components.ScalingCheckBox();
            this.scalingLabelCSVDelim = new TQVaultAE.GUI.Components.ScalingLabel();
            this.scalingComboBoxCSVDelim = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.bufferedFlowLayoutPanelGeneralSettings = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.checkGroupBoxAllowCheats = new TQVaultAE.GUI.Components.CheckGroupBox();
            this.bufferedFlowLayoutPanelCheats = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.groupBoxGfxAndAudio = new System.Windows.Forms.GroupBox();
            this.bufferedFlowLayoutPanelGfxAndAudio = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.detectLanguageCheckBox = new TQVaultAE.GUI.Components.CheckGroupBox();
            this.bufferedFlowLayoutPanelLanguage = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.detectGamePathsCheckBox = new TQVaultAE.GUI.Components.CheckGroupBox();
            this.bufferedFlowLayoutPanelAutoDetectGamePath = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.bufferedFlowLayoutPanelTQPath = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.bufferedFlowLayoutPanelTQITPath = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.enableCustomMapsCheckBox = new TQVaultAE.GUI.Components.CheckGroupBox();
            this.bufferedFlowLayoutPanelCustomMap = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.bufferedTableLayoutPanelSkeleton = new TQVaultAE.GUI.Components.BufferedTableLayoutPanel();
            this.bufferedFlowLayoutPanelSkeletonRight = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.bufferedFlowLayoutPanelSkeletonLeft = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.bufferedFlowLayoutPanelVaultPath = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.bufferedFlowLayoutPanelSkeletonCenter = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.checkGroupBoxGitBackup.SuspendLayout();
            this.bufferedFlowLayoutPanelGitBackup.SuspendLayout();
            this.checkGroupBoxOriginalTQSupport.SuspendLayout();
            this.bufferedFlowLayoutPanelTQOriginalSupport.SuspendLayout();
            this.tableLayoutPanelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ItemBGColorOpacityTrackBar)).BeginInit();
            this.groupBoxGeneral.SuspendLayout();
            this.bufferedFlowLayoutPanelGeneralSettings.SuspendLayout();
            this.checkGroupBoxAllowCheats.SuspendLayout();
            this.bufferedFlowLayoutPanelCheats.SuspendLayout();
            this.groupBoxGfxAndAudio.SuspendLayout();
            this.bufferedFlowLayoutPanelGfxAndAudio.SuspendLayout();
            this.detectLanguageCheckBox.SuspendLayout();
            this.bufferedFlowLayoutPanelLanguage.SuspendLayout();
            this.detectGamePathsCheckBox.SuspendLayout();
            this.bufferedFlowLayoutPanelAutoDetectGamePath.SuspendLayout();
            this.bufferedFlowLayoutPanelTQPath.SuspendLayout();
            this.bufferedFlowLayoutPanelTQITPath.SuspendLayout();
            this.enableCustomMapsCheckBox.SuspendLayout();
            this.bufferedFlowLayoutPanelCustomMap.SuspendLayout();
            this.bufferedTableLayoutPanelSkeleton.SuspendLayout();
            this.bufferedFlowLayoutPanelSkeletonRight.SuspendLayout();
            this.bufferedFlowLayoutPanelSkeletonLeft.SuspendLayout();
            this.bufferedFlowLayoutPanelVaultPath.SuspendLayout();
            this.bufferedFlowLayoutPanelSkeletonCenter.SuspendLayout();
            this.SuspendLayout();
            // 
            // allowItemEditCheckBox
            // 
            this.allowItemEditCheckBox.AutoSize = true;
            this.allowItemEditCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.allowItemEditCheckBox.ForeColor = System.Drawing.Color.White;
            this.allowItemEditCheckBox.Location = new System.Drawing.Point(3, 59);
            this.allowItemEditCheckBox.Name = "allowItemEditCheckBox";
            this.allowItemEditCheckBox.Size = new System.Drawing.Size(204, 22);
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
            this.allowItemCopyCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.allowItemCopyCheckBox.ForeColor = System.Drawing.Color.White;
            this.allowItemCopyCheckBox.Location = new System.Drawing.Point(3, 31);
            this.allowItemCopyCheckBox.Name = "allowItemCopyCheckBox";
            this.allowItemCopyCheckBox.Size = new System.Drawing.Size(152, 22);
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
            this.skipTitleCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.skipTitleCheckBox.ForeColor = System.Drawing.Color.White;
            this.skipTitleCheckBox.Location = new System.Drawing.Point(3, 3);
            this.skipTitleCheckBox.Name = "skipTitleCheckBox";
            this.skipTitleCheckBox.Size = new System.Drawing.Size(249, 22);
            this.skipTitleCheckBox.TabIndex = 2;
            this.skipTitleCheckBox.Text = "Automatically Bypass Title Screen";
            this.toolTip.SetToolTip(this.skipTitleCheckBox, "Ticking this box will automatically hit\r\nthe Enter key on the title screen.");
            this.skipTitleCheckBox.UseVisualStyleBackColor = true;
            this.skipTitleCheckBox.CheckedChanged += new System.EventHandler(this.SkipTitleCheckBoxCheckedChanged);
            // 
            // loadLastCharacterCheckBox
            // 
            this.loadLastCharacterCheckBox.AutoSize = true;
            this.loadLastCharacterCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.loadLastCharacterCheckBox.ForeColor = System.Drawing.Color.White;
            this.loadLastCharacterCheckBox.Location = new System.Drawing.Point(3, 31);
            this.loadLastCharacterCheckBox.Name = "loadLastCharacterCheckBox";
            this.loadLastCharacterCheckBox.Size = new System.Drawing.Size(324, 22);
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
            this.loadLastVaultCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.loadLastVaultCheckBox.ForeColor = System.Drawing.Color.White;
            this.loadLastVaultCheckBox.Location = new System.Drawing.Point(3, 59);
            this.loadLastVaultCheckBox.Name = "loadLastVaultCheckBox";
            this.loadLastVaultCheckBox.Size = new System.Drawing.Size(291, 22);
            this.loadLastVaultCheckBox.TabIndex = 6;
            this.loadLastVaultCheckBox.Text = "Automatically Load the last opened Vault";
            this.toolTip.SetToolTip(this.loadLastVaultCheckBox, "Selecting this item will automatically load the\r\nlast opened vault when TQVault w" +
        "as closed.\r\nTQVault will automatically open Main Vault\r\nif nothing is chosen.");
            this.loadLastVaultCheckBox.UseVisualStyleBackColor = true;
            this.loadLastVaultCheckBox.CheckedChanged += new System.EventHandler(this.LoadLastVaultCheckBoxCheckedChanged);
            // 
            // vaultPathTextBox
            // 
            this.vaultPathTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.vaultPathTextBox.Location = new System.Drawing.Point(3, 3);
            this.vaultPathTextBox.Name = "vaultPathTextBox";
            this.vaultPathTextBox.Size = new System.Drawing.Size(397, 24);
            this.vaultPathTextBox.TabIndex = 0;
            this.toolTip.SetToolTip(this.vaultPathTextBox, resources.GetString("vaultPathTextBox.ToolTip"));
            this.vaultPathTextBox.Leave += new System.EventHandler(this.VaultPathTextBoxLeave);
            // 
            // vaultPathLabel
            // 
            this.vaultPathLabel.AutoSize = true;
            this.vaultPathLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.vaultPathLabel.ForeColor = System.Drawing.Color.Gold;
            this.vaultPathLabel.Location = new System.Drawing.Point(3, 0);
            this.vaultPathLabel.Name = "vaultPathLabel";
            this.vaultPathLabel.Size = new System.Drawing.Size(74, 18);
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
            this.cancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.cancelButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.cancelButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelButton.Image")));
            this.cancelButton.Location = new System.Drawing.Point(595, 3);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("cancelButton.OverBitmap")));
            this.cancelButton.Size = new System.Drawing.Size(137, 30);
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
            this.okayButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.okayButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.okayButton.Image = ((System.Drawing.Image)(resources.GetObject("okayButton.Image")));
            this.okayButton.Location = new System.Drawing.Point(432, 3);
            this.okayButton.Name = "okayButton";
            this.okayButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("okayButton.OverBitmap")));
            this.okayButton.Size = new System.Drawing.Size(137, 30);
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
            this.resetButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.resetButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.resetButton.Image = ((System.Drawing.Image)(resources.GetObject("resetButton.Image")));
            this.resetButton.Location = new System.Drawing.Point(1026, 3);
            this.resetButton.Name = "resetButton";
            this.resetButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("resetButton.OverBitmap")));
            this.resetButton.Size = new System.Drawing.Size(137, 30);
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
            this.vaultPathBrowseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.vaultPathBrowseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.vaultPathBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("vaultPathBrowseButton.Image")));
            this.vaultPathBrowseButton.Location = new System.Drawing.Point(406, 0);
            this.vaultPathBrowseButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.vaultPathBrowseButton.Name = "vaultPathBrowseButton";
            this.vaultPathBrowseButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("vaultPathBrowseButton.OverBitmap")));
            this.vaultPathBrowseButton.Size = new System.Drawing.Size(47, 30);
            this.vaultPathBrowseButton.SizeToGraphic = false;
            this.vaultPathBrowseButton.TabIndex = 1;
            this.vaultPathBrowseButton.Text = "...";
            this.vaultPathBrowseButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("vaultPathBrowseButton.UpBitmap")));
            this.vaultPathBrowseButton.UseCustomGraphic = true;
            this.vaultPathBrowseButton.UseVisualStyleBackColor = false;
            this.vaultPathBrowseButton.Click += new System.EventHandler(this.VaultPathBrowseButtonClick);
            // 
            // loadAllFilesCheckBox
            // 
            this.loadAllFilesCheckBox.AutoSize = true;
            this.loadAllFilesCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.loadAllFilesCheckBox.ForeColor = System.Drawing.Color.White;
            this.loadAllFilesCheckBox.Location = new System.Drawing.Point(3, 87);
            this.loadAllFilesCheckBox.Name = "loadAllFilesCheckBox";
            this.loadAllFilesCheckBox.Size = new System.Drawing.Size(276, 22);
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
            this.suppressWarningsCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.suppressWarningsCheckBox.ForeColor = System.Drawing.Color.White;
            this.suppressWarningsCheckBox.Location = new System.Drawing.Point(3, 141);
            this.suppressWarningsCheckBox.Name = "suppressWarningsCheckBox";
            this.suppressWarningsCheckBox.Size = new System.Drawing.Size(238, 22);
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
            this.playerReadonlyCheckbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.playerReadonlyCheckbox.ForeColor = System.Drawing.Color.White;
            this.playerReadonlyCheckbox.Location = new System.Drawing.Point(3, 197);
            this.playerReadonlyCheckbox.Name = "playerReadonlyCheckbox";
            this.playerReadonlyCheckbox.Size = new System.Drawing.Size(211, 22);
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
            this.characterEditCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.characterEditCheckBox.ForeColor = System.Drawing.Color.White;
            this.characterEditCheckBox.Location = new System.Drawing.Point(3, 3);
            this.characterEditCheckBox.Name = "characterEditCheckBox";
            this.characterEditCheckBox.Size = new System.Drawing.Size(241, 22);
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
            this.EnableDetailedTooltipViewCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.EnableDetailedTooltipViewCheckBox.ForeColor = System.Drawing.Color.White;
            this.EnableDetailedTooltipViewCheckBox.Location = new System.Drawing.Point(3, 169);
            this.EnableDetailedTooltipViewCheckBox.Name = "EnableDetailedTooltipViewCheckBox";
            this.EnableDetailedTooltipViewCheckBox.Size = new System.Drawing.Size(213, 22);
            this.EnableDetailedTooltipViewCheckBox.TabIndex = 38;
            this.EnableDetailedTooltipViewCheckBox.Text = "Enable Detailed Tooltip View";
            this.toolTip.SetToolTip(this.EnableDetailedTooltipViewCheckBox, "Split tooltip attributes into Prefix/Base/Suffix categories");
            this.EnableDetailedTooltipViewCheckBox.UseVisualStyleBackColor = true;
            this.EnableDetailedTooltipViewCheckBox.CheckedChanged += new System.EventHandler(this.EnableDetailedTooltipViewCheckBox_CheckedChanged);
            // 
            // ItemBGColorOpacityLabel
            // 
            this.ItemBGColorOpacityLabel.AutoSize = true;
            this.ItemBGColorOpacityLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.ItemBGColorOpacityLabel.ForeColor = System.Drawing.Color.White;
            this.ItemBGColorOpacityLabel.Location = new System.Drawing.Point(3, 105);
            this.ItemBGColorOpacityLabel.Name = "ItemBGColorOpacityLabel";
            this.ItemBGColorOpacityLabel.Size = new System.Drawing.Size(151, 18);
            this.ItemBGColorOpacityLabel.TabIndex = 40;
            this.ItemBGColorOpacityLabel.Text = "Item BG Alpha Color :";
            this.toolTip.SetToolTip(this.ItemBGColorOpacityLabel, "Item background color opacity level");
            // 
            // EnableItemRequirementRestrictionCheckBox
            // 
            this.EnableItemRequirementRestrictionCheckBox.AutoSize = true;
            this.EnableItemRequirementRestrictionCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.EnableItemRequirementRestrictionCheckBox.ForeColor = System.Drawing.Color.Orange;
            this.EnableItemRequirementRestrictionCheckBox.Location = new System.Drawing.Point(3, 80);
            this.EnableItemRequirementRestrictionCheckBox.Name = "EnableItemRequirementRestrictionCheckBox";
            this.EnableItemRequirementRestrictionCheckBox.Size = new System.Drawing.Size(304, 22);
            this.EnableItemRequirementRestrictionCheckBox.TabIndex = 41;
            this.EnableItemRequirementRestrictionCheckBox.Text = "Enable Character Requierement BG Color";
            this.toolTip.SetToolTip(this.EnableItemRequirementRestrictionCheckBox, resources.GetString("EnableItemRequirementRestrictionCheckBox.ToolTip"));
            this.EnableItemRequirementRestrictionCheckBox.UseVisualStyleBackColor = true;
            this.EnableItemRequirementRestrictionCheckBox.CheckedChanged += new System.EventHandler(this.EnableItemRequirementRestrictionCheckBox_CheckedChanged);
            // 
            // hotReloadCheckBox
            // 
            this.hotReloadCheckBox.AutoSize = true;
            this.hotReloadCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.hotReloadCheckBox.ForeColor = System.Drawing.Color.White;
            this.hotReloadCheckBox.Location = new System.Drawing.Point(2, 114);
            this.hotReloadCheckBox.Margin = new System.Windows.Forms.Padding(2);
            this.hotReloadCheckBox.Name = "hotReloadCheckBox";
            this.hotReloadCheckBox.Size = new System.Drawing.Size(203, 22);
            this.hotReloadCheckBox.TabIndex = 42;
            this.hotReloadCheckBox.Text = "Allow Hot Reload Features";
            this.toolTip.SetToolTip(this.hotReloadCheckBox, "Turns on the editing features in the context menu.\r\nThese include item creation a" +
        "nd modification.");
            this.hotReloadCheckBox.UseVisualStyleBackColor = true;
            this.hotReloadCheckBox.CheckedChanged += new System.EventHandler(this.hotReloadCheckBox_CheckedChanged);
            // 
            // scalingCheckBoxEnableEpicLegendaryAffixes
            // 
            this.scalingCheckBoxEnableEpicLegendaryAffixes.AutoSize = true;
            this.scalingCheckBoxEnableEpicLegendaryAffixes.Enabled = false;
            this.scalingCheckBoxEnableEpicLegendaryAffixes.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.scalingCheckBoxEnableEpicLegendaryAffixes.ForeColor = System.Drawing.Color.White;
            this.scalingCheckBoxEnableEpicLegendaryAffixes.Location = new System.Drawing.Point(3, 87);
            this.scalingCheckBoxEnableEpicLegendaryAffixes.Name = "scalingCheckBoxEnableEpicLegendaryAffixes";
            this.scalingCheckBoxEnableEpicLegendaryAffixes.Size = new System.Drawing.Size(251, 22);
            this.scalingCheckBoxEnableEpicLegendaryAffixes.TabIndex = 46;
            this.scalingCheckBoxEnableEpicLegendaryAffixes.Text = "Enable Epic and Legendary affixes";
            this.toolTip.SetToolTip(this.scalingCheckBoxEnableEpicLegendaryAffixes, "Allow affixes pickup on Epic and Legendary items");
            this.scalingCheckBoxEnableEpicLegendaryAffixes.UseVisualStyleBackColor = true;
            this.scalingCheckBoxEnableEpicLegendaryAffixes.CheckedChanged += new System.EventHandler(this.scalingCheckBoxEnableEpicLegendaryAffixes_CheckedChanged);
            // 
            // scalingCheckBoxDisableAutoStacking
            // 
            this.scalingCheckBoxDisableAutoStacking.AutoSize = true;
            this.scalingCheckBoxDisableAutoStacking.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.scalingCheckBoxDisableAutoStacking.ForeColor = System.Drawing.Color.White;
            this.scalingCheckBoxDisableAutoStacking.Location = new System.Drawing.Point(3, 225);
            this.scalingCheckBoxDisableAutoStacking.Name = "scalingCheckBoxDisableAutoStacking";
            this.scalingCheckBoxDisableAutoStacking.Size = new System.Drawing.Size(168, 22);
            this.scalingCheckBoxDisableAutoStacking.TabIndex = 47;
            this.scalingCheckBoxDisableAutoStacking.Text = "Disable auto stacking";
            this.toolTip.SetToolTip(this.scalingCheckBoxDisableAutoStacking, "Disable auto stacking for relic, charms and potions");
            this.scalingCheckBoxDisableAutoStacking.UseVisualStyleBackColor = true;
            this.scalingCheckBoxDisableAutoStacking.CheckedChanged += new System.EventHandler(this.scalingCheckBoxDisableAutoStacking_CheckedChanged);
            // 
            // checkGroupBoxGitBackup
            // 
            this.checkGroupBoxGitBackup.AutoSize = true;
            this.checkGroupBoxGitBackup.Checked = false;
            this.checkGroupBoxGitBackup.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Enable;
            this.checkGroupBoxGitBackup.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this.checkGroupBoxGitBackup.Controls.Add(this.bufferedFlowLayoutPanelGitBackup);
            this.checkGroupBoxGitBackup.ForeColor = System.Drawing.Color.Gold;
            this.checkGroupBoxGitBackup.Location = new System.Drawing.Point(3, 374);
            this.checkGroupBoxGitBackup.Name = "checkGroupBoxGitBackup";
            this.checkGroupBoxGitBackup.Size = new System.Drawing.Size(409, 127);
            this.checkGroupBoxGitBackup.TabIndex = 48;
            this.checkGroupBoxGitBackup.TabStop = false;
            this.checkGroupBoxGitBackup.Text = "Git Backup";
            this.toolTip.SetToolTip(this.checkGroupBoxGitBackup, "Enable a backup of your modified vaults & saves to a a git repository");
            this.checkGroupBoxGitBackup.CheckedChanged += new System.EventHandler(this.checkGroupBoxGitBackup_CheckedChanged);
            // 
            // bufferedFlowLayoutPanelGitBackup
            // 
            this.bufferedFlowLayoutPanelGitBackup.AutoSize = true;
            this.bufferedFlowLayoutPanelGitBackup.Controls.Add(this.scalingCheckBoxDisableLegacyBackup);
            this.bufferedFlowLayoutPanelGitBackup.Controls.Add(this.scalingCheckBoxBackupPlayerSaves);
            this.bufferedFlowLayoutPanelGitBackup.Controls.Add(this.scalingLabelGitRepository);
            this.bufferedFlowLayoutPanelGitBackup.Controls.Add(this.scalingTextBoxGitRepository);
            this.bufferedFlowLayoutPanelGitBackup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelGitBackup.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelGitBackup.Location = new System.Drawing.Point(3, 20);
            this.bufferedFlowLayoutPanelGitBackup.Name = "bufferedFlowLayoutPanelGitBackup";
            this.bufferedFlowLayoutPanelGitBackup.Size = new System.Drawing.Size(403, 104);
            this.bufferedFlowLayoutPanelGitBackup.TabIndex = 1;
            // 
            // scalingCheckBoxDisableLegacyBackup
            // 
            this.scalingCheckBoxDisableLegacyBackup.AutoSize = true;
            this.scalingCheckBoxDisableLegacyBackup.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.scalingCheckBoxDisableLegacyBackup.ForeColor = System.Drawing.Color.White;
            this.scalingCheckBoxDisableLegacyBackup.Location = new System.Drawing.Point(3, 3);
            this.scalingCheckBoxDisableLegacyBackup.Name = "scalingCheckBoxDisableLegacyBackup";
            this.scalingCheckBoxDisableLegacyBackup.Size = new System.Drawing.Size(174, 22);
            this.scalingCheckBoxDisableLegacyBackup.TabIndex = 1;
            this.scalingCheckBoxDisableLegacyBackup.Text = "Disable legacy backup";
            this.toolTip.SetToolTip(this.scalingCheckBoxDisableLegacyBackup, "Optionaly disable the old backup system");
            this.scalingCheckBoxDisableLegacyBackup.UseVisualStyleBackColor = true;
            this.scalingCheckBoxDisableLegacyBackup.CheckedChanged += new System.EventHandler(this.scalingCheckBoxDisableLegacyBackup_CheckedChanged);
            // 
            // scalingCheckBoxBackupPlayerSaves
            // 
            this.scalingCheckBoxBackupPlayerSaves.AutoSize = true;
            this.scalingCheckBoxBackupPlayerSaves.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.scalingCheckBoxBackupPlayerSaves.ForeColor = System.Drawing.Color.White;
            this.scalingCheckBoxBackupPlayerSaves.Location = new System.Drawing.Point(3, 31);
            this.scalingCheckBoxBackupPlayerSaves.Name = "scalingCheckBoxBackupPlayerSaves";
            this.scalingCheckBoxBackupPlayerSaves.Size = new System.Drawing.Size(163, 22);
            this.scalingCheckBoxBackupPlayerSaves.TabIndex = 6;
            this.scalingCheckBoxBackupPlayerSaves.Text = "Backup player saves";
            this.toolTip.SetToolTip(this.scalingCheckBoxBackupPlayerSaves, "Include your character save files in the backup");
            this.scalingCheckBoxBackupPlayerSaves.UseVisualStyleBackColor = true;
            this.scalingCheckBoxBackupPlayerSaves.CheckedChanged += new System.EventHandler(this.scalingCheckBoxBackupPlayerSaves_CheckedChanged);
            // 
            // scalingLabelGitRepository
            // 
            this.scalingLabelGitRepository.AutoSize = true;
            this.scalingLabelGitRepository.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.scalingLabelGitRepository.ForeColor = System.Drawing.Color.White;
            this.scalingLabelGitRepository.Location = new System.Drawing.Point(3, 56);
            this.scalingLabelGitRepository.Name = "scalingLabelGitRepository";
            this.scalingLabelGitRepository.Size = new System.Drawing.Size(138, 18);
            this.scalingLabelGitRepository.TabIndex = 5;
            this.scalingLabelGitRepository.Text = "Git Repository Url : ";
            // 
            // scalingTextBoxGitRepository
            // 
            this.scalingTextBoxGitRepository.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.scalingTextBoxGitRepository.Location = new System.Drawing.Point(3, 77);
            this.scalingTextBoxGitRepository.Name = "scalingTextBoxGitRepository";
            this.scalingTextBoxGitRepository.Size = new System.Drawing.Size(397, 24);
            this.scalingTextBoxGitRepository.TabIndex = 4;
            this.scalingTextBoxGitRepository.TextChanged += new System.EventHandler(this.scalingTextBoxGitRepository_TextChanged);
            // 
            // checkGroupBoxOriginalTQSupport
            // 
            this.checkGroupBoxOriginalTQSupport.Checked = false;
            this.checkGroupBoxOriginalTQSupport.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Enable;
            this.checkGroupBoxOriginalTQSupport.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this.checkGroupBoxOriginalTQSupport.Controls.Add(this.bufferedFlowLayoutPanelTQOriginalSupport);
            this.checkGroupBoxOriginalTQSupport.ForeColor = System.Drawing.Color.Gold;
            this.checkGroupBoxOriginalTQSupport.Location = new System.Drawing.Point(3, 206);
            this.checkGroupBoxOriginalTQSupport.Name = "checkGroupBoxOriginalTQSupport";
            this.checkGroupBoxOriginalTQSupport.Size = new System.Drawing.Size(316, 49);
            this.checkGroupBoxOriginalTQSupport.TabIndex = 53;
            this.checkGroupBoxOriginalTQSupport.TabStop = false;
            this.checkGroupBoxOriginalTQSupport.Text = "TQ original support";
            this.toolTip.SetToolTip(this.checkGroupBoxOriginalTQSupport, "Provide support for original Titan Quest character file editing");
            this.checkGroupBoxOriginalTQSupport.CheckedChanged += new System.EventHandler(this.checkGroupBoxOriginalTQSupport_CheckedChanged);
            // 
            // bufferedFlowLayoutPanelTQOriginalSupport
            // 
            this.bufferedFlowLayoutPanelTQOriginalSupport.Controls.Add(this.linkLabelTQOriginalSupport);
            this.bufferedFlowLayoutPanelTQOriginalSupport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelTQOriginalSupport.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelTQOriginalSupport.ForeColor = System.Drawing.Color.White;
            this.bufferedFlowLayoutPanelTQOriginalSupport.Location = new System.Drawing.Point(3, 20);
            this.bufferedFlowLayoutPanelTQOriginalSupport.Name = "bufferedFlowLayoutPanelTQOriginalSupport";
            this.bufferedFlowLayoutPanelTQOriginalSupport.Size = new System.Drawing.Size(310, 26);
            this.bufferedFlowLayoutPanelTQOriginalSupport.TabIndex = 1;
            // 
            // linkLabelTQOriginalSupport
            // 
            this.linkLabelTQOriginalSupport.AutoSize = true;
            this.linkLabelTQOriginalSupport.LinkColor = System.Drawing.Color.Magenta;
            this.linkLabelTQOriginalSupport.Location = new System.Drawing.Point(3, 0);
            this.linkLabelTQOriginalSupport.Name = "linkLabelTQOriginalSupport";
            this.linkLabelTQOriginalSupport.Size = new System.Drawing.Size(98, 18);
            this.linkLabelTQOriginalSupport.TabIndex = 54;
            this.linkLabelTQOriginalSupport.TabStop = true;
            this.linkLabelTQOriginalSupport.Text = "How to play...";
            this.linkLabelTQOriginalSupport.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelTQOriginalSupport_LinkClicked);
            // 
            // languageComboBox
            // 
            this.languageComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.languageComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Location = new System.Drawing.Point(3, 21);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(397, 25);
            this.languageComboBox.TabIndex = 15;
            this.languageComboBox.SelectedIndexChanged += new System.EventHandler(this.LanguageComboBoxSelectedIndexChanged);
            // 
            // languageLabel
            // 
            this.languageLabel.AutoSize = true;
            this.languageLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.languageLabel.ForeColor = System.Drawing.Color.White;
            this.languageLabel.Location = new System.Drawing.Point(3, 0);
            this.languageLabel.Name = "languageLabel";
            this.languageLabel.Size = new System.Drawing.Size(117, 18);
            this.languageLabel.TabIndex = 16;
            this.languageLabel.Text = "Game Language";
            // 
            // titanQuestPathTextBox
            // 
            this.titanQuestPathTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.titanQuestPathTextBox.Location = new System.Drawing.Point(3, 3);
            this.titanQuestPathTextBox.Name = "titanQuestPathTextBox";
            this.titanQuestPathTextBox.Size = new System.Drawing.Size(397, 24);
            this.titanQuestPathTextBox.TabIndex = 18;
            this.titanQuestPathTextBox.Leave += new System.EventHandler(this.TitanQuestPathTextBoxLeave);
            // 
            // titanQuestPathLabel
            // 
            this.titanQuestPathLabel.AutoSize = true;
            this.titanQuestPathLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.titanQuestPathLabel.ForeColor = System.Drawing.Color.White;
            this.titanQuestPathLabel.Location = new System.Drawing.Point(3, 0);
            this.titanQuestPathLabel.Name = "titanQuestPathLabel";
            this.titanQuestPathLabel.Size = new System.Drawing.Size(108, 18);
            this.titanQuestPathLabel.TabIndex = 19;
            this.titanQuestPathLabel.Text = "TQ Game Path";
            // 
            // immortalThronePathLabel
            // 
            this.immortalThronePathLabel.AutoSize = true;
            this.immortalThronePathLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.immortalThronePathLabel.ForeColor = System.Drawing.Color.White;
            this.immortalThronePathLabel.Location = new System.Drawing.Point(3, 57);
            this.immortalThronePathLabel.Name = "immortalThronePathLabel";
            this.immortalThronePathLabel.Size = new System.Drawing.Size(99, 18);
            this.immortalThronePathLabel.TabIndex = 20;
            this.immortalThronePathLabel.Text = "IT Game Path";
            // 
            // immortalThronePathTextBox
            // 
            this.immortalThronePathTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.immortalThronePathTextBox.Location = new System.Drawing.Point(3, 3);
            this.immortalThronePathTextBox.Name = "immortalThronePathTextBox";
            this.immortalThronePathTextBox.Size = new System.Drawing.Size(397, 24);
            this.immortalThronePathTextBox.TabIndex = 21;
            this.immortalThronePathTextBox.Leave += new System.EventHandler(this.ImmortalThronePathTextBoxLeave);
            // 
            // titanQuestPathBrowseButton
            // 
            this.titanQuestPathBrowseButton.BackColor = System.Drawing.Color.Transparent;
            this.titanQuestPathBrowseButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("titanQuestPathBrowseButton.DownBitmap")));
            this.titanQuestPathBrowseButton.FlatAppearance.BorderSize = 0;
            this.titanQuestPathBrowseButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.titanQuestPathBrowseButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.titanQuestPathBrowseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.titanQuestPathBrowseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.titanQuestPathBrowseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.titanQuestPathBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("titanQuestPathBrowseButton.Image")));
            this.titanQuestPathBrowseButton.Location = new System.Drawing.Point(406, 0);
            this.titanQuestPathBrowseButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.titanQuestPathBrowseButton.Name = "titanQuestPathBrowseButton";
            this.titanQuestPathBrowseButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("titanQuestPathBrowseButton.OverBitmap")));
            this.titanQuestPathBrowseButton.Size = new System.Drawing.Size(47, 30);
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
            this.immortalThronePathBrowseButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.immortalThronePathBrowseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.immortalThronePathBrowseButton.Image = ((System.Drawing.Image)(resources.GetObject("immortalThronePathBrowseButton.Image")));
            this.immortalThronePathBrowseButton.Location = new System.Drawing.Point(406, 0);
            this.immortalThronePathBrowseButton.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.immortalThronePathBrowseButton.Name = "immortalThronePathBrowseButton";
            this.immortalThronePathBrowseButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("immortalThronePathBrowseButton.OverBitmap")));
            this.immortalThronePathBrowseButton.Size = new System.Drawing.Size(47, 30);
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
            this.customMapLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.customMapLabel.ForeColor = System.Drawing.Color.White;
            this.customMapLabel.Location = new System.Drawing.Point(3, 0);
            this.customMapLabel.Name = "customMapLabel";
            this.customMapLabel.Size = new System.Drawing.Size(94, 18);
            this.customMapLabel.TabIndex = 27;
            this.customMapLabel.Text = "Custom Map";
            // 
            // mapListComboBox
            // 
            this.mapListComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.mapListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mapListComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.mapListComboBox.FormattingEnabled = true;
            this.mapListComboBox.Location = new System.Drawing.Point(3, 21);
            this.mapListComboBox.Name = "mapListComboBox";
            this.mapListComboBox.Size = new System.Drawing.Size(397, 25);
            this.mapListComboBox.TabIndex = 26;
            this.mapListComboBox.SelectedIndexChanged += new System.EventHandler(this.MapListComboBoxSelectedIndexChanged);
            // 
            // baseFontLabel
            // 
            this.baseFontLabel.AutoSize = true;
            this.baseFontLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.baseFontLabel.ForeColor = System.Drawing.Color.White;
            this.baseFontLabel.Location = new System.Drawing.Point(3, 0);
            this.baseFontLabel.Name = "baseFontLabel";
            this.baseFontLabel.Size = new System.Drawing.Size(38, 18);
            this.baseFontLabel.TabIndex = 36;
            this.baseFontLabel.Text = "Font";
            // 
            // baseFontComboBox
            // 
            this.baseFontComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.baseFontComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.baseFontComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.baseFontComboBox.FormattingEnabled = true;
            this.baseFontComboBox.Location = new System.Drawing.Point(3, 21);
            this.baseFontComboBox.Name = "baseFontComboBox";
            this.baseFontComboBox.Size = new System.Drawing.Size(304, 25);
            this.baseFontComboBox.TabIndex = 35;
            this.baseFontComboBox.SelectedIndexChanged += new System.EventHandler(this.FontComboBoxBase_SelectedIndexChanged);
            // 
            // tableLayoutPanelButtons
            // 
            this.tableLayoutPanelButtons.AutoSize = true;
            this.tableLayoutPanelButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelButtons.ColumnCount = 5;
            this.bufferedTableLayoutPanelSkeleton.SetColumnSpan(this.tableLayoutPanelButtons, 5);
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelButtons.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanelButtons.Controls.Add(this.okayButton, 1, 0);
            this.tableLayoutPanelButtons.Controls.Add(this.cancelButton, 3, 0);
            this.tableLayoutPanelButtons.Controls.Add(this.resetButton, 4, 0);
            this.tableLayoutPanelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelButtons.Location = new System.Drawing.Point(3, 545);
            this.tableLayoutPanelButtons.Name = "tableLayoutPanelButtons";
            this.tableLayoutPanelButtons.RowCount = 1;
            this.tableLayoutPanelButtons.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelButtons.Size = new System.Drawing.Size(1166, 36);
            this.tableLayoutPanelButtons.TabIndex = 37;
            // 
            // ItemBGColorOpacityTrackBar
            // 
            this.ItemBGColorOpacityTrackBar.Location = new System.Drawing.Point(3, 126);
            this.ItemBGColorOpacityTrackBar.Maximum = 255;
            this.ItemBGColorOpacityTrackBar.Name = "ItemBGColorOpacityTrackBar";
            this.ItemBGColorOpacityTrackBar.Size = new System.Drawing.Size(304, 45);
            this.ItemBGColorOpacityTrackBar.TabIndex = 39;
            this.ItemBGColorOpacityTrackBar.TickFrequency = 5;
            this.ItemBGColorOpacityTrackBar.Value = 15;
            this.ItemBGColorOpacityTrackBar.Scroll += new System.EventHandler(this.ItemBGColorOpacityTrackBar_Scroll);
            // 
            // scalingCheckBoxEnableSounds
            // 
            this.scalingCheckBoxEnableSounds.AutoSize = true;
            this.scalingCheckBoxEnableSounds.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.scalingCheckBoxEnableSounds.ForeColor = System.Drawing.Color.White;
            this.scalingCheckBoxEnableSounds.Location = new System.Drawing.Point(3, 52);
            this.scalingCheckBoxEnableSounds.Name = "scalingCheckBoxEnableSounds";
            this.scalingCheckBoxEnableSounds.Size = new System.Drawing.Size(184, 22);
            this.scalingCheckBoxEnableSounds.TabIndex = 43;
            this.scalingCheckBoxEnableSounds.Text = "Enable TQVault Sounds";
            this.scalingCheckBoxEnableSounds.UseVisualStyleBackColor = true;
            this.scalingCheckBoxEnableSounds.CheckedChanged += new System.EventHandler(this.scalingCheckBoxEnableSounds_CheckedChanged);
            // 
            // scalingLabelCSVDelim
            // 
            this.scalingLabelCSVDelim.AutoSize = true;
            this.scalingLabelCSVDelim.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.scalingLabelCSVDelim.ForeColor = System.Drawing.Color.White;
            this.scalingLabelCSVDelim.Location = new System.Drawing.Point(3, 250);
            this.scalingLabelCSVDelim.Name = "scalingLabelCSVDelim";
            this.scalingLabelCSVDelim.Size = new System.Drawing.Size(96, 18);
            this.scalingLabelCSVDelim.TabIndex = 45;
            this.scalingLabelCSVDelim.Text = "Csv Delimiter";
            // 
            // scalingComboBoxCSVDelim
            // 
            this.scalingComboBoxCSVDelim.BackColor = System.Drawing.SystemColors.Window;
            this.scalingComboBoxCSVDelim.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.scalingComboBoxCSVDelim.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.scalingComboBoxCSVDelim.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F);
            this.scalingComboBoxCSVDelim.FormattingEnabled = true;
            this.scalingComboBoxCSVDelim.Location = new System.Drawing.Point(3, 271);
            this.scalingComboBoxCSVDelim.Name = "scalingComboBoxCSVDelim";
            this.scalingComboBoxCSVDelim.Size = new System.Drawing.Size(324, 25);
            this.scalingComboBoxCSVDelim.TabIndex = 44;
            this.scalingComboBoxCSVDelim.SelectedIndexChanged += new System.EventHandler(this.scalingComboBoxCSVDelim_SelectedIndexChanged);
            // 
            // groupBoxGeneral
            // 
            this.groupBoxGeneral.AutoSize = true;
            this.groupBoxGeneral.Controls.Add(this.bufferedFlowLayoutPanelGeneralSettings);
            this.groupBoxGeneral.ForeColor = System.Drawing.Color.Gold;
            this.groupBoxGeneral.Location = new System.Drawing.Point(3, 3);
            this.groupBoxGeneral.Name = "groupBoxGeneral";
            this.groupBoxGeneral.Size = new System.Drawing.Size(336, 322);
            this.groupBoxGeneral.TabIndex = 49;
            this.groupBoxGeneral.TabStop = false;
            this.groupBoxGeneral.Text = "General Settings";
            // 
            // bufferedFlowLayoutPanelGeneralSettings
            // 
            this.bufferedFlowLayoutPanelGeneralSettings.AutoSize = true;
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.skipTitleCheckBox);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.loadLastCharacterCheckBox);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.loadLastVaultCheckBox);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.loadAllFilesCheckBox);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.hotReloadCheckBox);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.suppressWarningsCheckBox);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.EnableDetailedTooltipViewCheckBox);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.playerReadonlyCheckbox);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.scalingCheckBoxDisableAutoStacking);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.scalingLabelCSVDelim);
            this.bufferedFlowLayoutPanelGeneralSettings.Controls.Add(this.scalingComboBoxCSVDelim);
            this.bufferedFlowLayoutPanelGeneralSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelGeneralSettings.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelGeneralSettings.Location = new System.Drawing.Point(3, 20);
            this.bufferedFlowLayoutPanelGeneralSettings.Name = "bufferedFlowLayoutPanelGeneralSettings";
            this.bufferedFlowLayoutPanelGeneralSettings.Size = new System.Drawing.Size(330, 299);
            this.bufferedFlowLayoutPanelGeneralSettings.TabIndex = 0;
            // 
            // checkGroupBoxAllowCheats
            // 
            this.checkGroupBoxAllowCheats.AutoSize = true;
            this.checkGroupBoxAllowCheats.Checked = false;
            this.checkGroupBoxAllowCheats.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Enable;
            this.checkGroupBoxAllowCheats.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this.checkGroupBoxAllowCheats.Controls.Add(this.bufferedFlowLayoutPanelCheats);
            this.checkGroupBoxAllowCheats.ForeColor = System.Drawing.Color.Gold;
            this.checkGroupBoxAllowCheats.Location = new System.Drawing.Point(3, 331);
            this.checkGroupBoxAllowCheats.Name = "checkGroupBoxAllowCheats";
            this.checkGroupBoxAllowCheats.Size = new System.Drawing.Size(263, 135);
            this.checkGroupBoxAllowCheats.TabIndex = 51;
            this.checkGroupBoxAllowCheats.TabStop = false;
            this.checkGroupBoxAllowCheats.Text = "Allow Cheats";
            this.checkGroupBoxAllowCheats.CheckedChanged += new System.EventHandler(this.checkGroupBoxCheats_CheckedChanged);
            // 
            // bufferedFlowLayoutPanelCheats
            // 
            this.bufferedFlowLayoutPanelCheats.AutoSize = true;
            this.bufferedFlowLayoutPanelCheats.Controls.Add(this.characterEditCheckBox);
            this.bufferedFlowLayoutPanelCheats.Controls.Add(this.allowItemCopyCheckBox);
            this.bufferedFlowLayoutPanelCheats.Controls.Add(this.allowItemEditCheckBox);
            this.bufferedFlowLayoutPanelCheats.Controls.Add(this.scalingCheckBoxEnableEpicLegendaryAffixes);
            this.bufferedFlowLayoutPanelCheats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelCheats.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelCheats.Location = new System.Drawing.Point(3, 20);
            this.bufferedFlowLayoutPanelCheats.Name = "bufferedFlowLayoutPanelCheats";
            this.bufferedFlowLayoutPanelCheats.Size = new System.Drawing.Size(257, 112);
            this.bufferedFlowLayoutPanelCheats.TabIndex = 1;
            // 
            // groupBoxGfxAndAudio
            // 
            this.groupBoxGfxAndAudio.AutoSize = true;
            this.groupBoxGfxAndAudio.Controls.Add(this.bufferedFlowLayoutPanelGfxAndAudio);
            this.groupBoxGfxAndAudio.ForeColor = System.Drawing.Color.Gold;
            this.groupBoxGfxAndAudio.Location = new System.Drawing.Point(3, 3);
            this.groupBoxGfxAndAudio.Name = "groupBoxGfxAndAudio";
            this.groupBoxGfxAndAudio.Size = new System.Drawing.Size(316, 197);
            this.groupBoxGfxAndAudio.TabIndex = 52;
            this.groupBoxGfxAndAudio.TabStop = false;
            this.groupBoxGfxAndAudio.Text = "Gfx / Audio";
            // 
            // bufferedFlowLayoutPanelGfxAndAudio
            // 
            this.bufferedFlowLayoutPanelGfxAndAudio.AutoSize = true;
            this.bufferedFlowLayoutPanelGfxAndAudio.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(this.baseFontLabel);
            this.bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(this.baseFontComboBox);
            this.bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(this.scalingCheckBoxEnableSounds);
            this.bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(this.EnableItemRequirementRestrictionCheckBox);
            this.bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(this.ItemBGColorOpacityLabel);
            this.bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(this.ItemBGColorOpacityTrackBar);
            this.bufferedFlowLayoutPanelGfxAndAudio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelGfxAndAudio.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelGfxAndAudio.Location = new System.Drawing.Point(3, 20);
            this.bufferedFlowLayoutPanelGfxAndAudio.Name = "bufferedFlowLayoutPanelGfxAndAudio";
            this.bufferedFlowLayoutPanelGfxAndAudio.Size = new System.Drawing.Size(310, 174);
            this.bufferedFlowLayoutPanelGfxAndAudio.TabIndex = 0;
            // 
            // detectLanguageCheckBox
            // 
            this.detectLanguageCheckBox.AutoSize = true;
            this.detectLanguageCheckBox.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Disable;
            this.detectLanguageCheckBox.Controls.Add(this.bufferedFlowLayoutPanelLanguage);
            this.detectLanguageCheckBox.ForeColor = System.Drawing.Color.Gold;
            this.detectLanguageCheckBox.Location = new System.Drawing.Point(3, 296);
            this.detectLanguageCheckBox.Name = "detectLanguageCheckBox";
            this.detectLanguageCheckBox.Size = new System.Drawing.Size(409, 72);
            this.detectLanguageCheckBox.TabIndex = 53;
            this.detectLanguageCheckBox.TabStop = false;
            this.detectLanguageCheckBox.Text = "Autodetect Language";
            this.detectLanguageCheckBox.CheckedChanged += new System.EventHandler(this.DetectLanguageCheckBoxCheckedChanged);
            // 
            // bufferedFlowLayoutPanelLanguage
            // 
            this.bufferedFlowLayoutPanelLanguage.AutoSize = true;
            this.bufferedFlowLayoutPanelLanguage.Controls.Add(this.languageLabel);
            this.bufferedFlowLayoutPanelLanguage.Controls.Add(this.languageComboBox);
            this.bufferedFlowLayoutPanelLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelLanguage.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelLanguage.Location = new System.Drawing.Point(3, 20);
            this.bufferedFlowLayoutPanelLanguage.Name = "bufferedFlowLayoutPanelLanguage";
            this.bufferedFlowLayoutPanelLanguage.Size = new System.Drawing.Size(403, 49);
            this.bufferedFlowLayoutPanelLanguage.TabIndex = 1;
            // 
            // detectGamePathsCheckBox
            // 
            this.detectGamePathsCheckBox.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Disable;
            this.detectGamePathsCheckBox.Controls.Add(this.bufferedFlowLayoutPanelAutoDetectGamePath);
            this.detectGamePathsCheckBox.ForeColor = System.Drawing.Color.Gold;
            this.detectGamePathsCheckBox.Location = new System.Drawing.Point(3, 60);
            this.detectGamePathsCheckBox.Name = "detectGamePathsCheckBox";
            this.detectGamePathsCheckBox.Size = new System.Drawing.Size(466, 141);
            this.detectGamePathsCheckBox.TabIndex = 54;
            this.detectGamePathsCheckBox.TabStop = false;
            this.detectGamePathsCheckBox.Text = "Autodetect Game Paths";
            this.detectGamePathsCheckBox.CheckedChanged += new System.EventHandler(this.DetectGamePathsCheckBoxCheckedChanged);
            // 
            // bufferedFlowLayoutPanelAutoDetectGamePath
            // 
            this.bufferedFlowLayoutPanelAutoDetectGamePath.Controls.Add(this.titanQuestPathLabel);
            this.bufferedFlowLayoutPanelAutoDetectGamePath.Controls.Add(this.bufferedFlowLayoutPanelTQPath);
            this.bufferedFlowLayoutPanelAutoDetectGamePath.Controls.Add(this.immortalThronePathLabel);
            this.bufferedFlowLayoutPanelAutoDetectGamePath.Controls.Add(this.bufferedFlowLayoutPanelTQITPath);
            this.bufferedFlowLayoutPanelAutoDetectGamePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelAutoDetectGamePath.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelAutoDetectGamePath.Location = new System.Drawing.Point(3, 20);
            this.bufferedFlowLayoutPanelAutoDetectGamePath.Name = "bufferedFlowLayoutPanelAutoDetectGamePath";
            this.bufferedFlowLayoutPanelAutoDetectGamePath.Size = new System.Drawing.Size(460, 118);
            this.bufferedFlowLayoutPanelAutoDetectGamePath.TabIndex = 1;
            // 
            // bufferedFlowLayoutPanelTQPath
            // 
            this.bufferedFlowLayoutPanelTQPath.AutoSize = true;
            this.bufferedFlowLayoutPanelTQPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedFlowLayoutPanelTQPath.Controls.Add(this.titanQuestPathTextBox);
            this.bufferedFlowLayoutPanelTQPath.Controls.Add(this.titanQuestPathBrowseButton);
            this.bufferedFlowLayoutPanelTQPath.Location = new System.Drawing.Point(3, 21);
            this.bufferedFlowLayoutPanelTQPath.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.bufferedFlowLayoutPanelTQPath.Name = "bufferedFlowLayoutPanelTQPath";
            this.bufferedFlowLayoutPanelTQPath.Size = new System.Drawing.Size(456, 33);
            this.bufferedFlowLayoutPanelTQPath.TabIndex = 55;
            // 
            // bufferedFlowLayoutPanelTQITPath
            // 
            this.bufferedFlowLayoutPanelTQITPath.AutoSize = true;
            this.bufferedFlowLayoutPanelTQITPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedFlowLayoutPanelTQITPath.Controls.Add(this.immortalThronePathTextBox);
            this.bufferedFlowLayoutPanelTQITPath.Controls.Add(this.immortalThronePathBrowseButton);
            this.bufferedFlowLayoutPanelTQITPath.Location = new System.Drawing.Point(3, 78);
            this.bufferedFlowLayoutPanelTQITPath.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.bufferedFlowLayoutPanelTQITPath.Name = "bufferedFlowLayoutPanelTQITPath";
            this.bufferedFlowLayoutPanelTQITPath.Size = new System.Drawing.Size(456, 33);
            this.bufferedFlowLayoutPanelTQITPath.TabIndex = 55;
            // 
            // enableCustomMapsCheckBox
            // 
            this.enableCustomMapsCheckBox.Checked = false;
            this.enableCustomMapsCheckBox.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Enable;
            this.enableCustomMapsCheckBox.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this.enableCustomMapsCheckBox.Controls.Add(this.bufferedFlowLayoutPanelCustomMap);
            this.enableCustomMapsCheckBox.ForeColor = System.Drawing.Color.Gold;
            this.enableCustomMapsCheckBox.Location = new System.Drawing.Point(3, 207);
            this.enableCustomMapsCheckBox.Name = "enableCustomMapsCheckBox";
            this.enableCustomMapsCheckBox.Size = new System.Drawing.Size(466, 83);
            this.enableCustomMapsCheckBox.TabIndex = 55;
            this.enableCustomMapsCheckBox.TabStop = false;
            this.enableCustomMapsCheckBox.Text = "Enable Custom Maps";
            this.enableCustomMapsCheckBox.CheckedChanged += new System.EventHandler(this.EnableCustomMapsCheckBoxCheckedChanged);
            // 
            // bufferedFlowLayoutPanelCustomMap
            // 
            this.bufferedFlowLayoutPanelCustomMap.Controls.Add(this.customMapLabel);
            this.bufferedFlowLayoutPanelCustomMap.Controls.Add(this.mapListComboBox);
            this.bufferedFlowLayoutPanelCustomMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelCustomMap.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelCustomMap.Location = new System.Drawing.Point(3, 20);
            this.bufferedFlowLayoutPanelCustomMap.Name = "bufferedFlowLayoutPanelCustomMap";
            this.bufferedFlowLayoutPanelCustomMap.Size = new System.Drawing.Size(460, 60);
            this.bufferedFlowLayoutPanelCustomMap.TabIndex = 1;
            // 
            // bufferedTableLayoutPanelSkeleton
            // 
            this.bufferedTableLayoutPanelSkeleton.AutoSize = true;
            this.bufferedTableLayoutPanelSkeleton.ColumnCount = 5;
            this.bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.bufferedTableLayoutPanelSkeleton.Controls.Add(this.bufferedFlowLayoutPanelSkeletonRight, 4, 0);
            this.bufferedTableLayoutPanelSkeleton.Controls.Add(this.tableLayoutPanelButtons, 0, 1);
            this.bufferedTableLayoutPanelSkeleton.Controls.Add(this.bufferedFlowLayoutPanelSkeletonLeft, 0, 0);
            this.bufferedTableLayoutPanelSkeleton.Controls.Add(this.bufferedFlowLayoutPanelSkeletonCenter, 2, 0);
            this.bufferedTableLayoutPanelSkeleton.Location = new System.Drawing.Point(10, 25);
            this.bufferedTableLayoutPanelSkeleton.Margin = new System.Windows.Forms.Padding(0);
            this.bufferedTableLayoutPanelSkeleton.Name = "bufferedTableLayoutPanelSkeleton";
            this.bufferedTableLayoutPanelSkeleton.RowCount = 2;
            this.bufferedTableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.bufferedTableLayoutPanelSkeleton.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.bufferedTableLayoutPanelSkeleton.Size = new System.Drawing.Size(1172, 584);
            this.bufferedTableLayoutPanelSkeleton.TabIndex = 56;
            // 
            // bufferedFlowLayoutPanelSkeletonRight
            // 
            this.bufferedFlowLayoutPanelSkeletonRight.AutoSize = true;
            this.bufferedFlowLayoutPanelSkeletonRight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedFlowLayoutPanelSkeletonRight.Controls.Add(this.groupBoxGfxAndAudio);
            this.bufferedFlowLayoutPanelSkeletonRight.Controls.Add(this.checkGroupBoxOriginalTQSupport);
            this.bufferedFlowLayoutPanelSkeletonRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelSkeletonRight.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelSkeletonRight.Location = new System.Drawing.Point(839, 3);
            this.bufferedFlowLayoutPanelSkeletonRight.Name = "bufferedFlowLayoutPanelSkeletonRight";
            this.bufferedFlowLayoutPanelSkeletonRight.Size = new System.Drawing.Size(330, 536);
            this.bufferedFlowLayoutPanelSkeletonRight.TabIndex = 2;
            // 
            // bufferedFlowLayoutPanelSkeletonLeft
            // 
            this.bufferedFlowLayoutPanelSkeletonLeft.AutoSize = true;
            this.bufferedFlowLayoutPanelSkeletonLeft.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(this.vaultPathLabel);
            this.bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(this.bufferedFlowLayoutPanelVaultPath);
            this.bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(this.detectGamePathsCheckBox);
            this.bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(this.enableCustomMapsCheckBox);
            this.bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(this.detectLanguageCheckBox);
            this.bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(this.checkGroupBoxGitBackup);
            this.bufferedFlowLayoutPanelSkeletonLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelSkeletonLeft.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelSkeletonLeft.Location = new System.Drawing.Point(3, 3);
            this.bufferedFlowLayoutPanelSkeletonLeft.Name = "bufferedFlowLayoutPanelSkeletonLeft";
            this.bufferedFlowLayoutPanelSkeletonLeft.Size = new System.Drawing.Size(472, 536);
            this.bufferedFlowLayoutPanelSkeletonLeft.TabIndex = 0;
            // 
            // bufferedFlowLayoutPanelVaultPath
            // 
            this.bufferedFlowLayoutPanelVaultPath.AutoSize = true;
            this.bufferedFlowLayoutPanelVaultPath.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedFlowLayoutPanelVaultPath.Controls.Add(this.vaultPathTextBox);
            this.bufferedFlowLayoutPanelVaultPath.Controls.Add(this.vaultPathBrowseButton);
            this.bufferedFlowLayoutPanelVaultPath.Location = new System.Drawing.Point(3, 21);
            this.bufferedFlowLayoutPanelVaultPath.Name = "bufferedFlowLayoutPanelVaultPath";
            this.bufferedFlowLayoutPanelVaultPath.Size = new System.Drawing.Size(456, 33);
            this.bufferedFlowLayoutPanelVaultPath.TabIndex = 57;
            // 
            // bufferedFlowLayoutPanelSkeletonCenter
            // 
            this.bufferedFlowLayoutPanelSkeletonCenter.AutoSize = true;
            this.bufferedFlowLayoutPanelSkeletonCenter.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bufferedFlowLayoutPanelSkeletonCenter.Controls.Add(this.groupBoxGeneral);
            this.bufferedFlowLayoutPanelSkeletonCenter.Controls.Add(this.checkGroupBoxAllowCheats);
            this.bufferedFlowLayoutPanelSkeletonCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bufferedFlowLayoutPanelSkeletonCenter.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.bufferedFlowLayoutPanelSkeletonCenter.Location = new System.Drawing.Point(486, 3);
            this.bufferedFlowLayoutPanelSkeletonCenter.Name = "bufferedFlowLayoutPanelSkeletonCenter";
            this.bufferedFlowLayoutPanelSkeletonCenter.Size = new System.Drawing.Size(342, 536);
            this.bufferedFlowLayoutPanelSkeletonCenter.TabIndex = 1;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(31)))), ((int)(((byte)(21)))));
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(1192, 618);
            this.Controls.Add(this.bufferedTableLayoutPanelSkeleton);
            this.DrawCustomBorder = true;
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsDialog";
            this.Padding = new System.Windows.Forms.Padding(5, 5, 5, 0);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Settings";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SettingsDialogLoad);
            this.Shown += new System.EventHandler(this.SettingsDialog_Shown);
            this.Controls.SetChildIndex(this.bufferedTableLayoutPanelSkeleton, 0);
            this.checkGroupBoxGitBackup.ResumeLayout(false);
            this.checkGroupBoxGitBackup.PerformLayout();
            this.bufferedFlowLayoutPanelGitBackup.ResumeLayout(false);
            this.bufferedFlowLayoutPanelGitBackup.PerformLayout();
            this.checkGroupBoxOriginalTQSupport.ResumeLayout(false);
            this.checkGroupBoxOriginalTQSupport.PerformLayout();
            this.bufferedFlowLayoutPanelTQOriginalSupport.ResumeLayout(false);
            this.bufferedFlowLayoutPanelTQOriginalSupport.PerformLayout();
            this.tableLayoutPanelButtons.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ItemBGColorOpacityTrackBar)).EndInit();
            this.groupBoxGeneral.ResumeLayout(false);
            this.groupBoxGeneral.PerformLayout();
            this.bufferedFlowLayoutPanelGeneralSettings.ResumeLayout(false);
            this.bufferedFlowLayoutPanelGeneralSettings.PerformLayout();
            this.checkGroupBoxAllowCheats.ResumeLayout(false);
            this.checkGroupBoxAllowCheats.PerformLayout();
            this.bufferedFlowLayoutPanelCheats.ResumeLayout(false);
            this.bufferedFlowLayoutPanelCheats.PerformLayout();
            this.groupBoxGfxAndAudio.ResumeLayout(false);
            this.groupBoxGfxAndAudio.PerformLayout();
            this.bufferedFlowLayoutPanelGfxAndAudio.ResumeLayout(false);
            this.bufferedFlowLayoutPanelGfxAndAudio.PerformLayout();
            this.detectLanguageCheckBox.ResumeLayout(false);
            this.detectLanguageCheckBox.PerformLayout();
            this.bufferedFlowLayoutPanelLanguage.ResumeLayout(false);
            this.bufferedFlowLayoutPanelLanguage.PerformLayout();
            this.detectGamePathsCheckBox.ResumeLayout(false);
            this.detectGamePathsCheckBox.PerformLayout();
            this.bufferedFlowLayoutPanelAutoDetectGamePath.ResumeLayout(false);
            this.bufferedFlowLayoutPanelAutoDetectGamePath.PerformLayout();
            this.bufferedFlowLayoutPanelTQPath.ResumeLayout(false);
            this.bufferedFlowLayoutPanelTQPath.PerformLayout();
            this.bufferedFlowLayoutPanelTQITPath.ResumeLayout(false);
            this.bufferedFlowLayoutPanelTQITPath.PerformLayout();
            this.enableCustomMapsCheckBox.ResumeLayout(false);
            this.enableCustomMapsCheckBox.PerformLayout();
            this.bufferedFlowLayoutPanelCustomMap.ResumeLayout(false);
            this.bufferedFlowLayoutPanelCustomMap.PerformLayout();
            this.bufferedTableLayoutPanelSkeleton.ResumeLayout(false);
            this.bufferedTableLayoutPanelSkeleton.PerformLayout();
            this.bufferedFlowLayoutPanelSkeletonRight.ResumeLayout(false);
            this.bufferedFlowLayoutPanelSkeletonRight.PerformLayout();
            this.bufferedFlowLayoutPanelSkeletonLeft.ResumeLayout(false);
            this.bufferedFlowLayoutPanelSkeletonLeft.PerformLayout();
            this.bufferedFlowLayoutPanelVaultPath.ResumeLayout(false);
            this.bufferedFlowLayoutPanelVaultPath.PerformLayout();
            this.bufferedFlowLayoutPanelSkeletonCenter.ResumeLayout(false);
            this.bufferedFlowLayoutPanelSkeletonCenter.PerformLayout();
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
		private ScalingCheckBox EnableItemRequirementRestrictionCheckBox;
		private ScalingCheckBox hotReloadCheckBox;
		private ScalingCheckBox scalingCheckBoxEnableSounds;
        private ScalingLabel scalingLabelCSVDelim;
        private ScalingComboBox scalingComboBoxCSVDelim;
		private ScalingCheckBox scalingCheckBoxEnableEpicLegendaryAffixes;
		private ScalingCheckBox scalingCheckBoxDisableAutoStacking;
        private TQVaultAE.GUI.Components.CheckGroupBox checkGroupBoxGitBackup;
        private ScalingCheckBox scalingCheckBoxDisableLegacyBackup;
        private ScalingTextBox scalingTextBoxGitRepository;
        private ScalingLabel scalingLabelGitRepository;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelGitBackup;
        private System.Windows.Forms.GroupBox groupBoxGeneral;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelGeneralSettings;
        private TQVaultAE.GUI.Components.CheckGroupBox checkGroupBoxAllowCheats;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelCheats;
        private System.Windows.Forms.GroupBox groupBoxGfxAndAudio;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelGfxAndAudio;
        private TQVaultAE.GUI.Components.CheckGroupBox detectLanguageCheckBox;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelLanguage;
        private TQVaultAE.GUI.Components.CheckGroupBox detectGamePathsCheckBox;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelAutoDetectGamePath;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelTQPath;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelTQITPath;
        private TQVaultAE.GUI.Components.CheckGroupBox enableCustomMapsCheckBox;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelCustomMap;
        private BufferedTableLayoutPanel bufferedTableLayoutPanelSkeleton;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelSkeletonRight;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelSkeletonLeft;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelVaultPath;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelSkeletonCenter;
		private System.Windows.Forms.OpenFileDialog openFileDialog;
        private ScalingCheckBox scalingCheckBoxBackupPlayerSaves;
        private CheckGroupBox checkGroupBoxOriginalTQSupport;
        private BufferedFlowLayoutPanel bufferedFlowLayoutPanelTQOriginalSupport;
        private System.Windows.Forms.LinkLabel linkLabelTQOriginalSupport;
    }
}