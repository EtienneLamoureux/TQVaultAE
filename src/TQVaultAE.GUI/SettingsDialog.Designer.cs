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
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsDialog));
			allowItemEditCheckBox = new ScalingCheckBox();
			allowItemCopyCheckBox = new ScalingCheckBox();
			skipTitleCheckBox = new ScalingCheckBox();
			loadLastCharacterCheckBox = new ScalingCheckBox();
			loadLastVaultCheckBox = new ScalingCheckBox();
			vaultPathTextBox = new ScalingTextBox();
			vaultPathLabel = new ScalingLabel();
			cancelButton = new ScalingButton();
			okayButton = new ScalingButton();
			folderBrowserDialog = new FolderBrowserDialog();
			resetButton = new ScalingButton();
			vaultPathBrowseButton = new ScalingButton();
			toolTip = new ToolTip(components);
			loadAllFilesCheckBox = new ScalingCheckBox();
			suppressWarningsCheckBox = new ScalingCheckBox();
			playerReadonlyCheckbox = new ScalingCheckBox();
			characterEditCheckBox = new ScalingCheckBox();
			EnableDetailedTooltipViewCheckBox = new ScalingCheckBox();
			ItemBGColorOpacityLabel = new ScalingLabel();
			EnableItemRequirementRestrictionCheckBox = new ScalingCheckBox();
			hotReloadCheckBox = new ScalingCheckBox();
			scalingCheckBoxEnableEpicLegendaryAffixes = new ScalingCheckBox();
			scalingCheckBoxDisableAutoStacking = new ScalingCheckBox();
			checkGroupBoxGitBackup = new CheckGroupBox();
			bufferedFlowLayoutPanelGitBackup = new BufferedFlowLayoutPanel();
			scalingCheckBoxDisableLegacyBackup = new ScalingCheckBox();
			scalingCheckBoxBackupPlayerSaves = new ScalingCheckBox();
			scalingLabelGitRepository = new ScalingLabel();
			scalingTextBoxGitRepository = new ScalingTextBox();
			checkGroupBoxOriginalTQSupport = new CheckGroupBox();
			bufferedFlowLayoutPanelTQOriginalSupport = new BufferedFlowLayoutPanel();
			linkLabelTQOriginalSupport = new LinkLabel();
			languageComboBox = new ScalingComboBox();
			languageLabel = new ScalingLabel();
			titanQuestPathTextBox = new ScalingTextBox();
			titanQuestPathLabel = new ScalingLabel();
			immortalThronePathLabel = new ScalingLabel();
			immortalThronePathTextBox = new ScalingTextBox();
			titanQuestPathBrowseButton = new ScalingButton();
			immortalThronePathBrowseButton = new ScalingButton();
			customMapLabel = new ScalingLabel();
			mapListComboBox = new ScalingComboBox();
			baseFontLabel = new ScalingLabel();
			baseFontComboBox = new ScalingComboBox();
			tableLayoutPanelButtons = new TableLayoutPanel();
			ItemBGColorOpacityTrackBar = new TrackBar();
			scalingCheckBoxEnableSounds = new ScalingCheckBox();
			scalingLabelCSVDelim = new ScalingLabel();
			scalingComboBoxCSVDelim = new ScalingComboBox();
			groupBoxGeneral = new GroupBox();
			bufferedFlowLayoutPanelGeneralSettings = new BufferedFlowLayoutPanel();
			checkGroupBoxAllowCheats = new CheckGroupBox();
			bufferedFlowLayoutPanelCheats = new BufferedFlowLayoutPanel();
			groupBoxGfxAndAudio = new GroupBox();
			bufferedFlowLayoutPanelGfxAndAudio = new BufferedFlowLayoutPanel();
			detectLanguageCheckBox = new CheckGroupBox();
			bufferedFlowLayoutPanelLanguage = new BufferedFlowLayoutPanel();
			detectGamePathsCheckBox = new CheckGroupBox();
			bufferedFlowLayoutPanelAutoDetectGamePath = new BufferedFlowLayoutPanel();
			bufferedFlowLayoutPanelTQPath = new BufferedFlowLayoutPanel();
			bufferedFlowLayoutPanelTQITPath = new BufferedFlowLayoutPanel();
			enableCustomMapsCheckBox = new CheckGroupBox();
			bufferedFlowLayoutPanelCustomMap = new BufferedFlowLayoutPanel();
			bufferedTableLayoutPanelSkeleton = new BufferedTableLayoutPanel();
			bufferedFlowLayoutPanelSkeletonRight = new BufferedFlowLayoutPanel();
			pasteBinGroupBox = new GroupBox();
			pasteBinFlowPanel = new BufferedTableLayoutPanel();
			pasteBinExpirationComboBox = new ScalingComboBox();
			pasteBinApiKeyLabel = new ScalingLabel();
			pasteBinApiKeyTextBox = new ScalingTextBox();
			pasteBinExpirationLabel = new ScalingLabel();
			bufferedFlowLayoutPanelSkeletonLeft = new BufferedFlowLayoutPanel();
			bufferedFlowLayoutPanelVaultPath = new BufferedFlowLayoutPanel();
			bufferedFlowLayoutPanelSkeletonCenter = new BufferedFlowLayoutPanel();
			openFileDialog = new OpenFileDialog();
			checkGroupBoxGitBackup.SuspendLayout();
			bufferedFlowLayoutPanelGitBackup.SuspendLayout();
			checkGroupBoxOriginalTQSupport.SuspendLayout();
			bufferedFlowLayoutPanelTQOriginalSupport.SuspendLayout();
			tableLayoutPanelButtons.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)ItemBGColorOpacityTrackBar).BeginInit();
			groupBoxGeneral.SuspendLayout();
			bufferedFlowLayoutPanelGeneralSettings.SuspendLayout();
			checkGroupBoxAllowCheats.SuspendLayout();
			bufferedFlowLayoutPanelCheats.SuspendLayout();
			groupBoxGfxAndAudio.SuspendLayout();
			bufferedFlowLayoutPanelGfxAndAudio.SuspendLayout();
			detectLanguageCheckBox.SuspendLayout();
			bufferedFlowLayoutPanelLanguage.SuspendLayout();
			detectGamePathsCheckBox.SuspendLayout();
			bufferedFlowLayoutPanelAutoDetectGamePath.SuspendLayout();
			bufferedFlowLayoutPanelTQPath.SuspendLayout();
			bufferedFlowLayoutPanelTQITPath.SuspendLayout();
			enableCustomMapsCheckBox.SuspendLayout();
			bufferedFlowLayoutPanelCustomMap.SuspendLayout();
			bufferedTableLayoutPanelSkeleton.SuspendLayout();
			bufferedFlowLayoutPanelSkeletonRight.SuspendLayout();
			pasteBinGroupBox.SuspendLayout();
			pasteBinFlowPanel.SuspendLayout();
			bufferedFlowLayoutPanelSkeletonLeft.SuspendLayout();
			bufferedFlowLayoutPanelVaultPath.SuspendLayout();
			bufferedFlowLayoutPanelSkeletonCenter.SuspendLayout();
			SuspendLayout();
			// 
			// allowItemEditCheckBox
			// 
			allowItemEditCheckBox.AutoSize = true;
			allowItemEditCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			allowItemEditCheckBox.ForeColor = Color.White;
			allowItemEditCheckBox.Location = new Point(3, 59);
			allowItemEditCheckBox.Name = "allowItemEditCheckBox";
			allowItemEditCheckBox.Size = new Size(204, 22);
			allowItemEditCheckBox.TabIndex = 3;
			allowItemEditCheckBox.Text = "Allow Item Editing Features";
			toolTip.SetToolTip(allowItemEditCheckBox, "Turns on the editing features in the context menu.\r\nThese include item creation and modification.");
			allowItemEditCheckBox.UseVisualStyleBackColor = true;
			allowItemEditCheckBox.CheckedChanged += AllowItemEditCheckBoxCheckedChanged;
			// 
			// allowItemCopyCheckBox
			// 
			allowItemCopyCheckBox.AutoSize = true;
			allowItemCopyCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			allowItemCopyCheckBox.ForeColor = Color.White;
			allowItemCopyCheckBox.Location = new Point(3, 31);
			allowItemCopyCheckBox.Name = "allowItemCopyCheckBox";
			allowItemCopyCheckBox.Size = new Size(152, 22);
			allowItemCopyCheckBox.TabIndex = 4;
			allowItemCopyCheckBox.Text = "Allow Item Copying";
			toolTip.SetToolTip(allowItemCopyCheckBox, "Enables copy selection in the context menu.");
			allowItemCopyCheckBox.UseVisualStyleBackColor = true;
			allowItemCopyCheckBox.CheckedChanged += AllowItemCopyCheckBoxCheckedChanged;
			// 
			// skipTitleCheckBox
			// 
			skipTitleCheckBox.AutoSize = true;
			skipTitleCheckBox.Checked = true;
			skipTitleCheckBox.CheckState = CheckState.Checked;
			skipTitleCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			skipTitleCheckBox.ForeColor = Color.White;
			skipTitleCheckBox.Location = new Point(3, 3);
			skipTitleCheckBox.Name = "skipTitleCheckBox";
			skipTitleCheckBox.Size = new Size(249, 22);
			skipTitleCheckBox.TabIndex = 2;
			skipTitleCheckBox.Text = "Automatically Bypass Title Screen";
			toolTip.SetToolTip(skipTitleCheckBox, "Ticking this box will automatically hit\r\nthe Enter key on the title screen.");
			skipTitleCheckBox.UseVisualStyleBackColor = true;
			skipTitleCheckBox.CheckedChanged += SkipTitleCheckBoxCheckedChanged;
			// 
			// loadLastCharacterCheckBox
			// 
			loadLastCharacterCheckBox.AutoSize = true;
			loadLastCharacterCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			loadLastCharacterCheckBox.ForeColor = Color.White;
			loadLastCharacterCheckBox.Location = new Point(3, 31);
			loadLastCharacterCheckBox.Name = "loadLastCharacterCheckBox";
			loadLastCharacterCheckBox.Size = new Size(324, 22);
			loadLastCharacterCheckBox.TabIndex = 5;
			loadLastCharacterCheckBox.Text = "Automatically Load the last opened Character";
			toolTip.SetToolTip(loadLastCharacterCheckBox, "Selecting this option will automatically load\r\nthe last open character when TQVault was closed.");
			loadLastCharacterCheckBox.UseVisualStyleBackColor = true;
			loadLastCharacterCheckBox.CheckedChanged += LoadLastCharacterCheckBoxCheckedChanged;
			// 
			// loadLastVaultCheckBox
			// 
			loadLastVaultCheckBox.AutoSize = true;
			loadLastVaultCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			loadLastVaultCheckBox.ForeColor = Color.White;
			loadLastVaultCheckBox.Location = new Point(3, 59);
			loadLastVaultCheckBox.Name = "loadLastVaultCheckBox";
			loadLastVaultCheckBox.Size = new Size(291, 22);
			loadLastVaultCheckBox.TabIndex = 6;
			loadLastVaultCheckBox.Text = "Automatically Load the last opened Vault";
			toolTip.SetToolTip(loadLastVaultCheckBox, "Selecting this item will automatically load the\r\nlast opened vault when TQVault was closed.\r\nTQVault will automatically open Main Vault\r\nif nothing is chosen.");
			loadLastVaultCheckBox.UseVisualStyleBackColor = true;
			loadLastVaultCheckBox.CheckedChanged += LoadLastVaultCheckBoxCheckedChanged;
			// 
			// vaultPathTextBox
			// 
			vaultPathTextBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			vaultPathTextBox.Location = new Point(3, 3);
			vaultPathTextBox.Name = "vaultPathTextBox";
			vaultPathTextBox.Size = new Size(397, 24);
			vaultPathTextBox.TabIndex = 0;
			toolTip.SetToolTip(vaultPathTextBox, resources.GetString("vaultPathTextBox.ToolTip"));
			vaultPathTextBox.Leave += VaultPathTextBoxLeave;
			// 
			// vaultPathLabel
			// 
			vaultPathLabel.AutoSize = true;
			vaultPathLabel.Font = new Font("Microsoft Sans Serif", 11.25F);
			vaultPathLabel.ForeColor = Color.Gold;
			vaultPathLabel.Location = new Point(3, 0);
			vaultPathLabel.Name = "vaultPathLabel";
			vaultPathLabel.Size = new Size(74, 18);
			vaultPathLabel.TabIndex = 14;
			vaultPathLabel.Text = "Vault Path";
			// 
			// cancelButton
			// 
			cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			cancelButton.BackColor = Color.Transparent;
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.DownBitmap = (Bitmap)resources.GetObject("cancelButton.DownBitmap");
			cancelButton.FlatAppearance.BorderSize = 0;
			cancelButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			cancelButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			cancelButton.FlatStyle = FlatStyle.Flat;
			cancelButton.Font = new Font("Microsoft Sans Serif", 12F);
			cancelButton.ForeColor = Color.FromArgb(51, 44, 28);
			cancelButton.Image = (Image)resources.GetObject("cancelButton.Image");
			cancelButton.Location = new Point(609, 3);
			cancelButton.Name = "cancelButton";
			cancelButton.OverBitmap = (Bitmap)resources.GetObject("cancelButton.OverBitmap");
			cancelButton.Size = new Size(137, 30);
			cancelButton.SizeToGraphic = false;
			cancelButton.TabIndex = 13;
			cancelButton.Text = "Cancel";
			cancelButton.UpBitmap = (Bitmap)resources.GetObject("cancelButton.UpBitmap");
			cancelButton.UseCustomGraphic = true;
			cancelButton.UseVisualStyleBackColor = false;
			// 
			// okayButton
			// 
			okayButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			okayButton.BackColor = Color.Transparent;
			okayButton.DialogResult = DialogResult.OK;
			okayButton.DownBitmap = (Bitmap)resources.GetObject("okayButton.DownBitmap");
			okayButton.FlatAppearance.BorderSize = 0;
			okayButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			okayButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			okayButton.FlatStyle = FlatStyle.Flat;
			okayButton.Font = new Font("Microsoft Sans Serif", 12F);
			okayButton.ForeColor = Color.FromArgb(51, 44, 28);
			okayButton.Image = (Image)resources.GetObject("okayButton.Image");
			okayButton.Location = new Point(446, 3);
			okayButton.Name = "okayButton";
			okayButton.OverBitmap = (Bitmap)resources.GetObject("okayButton.OverBitmap");
			okayButton.Size = new Size(137, 30);
			okayButton.SizeToGraphic = false;
			okayButton.TabIndex = 12;
			okayButton.Text = "OK";
			okayButton.UpBitmap = (Bitmap)resources.GetObject("okayButton.UpBitmap");
			okayButton.UseCustomGraphic = true;
			okayButton.UseVisualStyleBackColor = false;
			okayButton.Click += OkayButtonClick;
			// 
			// folderBrowserDialog
			// 
			folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
			// 
			// resetButton
			// 
			resetButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			resetButton.BackColor = Color.Transparent;
			resetButton.DownBitmap = (Bitmap)resources.GetObject("resetButton.DownBitmap");
			resetButton.FlatAppearance.BorderSize = 0;
			resetButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			resetButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			resetButton.FlatStyle = FlatStyle.Flat;
			resetButton.Font = new Font("Microsoft Sans Serif", 12F);
			resetButton.ForeColor = Color.FromArgb(51, 44, 28);
			resetButton.Image = (Image)resources.GetObject("resetButton.Image");
			resetButton.Location = new Point(1052, 3);
			resetButton.Name = "resetButton";
			resetButton.OverBitmap = (Bitmap)resources.GetObject("resetButton.OverBitmap");
			resetButton.Size = new Size(137, 30);
			resetButton.SizeToGraphic = false;
			resetButton.TabIndex = 11;
			resetButton.Text = "Reset";
			toolTip.SetToolTip(resetButton, "Causes the configuration to Reset to the\r\nlast saved configuration.");
			resetButton.UpBitmap = (Bitmap)resources.GetObject("resetButton.UpBitmap");
			resetButton.UseCustomGraphic = true;
			resetButton.UseVisualStyleBackColor = false;
			resetButton.Click += ResetButtonClick;
			// 
			// vaultPathBrowseButton
			// 
			vaultPathBrowseButton.BackColor = Color.Transparent;
			vaultPathBrowseButton.DownBitmap = (Bitmap)resources.GetObject("vaultPathBrowseButton.DownBitmap");
			vaultPathBrowseButton.FlatAppearance.BorderSize = 0;
			vaultPathBrowseButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			vaultPathBrowseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			vaultPathBrowseButton.FlatStyle = FlatStyle.Flat;
			vaultPathBrowseButton.Font = new Font("Microsoft Sans Serif", 12F);
			vaultPathBrowseButton.ForeColor = Color.FromArgb(51, 44, 28);
			vaultPathBrowseButton.Image = (Image)resources.GetObject("vaultPathBrowseButton.Image");
			vaultPathBrowseButton.Location = new Point(406, 0);
			vaultPathBrowseButton.Margin = new Padding(3, 0, 3, 3);
			vaultPathBrowseButton.Name = "vaultPathBrowseButton";
			vaultPathBrowseButton.OverBitmap = (Bitmap)resources.GetObject("vaultPathBrowseButton.OverBitmap");
			vaultPathBrowseButton.Size = new Size(47, 30);
			vaultPathBrowseButton.SizeToGraphic = false;
			vaultPathBrowseButton.TabIndex = 1;
			vaultPathBrowseButton.Text = "...";
			vaultPathBrowseButton.UpBitmap = (Bitmap)resources.GetObject("vaultPathBrowseButton.UpBitmap");
			vaultPathBrowseButton.UseCustomGraphic = true;
			vaultPathBrowseButton.UseVisualStyleBackColor = false;
			vaultPathBrowseButton.Click += VaultPathBrowseButtonClick;
			// 
			// loadAllFilesCheckBox
			// 
			loadAllFilesCheckBox.AutoSize = true;
			loadAllFilesCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			loadAllFilesCheckBox.ForeColor = Color.White;
			loadAllFilesCheckBox.Location = new Point(3, 87);
			loadAllFilesCheckBox.Name = "loadAllFilesCheckBox";
			loadAllFilesCheckBox.Size = new Size(276, 22);
			loadAllFilesCheckBox.TabIndex = 28;
			loadAllFilesCheckBox.Text = "Pre-Load All Vault And Character Files";
			toolTip.SetToolTip(loadAllFilesCheckBox, "Selecting this item will automatically load all\r\nof the available character, stash and vault files\r\non startup.  This aids the search function, but\r\nincreases startup time.");
			loadAllFilesCheckBox.UseVisualStyleBackColor = true;
			loadAllFilesCheckBox.CheckedChanged += LoadAllFilesCheckBoxCheckedChanged;
			// 
			// suppressWarningsCheckBox
			// 
			suppressWarningsCheckBox.AutoSize = true;
			suppressWarningsCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			suppressWarningsCheckBox.ForeColor = Color.White;
			suppressWarningsCheckBox.Location = new Point(3, 141);
			suppressWarningsCheckBox.Name = "suppressWarningsCheckBox";
			suppressWarningsCheckBox.Size = new Size(238, 22);
			suppressWarningsCheckBox.TabIndex = 30;
			suppressWarningsCheckBox.Text = "Bypass Confirmation Messages";
			toolTip.SetToolTip(suppressWarningsCheckBox, "When enabled, confirmation messages will no\r\nlonger be shown for item deletion and\r\nrelic removal or if there are items in the trash\r\nwhen TQVault is closed.");
			suppressWarningsCheckBox.UseVisualStyleBackColor = true;
			suppressWarningsCheckBox.CheckedChanged += SuppressWarningsCheckBoxCheckedChanged;
			// 
			// playerReadonlyCheckbox
			// 
			playerReadonlyCheckbox.AutoSize = true;
			playerReadonlyCheckbox.Font = new Font("Microsoft Sans Serif", 11.25F);
			playerReadonlyCheckbox.ForeColor = Color.White;
			playerReadonlyCheckbox.Location = new Point(3, 197);
			playerReadonlyCheckbox.Name = "playerReadonlyCheckbox";
			playerReadonlyCheckbox.Size = new Size(211, 22);
			playerReadonlyCheckbox.TabIndex = 33;
			playerReadonlyCheckbox.Text = "Player Equipment ReadOnly";
			toolTip.SetToolTip(playerReadonlyCheckbox, "Avoid save game corruption that occurs (randomly). When enabled, player equipment will be read-only,  you won't be able to select or move any item.");
			playerReadonlyCheckbox.UseVisualStyleBackColor = true;
			playerReadonlyCheckbox.CheckedChanged += PlayerReadonlyCheckboxCheckedChanged;
			// 
			// characterEditCheckBox
			// 
			characterEditCheckBox.AutoSize = true;
			characterEditCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			characterEditCheckBox.ForeColor = Color.White;
			characterEditCheckBox.Location = new Point(3, 3);
			characterEditCheckBox.Name = "characterEditCheckBox";
			characterEditCheckBox.Size = new Size(241, 22);
			characterEditCheckBox.TabIndex = 34;
			characterEditCheckBox.Text = "Allow Character Editing Features";
			toolTip.SetToolTip(characterEditCheckBox, "Turns on the editing features in the context menu.\r\nThese include item creation and modification.");
			characterEditCheckBox.UseVisualStyleBackColor = true;
			characterEditCheckBox.CheckedChanged += CharacterEditCheckBox_CheckedChanged;
			// 
			// EnableDetailedTooltipViewCheckBox
			// 
			EnableDetailedTooltipViewCheckBox.AutoSize = true;
			EnableDetailedTooltipViewCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			EnableDetailedTooltipViewCheckBox.ForeColor = Color.White;
			EnableDetailedTooltipViewCheckBox.Location = new Point(3, 169);
			EnableDetailedTooltipViewCheckBox.Name = "EnableDetailedTooltipViewCheckBox";
			EnableDetailedTooltipViewCheckBox.Size = new Size(213, 22);
			EnableDetailedTooltipViewCheckBox.TabIndex = 38;
			EnableDetailedTooltipViewCheckBox.Text = "Enable Detailed Tooltip View";
			toolTip.SetToolTip(EnableDetailedTooltipViewCheckBox, "Split tooltip attributes into Prefix/Base/Suffix categories");
			EnableDetailedTooltipViewCheckBox.UseVisualStyleBackColor = true;
			EnableDetailedTooltipViewCheckBox.CheckedChanged += EnableDetailedTooltipViewCheckBox_CheckedChanged;
			// 
			// ItemBGColorOpacityLabel
			// 
			ItemBGColorOpacityLabel.AutoSize = true;
			ItemBGColorOpacityLabel.Font = new Font("Microsoft Sans Serif", 11.25F);
			ItemBGColorOpacityLabel.ForeColor = Color.White;
			ItemBGColorOpacityLabel.Location = new Point(3, 105);
			ItemBGColorOpacityLabel.Name = "ItemBGColorOpacityLabel";
			ItemBGColorOpacityLabel.Size = new Size(151, 18);
			ItemBGColorOpacityLabel.TabIndex = 40;
			ItemBGColorOpacityLabel.Text = "Item BG Alpha Color :";
			toolTip.SetToolTip(ItemBGColorOpacityLabel, "Item background color opacity level");
			// 
			// EnableItemRequirementRestrictionCheckBox
			// 
			EnableItemRequirementRestrictionCheckBox.AutoSize = true;
			EnableItemRequirementRestrictionCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			EnableItemRequirementRestrictionCheckBox.ForeColor = Color.Orange;
			EnableItemRequirementRestrictionCheckBox.Location = new Point(3, 80);
			EnableItemRequirementRestrictionCheckBox.Name = "EnableItemRequirementRestrictionCheckBox";
			EnableItemRequirementRestrictionCheckBox.Size = new Size(304, 22);
			EnableItemRequirementRestrictionCheckBox.TabIndex = 41;
			EnableItemRequirementRestrictionCheckBox.Text = "Enable Character Requierement BG Color";
			toolTip.SetToolTip(EnableItemRequirementRestrictionCheckBox, resources.GetString("EnableItemRequirementRestrictionCheckBox.ToolTip"));
			EnableItemRequirementRestrictionCheckBox.UseVisualStyleBackColor = true;
			EnableItemRequirementRestrictionCheckBox.CheckedChanged += EnableItemRequirementRestrictionCheckBox_CheckedChanged;
			// 
			// hotReloadCheckBox
			// 
			hotReloadCheckBox.AutoSize = true;
			hotReloadCheckBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			hotReloadCheckBox.ForeColor = Color.White;
			hotReloadCheckBox.Location = new Point(2, 114);
			hotReloadCheckBox.Margin = new Padding(2);
			hotReloadCheckBox.Name = "hotReloadCheckBox";
			hotReloadCheckBox.Size = new Size(203, 22);
			hotReloadCheckBox.TabIndex = 42;
			hotReloadCheckBox.Text = "Allow Hot Reload Features";
			toolTip.SetToolTip(hotReloadCheckBox, "Turns on the editing features in the context menu.\r\nThese include item creation and modification.");
			hotReloadCheckBox.UseVisualStyleBackColor = true;
			hotReloadCheckBox.CheckedChanged += hotReloadCheckBox_CheckedChanged;
			// 
			// scalingCheckBoxEnableEpicLegendaryAffixes
			// 
			scalingCheckBoxEnableEpicLegendaryAffixes.AutoSize = true;
			scalingCheckBoxEnableEpicLegendaryAffixes.Enabled = false;
			scalingCheckBoxEnableEpicLegendaryAffixes.Font = new Font("Microsoft Sans Serif", 11.25F);
			scalingCheckBoxEnableEpicLegendaryAffixes.ForeColor = Color.White;
			scalingCheckBoxEnableEpicLegendaryAffixes.Location = new Point(3, 87);
			scalingCheckBoxEnableEpicLegendaryAffixes.Name = "scalingCheckBoxEnableEpicLegendaryAffixes";
			scalingCheckBoxEnableEpicLegendaryAffixes.Size = new Size(251, 22);
			scalingCheckBoxEnableEpicLegendaryAffixes.TabIndex = 46;
			scalingCheckBoxEnableEpicLegendaryAffixes.Text = "Enable Epic and Legendary affixes";
			toolTip.SetToolTip(scalingCheckBoxEnableEpicLegendaryAffixes, "Allow affixes pickup on Epic and Legendary items");
			scalingCheckBoxEnableEpicLegendaryAffixes.UseVisualStyleBackColor = true;
			scalingCheckBoxEnableEpicLegendaryAffixes.CheckedChanged += scalingCheckBoxEnableEpicLegendaryAffixes_CheckedChanged;
			// 
			// scalingCheckBoxDisableAutoStacking
			// 
			scalingCheckBoxDisableAutoStacking.AutoSize = true;
			scalingCheckBoxDisableAutoStacking.Font = new Font("Microsoft Sans Serif", 11.25F);
			scalingCheckBoxDisableAutoStacking.ForeColor = Color.White;
			scalingCheckBoxDisableAutoStacking.Location = new Point(3, 225);
			scalingCheckBoxDisableAutoStacking.Name = "scalingCheckBoxDisableAutoStacking";
			scalingCheckBoxDisableAutoStacking.Size = new Size(168, 22);
			scalingCheckBoxDisableAutoStacking.TabIndex = 47;
			scalingCheckBoxDisableAutoStacking.Text = "Disable auto stacking";
			toolTip.SetToolTip(scalingCheckBoxDisableAutoStacking, "Disable auto stacking for relic, charms and potions");
			scalingCheckBoxDisableAutoStacking.UseVisualStyleBackColor = true;
			scalingCheckBoxDisableAutoStacking.CheckedChanged += scalingCheckBoxDisableAutoStacking_CheckedChanged;
			// 
			// checkGroupBoxGitBackup
			// 
			checkGroupBoxGitBackup.AutoSize = true;
			checkGroupBoxGitBackup.Checked = false;
			checkGroupBoxGitBackup.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Enable;
			checkGroupBoxGitBackup.CheckState = CheckState.Unchecked;
			checkGroupBoxGitBackup.Controls.Add(bufferedFlowLayoutPanelGitBackup);
			checkGroupBoxGitBackup.ForeColor = Color.Gold;
			checkGroupBoxGitBackup.Location = new Point(3, 374);
			checkGroupBoxGitBackup.Name = "checkGroupBoxGitBackup";
			checkGroupBoxGitBackup.Size = new Size(409, 127);
			checkGroupBoxGitBackup.TabIndex = 48;
			checkGroupBoxGitBackup.TabStop = false;
			checkGroupBoxGitBackup.Text = "Git Backup";
			toolTip.SetToolTip(checkGroupBoxGitBackup, "Enable a backup of your modified vaults & saves to a a git repository");
			checkGroupBoxGitBackup.CheckedChanged += checkGroupBoxGitBackup_CheckedChanged;
			// 
			// bufferedFlowLayoutPanelGitBackup
			// 
			bufferedFlowLayoutPanelGitBackup.AutoSize = true;
			bufferedFlowLayoutPanelGitBackup.Controls.Add(scalingCheckBoxDisableLegacyBackup);
			bufferedFlowLayoutPanelGitBackup.Controls.Add(scalingCheckBoxBackupPlayerSaves);
			bufferedFlowLayoutPanelGitBackup.Controls.Add(scalingLabelGitRepository);
			bufferedFlowLayoutPanelGitBackup.Controls.Add(scalingTextBoxGitRepository);
			bufferedFlowLayoutPanelGitBackup.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelGitBackup.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelGitBackup.Location = new Point(3, 20);
			bufferedFlowLayoutPanelGitBackup.Name = "bufferedFlowLayoutPanelGitBackup";
			bufferedFlowLayoutPanelGitBackup.Size = new Size(403, 104);
			bufferedFlowLayoutPanelGitBackup.TabIndex = 1;
			// 
			// scalingCheckBoxDisableLegacyBackup
			// 
			scalingCheckBoxDisableLegacyBackup.AutoSize = true;
			scalingCheckBoxDisableLegacyBackup.Font = new Font("Microsoft Sans Serif", 11.25F);
			scalingCheckBoxDisableLegacyBackup.ForeColor = Color.White;
			scalingCheckBoxDisableLegacyBackup.Location = new Point(3, 3);
			scalingCheckBoxDisableLegacyBackup.Name = "scalingCheckBoxDisableLegacyBackup";
			scalingCheckBoxDisableLegacyBackup.Size = new Size(174, 22);
			scalingCheckBoxDisableLegacyBackup.TabIndex = 1;
			scalingCheckBoxDisableLegacyBackup.Text = "Disable legacy backup";
			toolTip.SetToolTip(scalingCheckBoxDisableLegacyBackup, "Optionaly disable the old backup system");
			scalingCheckBoxDisableLegacyBackup.UseVisualStyleBackColor = true;
			scalingCheckBoxDisableLegacyBackup.CheckedChanged += scalingCheckBoxDisableLegacyBackup_CheckedChanged;
			// 
			// scalingCheckBoxBackupPlayerSaves
			// 
			scalingCheckBoxBackupPlayerSaves.AutoSize = true;
			scalingCheckBoxBackupPlayerSaves.Font = new Font("Microsoft Sans Serif", 11.25F);
			scalingCheckBoxBackupPlayerSaves.ForeColor = Color.White;
			scalingCheckBoxBackupPlayerSaves.Location = new Point(3, 31);
			scalingCheckBoxBackupPlayerSaves.Name = "scalingCheckBoxBackupPlayerSaves";
			scalingCheckBoxBackupPlayerSaves.Size = new Size(163, 22);
			scalingCheckBoxBackupPlayerSaves.TabIndex = 6;
			scalingCheckBoxBackupPlayerSaves.Text = "Backup player saves";
			toolTip.SetToolTip(scalingCheckBoxBackupPlayerSaves, "Include your character save files in the backup");
			scalingCheckBoxBackupPlayerSaves.UseVisualStyleBackColor = true;
			scalingCheckBoxBackupPlayerSaves.CheckedChanged += scalingCheckBoxBackupPlayerSaves_CheckedChanged;
			// 
			// scalingLabelGitRepository
			// 
			scalingLabelGitRepository.AutoSize = true;
			scalingLabelGitRepository.Font = new Font("Microsoft Sans Serif", 11.25F);
			scalingLabelGitRepository.ForeColor = Color.White;
			scalingLabelGitRepository.Location = new Point(3, 56);
			scalingLabelGitRepository.Name = "scalingLabelGitRepository";
			scalingLabelGitRepository.Size = new Size(138, 18);
			scalingLabelGitRepository.TabIndex = 5;
			scalingLabelGitRepository.Text = "Git Repository Url : ";
			// 
			// scalingTextBoxGitRepository
			// 
			scalingTextBoxGitRepository.Font = new Font("Microsoft Sans Serif", 11.25F);
			scalingTextBoxGitRepository.Location = new Point(3, 77);
			scalingTextBoxGitRepository.Name = "scalingTextBoxGitRepository";
			scalingTextBoxGitRepository.Size = new Size(397, 24);
			scalingTextBoxGitRepository.TabIndex = 4;
			scalingTextBoxGitRepository.TextChanged += scalingTextBoxGitRepository_TextChanged;
			// 
			// checkGroupBoxOriginalTQSupport
			// 
			checkGroupBoxOriginalTQSupport.Checked = false;
			checkGroupBoxOriginalTQSupport.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Enable;
			checkGroupBoxOriginalTQSupport.CheckState = CheckState.Unchecked;
			checkGroupBoxOriginalTQSupport.Controls.Add(bufferedFlowLayoutPanelTQOriginalSupport);
			checkGroupBoxOriginalTQSupport.ForeColor = Color.Gold;
			checkGroupBoxOriginalTQSupport.Location = new Point(3, 331);
			checkGroupBoxOriginalTQSupport.Name = "checkGroupBoxOriginalTQSupport";
			checkGroupBoxOriginalTQSupport.Size = new Size(316, 49);
			checkGroupBoxOriginalTQSupport.TabIndex = 53;
			checkGroupBoxOriginalTQSupport.TabStop = false;
			checkGroupBoxOriginalTQSupport.Text = "TQ original support";
			toolTip.SetToolTip(checkGroupBoxOriginalTQSupport, "Provide support for original Titan Quest character file editing");
			checkGroupBoxOriginalTQSupport.CheckedChanged += checkGroupBoxOriginalTQSupport_CheckedChanged;
			// 
			// bufferedFlowLayoutPanelTQOriginalSupport
			// 
			bufferedFlowLayoutPanelTQOriginalSupport.Controls.Add(linkLabelTQOriginalSupport);
			bufferedFlowLayoutPanelTQOriginalSupport.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelTQOriginalSupport.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelTQOriginalSupport.ForeColor = Color.White;
			bufferedFlowLayoutPanelTQOriginalSupport.Location = new Point(3, 20);
			bufferedFlowLayoutPanelTQOriginalSupport.Name = "bufferedFlowLayoutPanelTQOriginalSupport";
			bufferedFlowLayoutPanelTQOriginalSupport.Size = new Size(310, 26);
			bufferedFlowLayoutPanelTQOriginalSupport.TabIndex = 1;
			// 
			// linkLabelTQOriginalSupport
			// 
			linkLabelTQOriginalSupport.AutoSize = true;
			linkLabelTQOriginalSupport.LinkColor = Color.Magenta;
			linkLabelTQOriginalSupport.Location = new Point(3, 0);
			linkLabelTQOriginalSupport.Name = "linkLabelTQOriginalSupport";
			linkLabelTQOriginalSupport.Size = new Size(98, 18);
			linkLabelTQOriginalSupport.TabIndex = 54;
			linkLabelTQOriginalSupport.TabStop = true;
			linkLabelTQOriginalSupport.Text = "How to play...";
			linkLabelTQOriginalSupport.LinkClicked += linkLabelTQOriginalSupport_LinkClicked;
			// 
			// languageComboBox
			// 
			languageComboBox.DrawMode = DrawMode.OwnerDrawFixed;
			languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			languageComboBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			languageComboBox.FormattingEnabled = true;
			languageComboBox.Location = new Point(3, 21);
			languageComboBox.Name = "languageComboBox";
			languageComboBox.Size = new Size(397, 25);
			languageComboBox.TabIndex = 15;
			languageComboBox.SelectedIndexChanged += LanguageComboBoxSelectedIndexChanged;
			// 
			// languageLabel
			// 
			languageLabel.AutoSize = true;
			languageLabel.Font = new Font("Microsoft Sans Serif", 11.25F);
			languageLabel.ForeColor = Color.White;
			languageLabel.Location = new Point(3, 0);
			languageLabel.Name = "languageLabel";
			languageLabel.Size = new Size(117, 18);
			languageLabel.TabIndex = 16;
			languageLabel.Text = "Game Language";
			// 
			// titanQuestPathTextBox
			// 
			titanQuestPathTextBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			titanQuestPathTextBox.Location = new Point(3, 3);
			titanQuestPathTextBox.Name = "titanQuestPathTextBox";
			titanQuestPathTextBox.Size = new Size(397, 24);
			titanQuestPathTextBox.TabIndex = 18;
			titanQuestPathTextBox.Leave += TitanQuestPathTextBoxLeave;
			// 
			// titanQuestPathLabel
			// 
			titanQuestPathLabel.AutoSize = true;
			titanQuestPathLabel.Font = new Font("Microsoft Sans Serif", 11.25F);
			titanQuestPathLabel.ForeColor = Color.White;
			titanQuestPathLabel.Location = new Point(3, 0);
			titanQuestPathLabel.Name = "titanQuestPathLabel";
			titanQuestPathLabel.Size = new Size(108, 18);
			titanQuestPathLabel.TabIndex = 19;
			titanQuestPathLabel.Text = "TQ Game Path";
			// 
			// immortalThronePathLabel
			// 
			immortalThronePathLabel.AutoSize = true;
			immortalThronePathLabel.Font = new Font("Microsoft Sans Serif", 11.25F);
			immortalThronePathLabel.ForeColor = Color.White;
			immortalThronePathLabel.Location = new Point(3, 57);
			immortalThronePathLabel.Name = "immortalThronePathLabel";
			immortalThronePathLabel.Size = new Size(99, 18);
			immortalThronePathLabel.TabIndex = 20;
			immortalThronePathLabel.Text = "IT Game Path";
			// 
			// immortalThronePathTextBox
			// 
			immortalThronePathTextBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			immortalThronePathTextBox.Location = new Point(3, 3);
			immortalThronePathTextBox.Name = "immortalThronePathTextBox";
			immortalThronePathTextBox.Size = new Size(397, 24);
			immortalThronePathTextBox.TabIndex = 21;
			immortalThronePathTextBox.Leave += ImmortalThronePathTextBoxLeave;
			// 
			// titanQuestPathBrowseButton
			// 
			titanQuestPathBrowseButton.BackColor = Color.Transparent;
			titanQuestPathBrowseButton.DownBitmap = (Bitmap)resources.GetObject("titanQuestPathBrowseButton.DownBitmap");
			titanQuestPathBrowseButton.FlatAppearance.BorderSize = 0;
			titanQuestPathBrowseButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			titanQuestPathBrowseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			titanQuestPathBrowseButton.FlatStyle = FlatStyle.Flat;
			titanQuestPathBrowseButton.Font = new Font("Microsoft Sans Serif", 12F);
			titanQuestPathBrowseButton.ForeColor = Color.FromArgb(51, 44, 28);
			titanQuestPathBrowseButton.Image = (Image)resources.GetObject("titanQuestPathBrowseButton.Image");
			titanQuestPathBrowseButton.Location = new Point(406, 0);
			titanQuestPathBrowseButton.Margin = new Padding(3, 0, 3, 3);
			titanQuestPathBrowseButton.Name = "titanQuestPathBrowseButton";
			titanQuestPathBrowseButton.OverBitmap = (Bitmap)resources.GetObject("titanQuestPathBrowseButton.OverBitmap");
			titanQuestPathBrowseButton.Size = new Size(47, 30);
			titanQuestPathBrowseButton.SizeToGraphic = false;
			titanQuestPathBrowseButton.TabIndex = 23;
			titanQuestPathBrowseButton.Text = "...";
			titanQuestPathBrowseButton.UpBitmap = (Bitmap)resources.GetObject("titanQuestPathBrowseButton.UpBitmap");
			titanQuestPathBrowseButton.UseCustomGraphic = true;
			titanQuestPathBrowseButton.UseVisualStyleBackColor = false;
			titanQuestPathBrowseButton.Click += TitanQuestPathBrowseButtonClick;
			// 
			// immortalThronePathBrowseButton
			// 
			immortalThronePathBrowseButton.BackColor = Color.Transparent;
			immortalThronePathBrowseButton.DownBitmap = (Bitmap)resources.GetObject("immortalThronePathBrowseButton.DownBitmap");
			immortalThronePathBrowseButton.FlatAppearance.BorderSize = 0;
			immortalThronePathBrowseButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 51, 44, 28);
			immortalThronePathBrowseButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 51, 44, 28);
			immortalThronePathBrowseButton.FlatStyle = FlatStyle.Flat;
			immortalThronePathBrowseButton.Font = new Font("Microsoft Sans Serif", 12F);
			immortalThronePathBrowseButton.ForeColor = Color.FromArgb(51, 44, 28);
			immortalThronePathBrowseButton.Image = (Image)resources.GetObject("immortalThronePathBrowseButton.Image");
			immortalThronePathBrowseButton.Location = new Point(406, 0);
			immortalThronePathBrowseButton.Margin = new Padding(3, 0, 3, 3);
			immortalThronePathBrowseButton.Name = "immortalThronePathBrowseButton";
			immortalThronePathBrowseButton.OverBitmap = (Bitmap)resources.GetObject("immortalThronePathBrowseButton.OverBitmap");
			immortalThronePathBrowseButton.Size = new Size(47, 30);
			immortalThronePathBrowseButton.SizeToGraphic = false;
			immortalThronePathBrowseButton.TabIndex = 24;
			immortalThronePathBrowseButton.Text = "...";
			immortalThronePathBrowseButton.UpBitmap = (Bitmap)resources.GetObject("immortalThronePathBrowseButton.UpBitmap");
			immortalThronePathBrowseButton.UseCustomGraphic = true;
			immortalThronePathBrowseButton.UseVisualStyleBackColor = false;
			immortalThronePathBrowseButton.Click += ImmortalThronePathBrowseButtonClick;
			// 
			// customMapLabel
			// 
			customMapLabel.AutoSize = true;
			customMapLabel.Font = new Font("Microsoft Sans Serif", 11.25F);
			customMapLabel.ForeColor = Color.White;
			customMapLabel.Location = new Point(3, 0);
			customMapLabel.Name = "customMapLabel";
			customMapLabel.Size = new Size(94, 18);
			customMapLabel.TabIndex = 27;
			customMapLabel.Text = "Custom Map";
			// 
			// mapListComboBox
			// 
			mapListComboBox.DrawMode = DrawMode.OwnerDrawFixed;
			mapListComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			mapListComboBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			mapListComboBox.FormattingEnabled = true;
			mapListComboBox.Location = new Point(3, 21);
			mapListComboBox.Name = "mapListComboBox";
			mapListComboBox.Size = new Size(397, 25);
			mapListComboBox.TabIndex = 26;
			mapListComboBox.SelectedIndexChanged += MapListComboBoxSelectedIndexChanged;
			// 
			// baseFontLabel
			// 
			baseFontLabel.AutoSize = true;
			baseFontLabel.Font = new Font("Microsoft Sans Serif", 11.25F);
			baseFontLabel.ForeColor = Color.White;
			baseFontLabel.Location = new Point(3, 0);
			baseFontLabel.Name = "baseFontLabel";
			baseFontLabel.Size = new Size(38, 18);
			baseFontLabel.TabIndex = 36;
			baseFontLabel.Text = "Font";
			// 
			// baseFontComboBox
			// 
			baseFontComboBox.DrawMode = DrawMode.OwnerDrawFixed;
			baseFontComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			baseFontComboBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			baseFontComboBox.FormattingEnabled = true;
			baseFontComboBox.Location = new Point(3, 21);
			baseFontComboBox.Name = "baseFontComboBox";
			baseFontComboBox.Size = new Size(304, 25);
			baseFontComboBox.TabIndex = 35;
			baseFontComboBox.SelectedIndexChanged += FontComboBoxBase_SelectedIndexChanged;
			// 
			// tableLayoutPanelButtons
			// 
			tableLayoutPanelButtons.AutoSize = true;
			tableLayoutPanelButtons.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			tableLayoutPanelButtons.ColumnCount = 5;
			bufferedTableLayoutPanelSkeleton.SetColumnSpan(tableLayoutPanelButtons, 5);
			tableLayoutPanelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			tableLayoutPanelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			tableLayoutPanelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
			tableLayoutPanelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			tableLayoutPanelButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
			tableLayoutPanelButtons.Controls.Add(okayButton, 1, 0);
			tableLayoutPanelButtons.Controls.Add(cancelButton, 3, 0);
			tableLayoutPanelButtons.Controls.Add(resetButton, 4, 0);
			tableLayoutPanelButtons.Dock = DockStyle.Fill;
			tableLayoutPanelButtons.Location = new Point(3, 577);
			tableLayoutPanelButtons.Name = "tableLayoutPanelButtons";
			tableLayoutPanelButtons.RowCount = 1;
			tableLayoutPanelButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanelButtons.Size = new Size(1192, 36);
			tableLayoutPanelButtons.TabIndex = 37;
			// 
			// ItemBGColorOpacityTrackBar
			// 
			ItemBGColorOpacityTrackBar.Location = new Point(3, 126);
			ItemBGColorOpacityTrackBar.Maximum = 255;
			ItemBGColorOpacityTrackBar.Name = "ItemBGColorOpacityTrackBar";
			ItemBGColorOpacityTrackBar.Size = new Size(304, 45);
			ItemBGColorOpacityTrackBar.TabIndex = 39;
			ItemBGColorOpacityTrackBar.TickFrequency = 5;
			ItemBGColorOpacityTrackBar.Value = 15;
			ItemBGColorOpacityTrackBar.Scroll += ItemBGColorOpacityTrackBar_Scroll;
			// 
			// scalingCheckBoxEnableSounds
			// 
			scalingCheckBoxEnableSounds.AutoSize = true;
			scalingCheckBoxEnableSounds.Font = new Font("Microsoft Sans Serif", 11.25F);
			scalingCheckBoxEnableSounds.ForeColor = Color.White;
			scalingCheckBoxEnableSounds.Location = new Point(3, 52);
			scalingCheckBoxEnableSounds.Name = "scalingCheckBoxEnableSounds";
			scalingCheckBoxEnableSounds.Size = new Size(184, 22);
			scalingCheckBoxEnableSounds.TabIndex = 43;
			scalingCheckBoxEnableSounds.Text = "Enable TQVault Sounds";
			scalingCheckBoxEnableSounds.UseVisualStyleBackColor = true;
			scalingCheckBoxEnableSounds.CheckedChanged += scalingCheckBoxEnableSounds_CheckedChanged;
			// 
			// scalingLabelCSVDelim
			// 
			scalingLabelCSVDelim.AutoSize = true;
			scalingLabelCSVDelim.Font = new Font("Microsoft Sans Serif", 11.25F);
			scalingLabelCSVDelim.ForeColor = Color.White;
			scalingLabelCSVDelim.Location = new Point(3, 250);
			scalingLabelCSVDelim.Name = "scalingLabelCSVDelim";
			scalingLabelCSVDelim.Size = new Size(96, 18);
			scalingLabelCSVDelim.TabIndex = 45;
			scalingLabelCSVDelim.Text = "Csv Delimiter";
			// 
			// scalingComboBoxCSVDelim
			// 
			scalingComboBoxCSVDelim.BackColor = SystemColors.Window;
			scalingComboBoxCSVDelim.DrawMode = DrawMode.OwnerDrawFixed;
			scalingComboBoxCSVDelim.DropDownStyle = ComboBoxStyle.DropDownList;
			scalingComboBoxCSVDelim.Font = new Font("Microsoft Sans Serif", 11.25F);
			scalingComboBoxCSVDelim.FormattingEnabled = true;
			scalingComboBoxCSVDelim.Location = new Point(3, 271);
			scalingComboBoxCSVDelim.Name = "scalingComboBoxCSVDelim";
			scalingComboBoxCSVDelim.Size = new Size(324, 25);
			scalingComboBoxCSVDelim.TabIndex = 44;
			scalingComboBoxCSVDelim.SelectedIndexChanged += scalingComboBoxCSVDelim_SelectedIndexChanged;
			// 
			// groupBoxGeneral
			// 
			groupBoxGeneral.AutoSize = true;
			groupBoxGeneral.Controls.Add(bufferedFlowLayoutPanelGeneralSettings);
			groupBoxGeneral.ForeColor = Color.Gold;
			groupBoxGeneral.Location = new Point(3, 3);
			groupBoxGeneral.Name = "groupBoxGeneral";
			groupBoxGeneral.Size = new Size(336, 322);
			groupBoxGeneral.TabIndex = 49;
			groupBoxGeneral.TabStop = false;
			groupBoxGeneral.Text = "General Settings";
			// 
			// bufferedFlowLayoutPanelGeneralSettings
			// 
			bufferedFlowLayoutPanelGeneralSettings.AutoSize = true;
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(skipTitleCheckBox);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(loadLastCharacterCheckBox);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(loadLastVaultCheckBox);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(loadAllFilesCheckBox);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(hotReloadCheckBox);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(suppressWarningsCheckBox);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(EnableDetailedTooltipViewCheckBox);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(playerReadonlyCheckbox);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(scalingCheckBoxDisableAutoStacking);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(scalingLabelCSVDelim);
			bufferedFlowLayoutPanelGeneralSettings.Controls.Add(scalingComboBoxCSVDelim);
			bufferedFlowLayoutPanelGeneralSettings.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelGeneralSettings.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelGeneralSettings.Location = new Point(3, 20);
			bufferedFlowLayoutPanelGeneralSettings.Name = "bufferedFlowLayoutPanelGeneralSettings";
			bufferedFlowLayoutPanelGeneralSettings.Size = new Size(330, 299);
			bufferedFlowLayoutPanelGeneralSettings.TabIndex = 0;
			// 
			// checkGroupBoxAllowCheats
			// 
			checkGroupBoxAllowCheats.AutoSize = true;
			checkGroupBoxAllowCheats.Checked = false;
			checkGroupBoxAllowCheats.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Enable;
			checkGroupBoxAllowCheats.CheckState = CheckState.Unchecked;
			checkGroupBoxAllowCheats.Controls.Add(bufferedFlowLayoutPanelCheats);
			checkGroupBoxAllowCheats.ForeColor = Color.Gold;
			checkGroupBoxAllowCheats.Location = new Point(3, 331);
			checkGroupBoxAllowCheats.Name = "checkGroupBoxAllowCheats";
			checkGroupBoxAllowCheats.Size = new Size(263, 135);
			checkGroupBoxAllowCheats.TabIndex = 51;
			checkGroupBoxAllowCheats.TabStop = false;
			checkGroupBoxAllowCheats.Text = "Allow Cheats";
			checkGroupBoxAllowCheats.CheckedChanged += checkGroupBoxCheats_CheckedChanged;
			// 
			// bufferedFlowLayoutPanelCheats
			// 
			bufferedFlowLayoutPanelCheats.AutoSize = true;
			bufferedFlowLayoutPanelCheats.Controls.Add(characterEditCheckBox);
			bufferedFlowLayoutPanelCheats.Controls.Add(allowItemCopyCheckBox);
			bufferedFlowLayoutPanelCheats.Controls.Add(allowItemEditCheckBox);
			bufferedFlowLayoutPanelCheats.Controls.Add(scalingCheckBoxEnableEpicLegendaryAffixes);
			bufferedFlowLayoutPanelCheats.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelCheats.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelCheats.Location = new Point(3, 20);
			bufferedFlowLayoutPanelCheats.Name = "bufferedFlowLayoutPanelCheats";
			bufferedFlowLayoutPanelCheats.Size = new Size(257, 112);
			bufferedFlowLayoutPanelCheats.TabIndex = 1;
			// 
			// groupBoxGfxAndAudio
			// 
			groupBoxGfxAndAudio.AutoSize = true;
			groupBoxGfxAndAudio.Controls.Add(bufferedFlowLayoutPanelGfxAndAudio);
			groupBoxGfxAndAudio.ForeColor = Color.Gold;
			groupBoxGfxAndAudio.Location = new Point(3, 3);
			groupBoxGfxAndAudio.Name = "groupBoxGfxAndAudio";
			groupBoxGfxAndAudio.Size = new Size(316, 197);
			groupBoxGfxAndAudio.TabIndex = 52;
			groupBoxGfxAndAudio.TabStop = false;
			groupBoxGfxAndAudio.Text = "Gfx / Audio";
			// 
			// bufferedFlowLayoutPanelGfxAndAudio
			// 
			bufferedFlowLayoutPanelGfxAndAudio.AutoSize = true;
			bufferedFlowLayoutPanelGfxAndAudio.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(baseFontLabel);
			bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(baseFontComboBox);
			bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(scalingCheckBoxEnableSounds);
			bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(EnableItemRequirementRestrictionCheckBox);
			bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(ItemBGColorOpacityLabel);
			bufferedFlowLayoutPanelGfxAndAudio.Controls.Add(ItemBGColorOpacityTrackBar);
			bufferedFlowLayoutPanelGfxAndAudio.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelGfxAndAudio.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelGfxAndAudio.Location = new Point(3, 20);
			bufferedFlowLayoutPanelGfxAndAudio.Name = "bufferedFlowLayoutPanelGfxAndAudio";
			bufferedFlowLayoutPanelGfxAndAudio.Size = new Size(310, 174);
			bufferedFlowLayoutPanelGfxAndAudio.TabIndex = 0;
			// 
			// detectLanguageCheckBox
			// 
			detectLanguageCheckBox.AutoSize = true;
			detectLanguageCheckBox.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Disable;
			detectLanguageCheckBox.Controls.Add(bufferedFlowLayoutPanelLanguage);
			detectLanguageCheckBox.ForeColor = Color.Gold;
			detectLanguageCheckBox.Location = new Point(3, 296);
			detectLanguageCheckBox.Name = "detectLanguageCheckBox";
			detectLanguageCheckBox.Size = new Size(409, 72);
			detectLanguageCheckBox.TabIndex = 53;
			detectLanguageCheckBox.TabStop = false;
			detectLanguageCheckBox.Text = "Autodetect Language";
			detectLanguageCheckBox.CheckedChanged += DetectLanguageCheckBoxCheckedChanged;
			// 
			// bufferedFlowLayoutPanelLanguage
			// 
			bufferedFlowLayoutPanelLanguage.AutoSize = true;
			bufferedFlowLayoutPanelLanguage.Controls.Add(languageLabel);
			bufferedFlowLayoutPanelLanguage.Controls.Add(languageComboBox);
			bufferedFlowLayoutPanelLanguage.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelLanguage.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelLanguage.Location = new Point(3, 20);
			bufferedFlowLayoutPanelLanguage.Name = "bufferedFlowLayoutPanelLanguage";
			bufferedFlowLayoutPanelLanguage.Size = new Size(403, 49);
			bufferedFlowLayoutPanelLanguage.TabIndex = 1;
			// 
			// detectGamePathsCheckBox
			// 
			detectGamePathsCheckBox.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Disable;
			detectGamePathsCheckBox.Controls.Add(bufferedFlowLayoutPanelAutoDetectGamePath);
			detectGamePathsCheckBox.ForeColor = Color.Gold;
			detectGamePathsCheckBox.Location = new Point(3, 60);
			detectGamePathsCheckBox.Name = "detectGamePathsCheckBox";
			detectGamePathsCheckBox.Size = new Size(466, 141);
			detectGamePathsCheckBox.TabIndex = 54;
			detectGamePathsCheckBox.TabStop = false;
			detectGamePathsCheckBox.Text = "Autodetect Game Paths";
			detectGamePathsCheckBox.CheckedChanged += DetectGamePathsCheckBoxCheckedChanged;
			// 
			// bufferedFlowLayoutPanelAutoDetectGamePath
			// 
			bufferedFlowLayoutPanelAutoDetectGamePath.Controls.Add(titanQuestPathLabel);
			bufferedFlowLayoutPanelAutoDetectGamePath.Controls.Add(bufferedFlowLayoutPanelTQPath);
			bufferedFlowLayoutPanelAutoDetectGamePath.Controls.Add(immortalThronePathLabel);
			bufferedFlowLayoutPanelAutoDetectGamePath.Controls.Add(bufferedFlowLayoutPanelTQITPath);
			bufferedFlowLayoutPanelAutoDetectGamePath.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelAutoDetectGamePath.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelAutoDetectGamePath.Location = new Point(3, 20);
			bufferedFlowLayoutPanelAutoDetectGamePath.Name = "bufferedFlowLayoutPanelAutoDetectGamePath";
			bufferedFlowLayoutPanelAutoDetectGamePath.Size = new Size(460, 118);
			bufferedFlowLayoutPanelAutoDetectGamePath.TabIndex = 1;
			// 
			// bufferedFlowLayoutPanelTQPath
			// 
			bufferedFlowLayoutPanelTQPath.AutoSize = true;
			bufferedFlowLayoutPanelTQPath.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			bufferedFlowLayoutPanelTQPath.Controls.Add(titanQuestPathTextBox);
			bufferedFlowLayoutPanelTQPath.Controls.Add(titanQuestPathBrowseButton);
			bufferedFlowLayoutPanelTQPath.Location = new Point(3, 21);
			bufferedFlowLayoutPanelTQPath.Margin = new Padding(3, 3, 0, 3);
			bufferedFlowLayoutPanelTQPath.Name = "bufferedFlowLayoutPanelTQPath";
			bufferedFlowLayoutPanelTQPath.Size = new Size(456, 33);
			bufferedFlowLayoutPanelTQPath.TabIndex = 55;
			// 
			// bufferedFlowLayoutPanelTQITPath
			// 
			bufferedFlowLayoutPanelTQITPath.AutoSize = true;
			bufferedFlowLayoutPanelTQITPath.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			bufferedFlowLayoutPanelTQITPath.Controls.Add(immortalThronePathTextBox);
			bufferedFlowLayoutPanelTQITPath.Controls.Add(immortalThronePathBrowseButton);
			bufferedFlowLayoutPanelTQITPath.Location = new Point(3, 78);
			bufferedFlowLayoutPanelTQITPath.Margin = new Padding(3, 3, 0, 3);
			bufferedFlowLayoutPanelTQITPath.Name = "bufferedFlowLayoutPanelTQITPath";
			bufferedFlowLayoutPanelTQITPath.Size = new Size(456, 33);
			bufferedFlowLayoutPanelTQITPath.TabIndex = 55;
			// 
			// enableCustomMapsCheckBox
			// 
			enableCustomMapsCheckBox.Checked = false;
			enableCustomMapsCheckBox.CheckedBehavior = UIToolbox.CheckGroupBoxCheckedBehavior.Enable;
			enableCustomMapsCheckBox.CheckState = CheckState.Unchecked;
			enableCustomMapsCheckBox.Controls.Add(bufferedFlowLayoutPanelCustomMap);
			enableCustomMapsCheckBox.ForeColor = Color.Gold;
			enableCustomMapsCheckBox.Location = new Point(3, 207);
			enableCustomMapsCheckBox.Name = "enableCustomMapsCheckBox";
			enableCustomMapsCheckBox.Size = new Size(466, 83);
			enableCustomMapsCheckBox.TabIndex = 55;
			enableCustomMapsCheckBox.TabStop = false;
			enableCustomMapsCheckBox.Text = "Enable Custom Maps";
			enableCustomMapsCheckBox.CheckedChanged += EnableCustomMapsCheckBoxCheckedChanged;
			// 
			// bufferedFlowLayoutPanelCustomMap
			// 
			bufferedFlowLayoutPanelCustomMap.Controls.Add(customMapLabel);
			bufferedFlowLayoutPanelCustomMap.Controls.Add(mapListComboBox);
			bufferedFlowLayoutPanelCustomMap.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelCustomMap.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelCustomMap.Location = new Point(3, 20);
			bufferedFlowLayoutPanelCustomMap.Name = "bufferedFlowLayoutPanelCustomMap";
			bufferedFlowLayoutPanelCustomMap.Size = new Size(460, 60);
			bufferedFlowLayoutPanelCustomMap.TabIndex = 1;
			// 
			// bufferedTableLayoutPanelSkeleton
			// 
			bufferedTableLayoutPanelSkeleton.AutoSize = true;
			bufferedTableLayoutPanelSkeleton.ColumnCount = 5;
			bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new ColumnStyle());
			bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
			bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new ColumnStyle());
			bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 5F));
			bufferedTableLayoutPanelSkeleton.ColumnStyles.Add(new ColumnStyle());
			bufferedTableLayoutPanelSkeleton.Controls.Add(bufferedFlowLayoutPanelSkeletonRight, 4, 0);
			bufferedTableLayoutPanelSkeleton.Controls.Add(tableLayoutPanelButtons, 0, 1);
			bufferedTableLayoutPanelSkeleton.Controls.Add(bufferedFlowLayoutPanelSkeletonLeft, 0, 0);
			bufferedTableLayoutPanelSkeleton.Controls.Add(bufferedFlowLayoutPanelSkeletonCenter, 2, 0);
			bufferedTableLayoutPanelSkeleton.Location = new Point(10, 25);
			bufferedTableLayoutPanelSkeleton.Margin = new Padding(0);
			bufferedTableLayoutPanelSkeleton.Name = "bufferedTableLayoutPanelSkeleton";
			bufferedTableLayoutPanelSkeleton.RowCount = 2;
			bufferedTableLayoutPanelSkeleton.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			bufferedTableLayoutPanelSkeleton.RowStyles.Add(new RowStyle());
			bufferedTableLayoutPanelSkeleton.Size = new Size(1198, 616);
			bufferedTableLayoutPanelSkeleton.TabIndex = 56;
			// 
			// bufferedFlowLayoutPanelSkeletonRight
			// 
			bufferedFlowLayoutPanelSkeletonRight.AutoSize = true;
			bufferedFlowLayoutPanelSkeletonRight.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			bufferedFlowLayoutPanelSkeletonRight.Controls.Add(groupBoxGfxAndAudio);
			bufferedFlowLayoutPanelSkeletonRight.Controls.Add(pasteBinGroupBox);
			bufferedFlowLayoutPanelSkeletonRight.Controls.Add(checkGroupBoxOriginalTQSupport);
			bufferedFlowLayoutPanelSkeletonRight.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelSkeletonRight.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelSkeletonRight.Location = new Point(839, 3);
			bufferedFlowLayoutPanelSkeletonRight.Name = "bufferedFlowLayoutPanelSkeletonRight";
			bufferedFlowLayoutPanelSkeletonRight.Size = new Size(356, 568);
			bufferedFlowLayoutPanelSkeletonRight.TabIndex = 2;
			// 
			// pasteBinGroupBox
			// 
			pasteBinGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			pasteBinGroupBox.Controls.Add(pasteBinFlowPanel);
			pasteBinGroupBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			pasteBinGroupBox.ForeColor = Color.Gold;
			pasteBinGroupBox.Location = new Point(3, 206);
			pasteBinGroupBox.Name = "pasteBinGroupBox";
			pasteBinGroupBox.Size = new Size(316, 119);
			pasteBinGroupBox.TabIndex = 53;
			pasteBinGroupBox.TabStop = false;
			pasteBinGroupBox.Text = "PasteBin Integration";
			// 
			// pasteBinFlowPanel
			// 
			pasteBinFlowPanel.ColumnCount = 2;
			pasteBinFlowPanel.ColumnStyles.Add(new ColumnStyle());
			pasteBinFlowPanel.ColumnStyles.Add(new ColumnStyle());
			pasteBinFlowPanel.Controls.Add(pasteBinExpirationComboBox, 1, 2);
			pasteBinFlowPanel.Controls.Add(pasteBinApiKeyLabel, 0, 0);
			pasteBinFlowPanel.Controls.Add(pasteBinApiKeyTextBox, 0, 1);
			pasteBinFlowPanel.Controls.Add(pasteBinExpirationLabel, 0, 2);
			pasteBinFlowPanel.Dock = DockStyle.Fill;
			pasteBinFlowPanel.Location = new Point(3, 20);
			pasteBinFlowPanel.Name = "pasteBinFlowPanel";
			pasteBinFlowPanel.RowCount = 3;
			pasteBinFlowPanel.RowStyles.Add(new RowStyle());
			pasteBinFlowPanel.RowStyles.Add(new RowStyle());
			pasteBinFlowPanel.RowStyles.Add(new RowStyle());
			pasteBinFlowPanel.Size = new Size(310, 96);
			pasteBinFlowPanel.TabIndex = 0;
			// 
			// pasteBinExpirationComboBox
			// 
			pasteBinExpirationComboBox.DrawMode = DrawMode.OwnerDrawFixed;
			pasteBinExpirationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			pasteBinExpirationComboBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			pasteBinExpirationComboBox.Name = "pasteBinExpirationComboBox";
			pasteBinExpirationComboBox.Size = new Size(150, 30);
			pasteBinExpirationComboBox.TabIndex = 3;
			pasteBinExpirationComboBox.SelectedIndexChanged += PasteBinExpirationComboBoxSelectedIndexChanged;
			// 
			// pasteBinApiKeyLabel
			// 
			pasteBinApiKeyLabel.AutoSize = true;
			pasteBinFlowPanel.SetColumnSpan(pasteBinApiKeyLabel, 2);
			pasteBinApiKeyLabel.Font = new Font("Microsoft Sans Serif", 11.25F);
			pasteBinApiKeyLabel.ForeColor = Color.White;
			pasteBinApiKeyLabel.Location = new Point(3, 0);
			pasteBinApiKeyLabel.Name = "pasteBinApiKeyLabel";
			pasteBinApiKeyLabel.Size = new Size(126, 18);
			pasteBinApiKeyLabel.TabIndex = 0;
			pasteBinApiKeyLabel.Text = "PasteBin API Key:";
			// 
			// pasteBinApiKeyTextBox
			// 
			pasteBinFlowPanel.SetColumnSpan(pasteBinApiKeyTextBox, 2);
			pasteBinApiKeyTextBox.Font = new Font("Microsoft Sans Serif", 11.25F);
			pasteBinApiKeyTextBox.Location = new Point(3, 21);
			pasteBinApiKeyTextBox.Name = "pasteBinApiKeyTextBox";
			pasteBinApiKeyTextBox.Size = new Size(250, 24);
			pasteBinApiKeyTextBox.TabIndex = 1;
			pasteBinApiKeyTextBox.TextChanged += PasteBinApiKeyTextBoxTextChanged;
			// 
			// pasteBinExpirationLabel
			// 
			pasteBinExpirationLabel.AutoSize = true;
			pasteBinExpirationLabel.Font = new Font("Microsoft Sans Serif", 11.25F);
			pasteBinExpirationLabel.ForeColor = Color.White;
			pasteBinExpirationLabel.Location = new Point(3, 48);
			pasteBinExpirationLabel.Name = "pasteBinExpirationLabel";
			pasteBinExpirationLabel.Size = new Size(119, 18);
			pasteBinExpirationLabel.TabIndex = 2;
			pasteBinExpirationLabel.Text = "Paste Expiration:";
			// 
			// bufferedFlowLayoutPanelSkeletonLeft
			// 
			bufferedFlowLayoutPanelSkeletonLeft.AutoSize = true;
			bufferedFlowLayoutPanelSkeletonLeft.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(vaultPathLabel);
			bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(bufferedFlowLayoutPanelVaultPath);
			bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(detectGamePathsCheckBox);
			bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(enableCustomMapsCheckBox);
			bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(detectLanguageCheckBox);
			bufferedFlowLayoutPanelSkeletonLeft.Controls.Add(checkGroupBoxGitBackup);
			bufferedFlowLayoutPanelSkeletonLeft.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelSkeletonLeft.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelSkeletonLeft.Location = new Point(3, 3);
			bufferedFlowLayoutPanelSkeletonLeft.Name = "bufferedFlowLayoutPanelSkeletonLeft";
			bufferedFlowLayoutPanelSkeletonLeft.Size = new Size(472, 568);
			bufferedFlowLayoutPanelSkeletonLeft.TabIndex = 0;
			// 
			// bufferedFlowLayoutPanelVaultPath
			// 
			bufferedFlowLayoutPanelVaultPath.AutoSize = true;
			bufferedFlowLayoutPanelVaultPath.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			bufferedFlowLayoutPanelVaultPath.Controls.Add(vaultPathTextBox);
			bufferedFlowLayoutPanelVaultPath.Controls.Add(vaultPathBrowseButton);
			bufferedFlowLayoutPanelVaultPath.Location = new Point(3, 21);
			bufferedFlowLayoutPanelVaultPath.Name = "bufferedFlowLayoutPanelVaultPath";
			bufferedFlowLayoutPanelVaultPath.Size = new Size(456, 33);
			bufferedFlowLayoutPanelVaultPath.TabIndex = 57;
			// 
			// bufferedFlowLayoutPanelSkeletonCenter
			// 
			bufferedFlowLayoutPanelSkeletonCenter.AutoSize = true;
			bufferedFlowLayoutPanelSkeletonCenter.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			bufferedFlowLayoutPanelSkeletonCenter.Controls.Add(groupBoxGeneral);
			bufferedFlowLayoutPanelSkeletonCenter.Controls.Add(checkGroupBoxAllowCheats);
			bufferedFlowLayoutPanelSkeletonCenter.Dock = DockStyle.Fill;
			bufferedFlowLayoutPanelSkeletonCenter.FlowDirection = FlowDirection.TopDown;
			bufferedFlowLayoutPanelSkeletonCenter.Location = new Point(486, 3);
			bufferedFlowLayoutPanelSkeletonCenter.Name = "bufferedFlowLayoutPanelSkeletonCenter";
			bufferedFlowLayoutPanelSkeletonCenter.Size = new Size(342, 568);
			bufferedFlowLayoutPanelSkeletonCenter.TabIndex = 1;
			// 
			// SettingsDialog
			// 
			AutoScaleDimensions = new SizeF(96F, 96F);
			AutoScaleMode = AutoScaleMode.Dpi;
			AutoValidate = AutoValidate.EnablePreventFocusChange;
			BackColor = Color.FromArgb(46, 31, 21);
			CancelButton = cancelButton;
			ClientSize = new Size(1222, 666);
			Controls.Add(bufferedTableLayoutPanelSkeleton);
			DrawCustomBorder = true;
			Font = new Font("Microsoft Sans Serif", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
			ForeColor = Color.White;
			FormBorderStyle = FormBorderStyle.None;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "SettingsDialog";
			Padding = new Padding(5, 5, 5, 0);
			ShowIcon = false;
			ShowInTaskbar = false;
			SizeGripStyle = SizeGripStyle.Hide;
			StartPosition = FormStartPosition.CenterParent;
			Text = "Configure Settings";
			TopMost = true;
			Load += SettingsDialogLoad;
			Shown += SettingsDialog_Shown;
			Controls.SetChildIndex(bufferedTableLayoutPanelSkeleton, 0);
			checkGroupBoxGitBackup.ResumeLayout(false);
			checkGroupBoxGitBackup.PerformLayout();
			bufferedFlowLayoutPanelGitBackup.ResumeLayout(false);
			bufferedFlowLayoutPanelGitBackup.PerformLayout();
			checkGroupBoxOriginalTQSupport.ResumeLayout(false);
			checkGroupBoxOriginalTQSupport.PerformLayout();
			bufferedFlowLayoutPanelTQOriginalSupport.ResumeLayout(false);
			bufferedFlowLayoutPanelTQOriginalSupport.PerformLayout();
			tableLayoutPanelButtons.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)ItemBGColorOpacityTrackBar).EndInit();
			groupBoxGeneral.ResumeLayout(false);
			groupBoxGeneral.PerformLayout();
			bufferedFlowLayoutPanelGeneralSettings.ResumeLayout(false);
			bufferedFlowLayoutPanelGeneralSettings.PerformLayout();
			checkGroupBoxAllowCheats.ResumeLayout(false);
			checkGroupBoxAllowCheats.PerformLayout();
			bufferedFlowLayoutPanelCheats.ResumeLayout(false);
			bufferedFlowLayoutPanelCheats.PerformLayout();
			groupBoxGfxAndAudio.ResumeLayout(false);
			groupBoxGfxAndAudio.PerformLayout();
			bufferedFlowLayoutPanelGfxAndAudio.ResumeLayout(false);
			bufferedFlowLayoutPanelGfxAndAudio.PerformLayout();
			detectLanguageCheckBox.ResumeLayout(false);
			detectLanguageCheckBox.PerformLayout();
			bufferedFlowLayoutPanelLanguage.ResumeLayout(false);
			bufferedFlowLayoutPanelLanguage.PerformLayout();
			detectGamePathsCheckBox.ResumeLayout(false);
			detectGamePathsCheckBox.PerformLayout();
			bufferedFlowLayoutPanelAutoDetectGamePath.ResumeLayout(false);
			bufferedFlowLayoutPanelAutoDetectGamePath.PerformLayout();
			bufferedFlowLayoutPanelTQPath.ResumeLayout(false);
			bufferedFlowLayoutPanelTQPath.PerformLayout();
			bufferedFlowLayoutPanelTQITPath.ResumeLayout(false);
			bufferedFlowLayoutPanelTQITPath.PerformLayout();
			enableCustomMapsCheckBox.ResumeLayout(false);
			enableCustomMapsCheckBox.PerformLayout();
			bufferedFlowLayoutPanelCustomMap.ResumeLayout(false);
			bufferedFlowLayoutPanelCustomMap.PerformLayout();
			bufferedTableLayoutPanelSkeleton.ResumeLayout(false);
			bufferedTableLayoutPanelSkeleton.PerformLayout();
			bufferedFlowLayoutPanelSkeletonRight.ResumeLayout(false);
			bufferedFlowLayoutPanelSkeletonRight.PerformLayout();
			pasteBinGroupBox.ResumeLayout(false);
			pasteBinFlowPanel.ResumeLayout(false);
			pasteBinFlowPanel.PerformLayout();
			bufferedFlowLayoutPanelSkeletonLeft.ResumeLayout(false);
			bufferedFlowLayoutPanelSkeletonLeft.PerformLayout();
			bufferedFlowLayoutPanelVaultPath.ResumeLayout(false);
			bufferedFlowLayoutPanelVaultPath.PerformLayout();
			bufferedFlowLayoutPanelSkeletonCenter.ResumeLayout(false);
			bufferedFlowLayoutPanelSkeletonCenter.PerformLayout();
			ResumeLayout(false);
			PerformLayout();

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
		private ScalingLabel pasteBinApiKeyLabel;
		private ScalingTextBox pasteBinApiKeyTextBox;
		private ScalingLabel pasteBinExpirationLabel;
		private ScalingComboBox pasteBinExpirationComboBox;
		private GroupBox pasteBinGroupBox;
		private BufferedTableLayoutPanel pasteBinFlowPanel;
	}
}