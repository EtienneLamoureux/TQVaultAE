//-----------------------------------------------------------------------
// <copyright file="MainForm.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Components;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// MainForm Designer class
	/// </summary>
	public partial class MainForm
	{
		/// <summary>
		/// Windows Form Exit button
		/// </summary>
		private ScalingButton exitButton;

		/// <summary>
		/// Windows Form Item Text Panel
		/// </summary>
		private System.Windows.Forms.Panel itemTextPanel;

		/// <summary>
		/// Windows Form item text label
		/// </summary>
		private ScalingLabel itemText;

		/// <summary>
		/// Windows Form Vault List Dropdown
		/// </summary>
		private ScalingComboBox vaultListComboBox;

		/// <summary>
		/// Windows Form Vault Label
		/// </summary>
		private ScalingLabel vaultLabel;

		/// <summary>
		/// Windows Form Configure Button
		/// </summary>
		private ScalingButton configureButton;

		/// <summary>
		/// Windows Form Custom Map Text Label
		/// </summary>
		private ScalingLabel customMapText;

		/// <summary>
		/// Windows Form Panel Selection Button
		/// </summary>
		private ScalingButton showVaulButton;

		/// <summary>
		/// Windows Form About Button
		/// </summary>
		private ScalingButton aboutButton;

		/// <summary>
		/// Windows Form TQVault Title Label
		/// </summary>
		private ScalingLabel titleLabel;

		/// <summary>
		/// Windows Form Search Button
		/// </summary>
		private ScalingButton searchButton;

		/// <summary>
		/// Background worker to load resources.
		/// </summary>
		private System.ComponentModel.BackgroundWorker backgroundWorkerLoadAllFiles;

		/// <summary>
		/// Timer for fading in the main form.
		/// </summary>
		private System.Windows.Forms.Timer fadeInTimer;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.exitButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.itemTextPanel = new System.Windows.Forms.Panel();
            this.itemText = new TQVaultAE.GUI.Components.ScalingLabel();
            this.vaultListComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.vaultLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.configureButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.customMapText = new TQVaultAE.GUI.Components.ScalingLabel();
            this.showVaulButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.aboutButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.titleLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.searchButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.backgroundWorkerLoadAllFiles = new System.ComponentModel.BackgroundWorker();
            this.fadeInTimer = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanelVaultSelector = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.tableLayoutPanelMain = new TQVaultAE.GUI.Components.BufferedTableLayoutPanel();
            this.tableLayoutPanelHighlight = new System.Windows.Forms.TableLayoutPanel();
            this.scalingLabelHighlight = new System.Windows.Forms.LinkLabel();
            this.scalingTextBoxHighlight = new TQVaultAE.GUI.Components.ScalingTextBox();
            this.flowLayoutPanelRightPanels = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.flowLayoutPanelRightComboBox = new TQVaultAE.GUI.Components.BufferedFlowLayoutPanel();
            this.characterLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.characterComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.secondaryVaultListComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.duplicateButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.fileSystemWatcherTransferStash = new System.IO.FileSystemWatcher();
            this.fileSystemWatcherRelicStash = new System.IO.FileSystemWatcher();
            this.typeAssistant = new TQVaultAE.GUI.Components.TypeAssistant();
            this.saveButton = new TQVaultAE.GUI.Components.ScalingButton();
            this.flowLayoutPanelMenuButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.highlightFilters = new TQVaultAE.GUI.Components.HighlightFilters();
            this.itemTextPanel.SuspendLayout();
            this.flowLayoutPanelVaultSelector.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            this.tableLayoutPanelHighlight.SuspendLayout();
            this.flowLayoutPanelRightComboBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcherTransferStash)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcherRelicStash)).BeginInit();
            this.flowLayoutPanelMenuButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // exitButton
            // 
            this.exitButton.BackColor = System.Drawing.Color.Transparent;
            this.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.exitButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("exitButton.DownBitmap")));
            this.exitButton.FlatAppearance.BorderSize = 0;
            this.exitButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exitButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.exitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.Image = ((System.Drawing.Image)(resources.GetObject("exitButton.Image")));
            this.exitButton.Location = new System.Drawing.Point(718, 3);
            this.exitButton.Name = "exitButton";
            this.exitButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("exitButton.OverBitmap")));
            this.exitButton.Size = new System.Drawing.Size(137, 30);
            this.exitButton.SizeToGraphic = false;
            this.exitButton.TabIndex = 0;
            this.exitButton.Text = "Exit";
            this.exitButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("exitButton.UpBitmap")));
            this.exitButton.UseCustomGraphic = true;
            this.exitButton.UseVisualStyleBackColor = false;
            this.exitButton.Click += new System.EventHandler(this.ExitButtonClick);
            // 
            // itemTextPanel
            // 
            this.itemTextPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(188)))), ((int)(((byte)(97)))));
            this.itemTextPanel.Controls.Add(this.itemText);
            this.itemTextPanel.Location = new System.Drawing.Point(4, 135);
            this.itemTextPanel.Margin = new System.Windows.Forms.Padding(3, 3, 3, 20);
            this.itemTextPanel.Name = "itemTextPanel";
            this.itemTextPanel.Padding = new System.Windows.Forms.Padding(2);
            this.itemTextPanel.Size = new System.Drawing.Size(592, 22);
            this.itemTextPanel.TabIndex = 4;
            // 
            // itemText
            // 
            this.itemText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.itemText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemText.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.itemText.Location = new System.Drawing.Point(2, 2);
            this.itemText.Name = "itemText";
            this.itemText.Size = new System.Drawing.Size(588, 18);
            this.itemText.TabIndex = 0;
            // 
            // vaultListComboBox
            // 
            this.vaultListComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.vaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vaultListComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.vaultListComboBox.Location = new System.Drawing.Point(99, 3);
            this.vaultListComboBox.Name = "vaultListComboBox";
            this.vaultListComboBox.Size = new System.Drawing.Size(481, 28);
            this.vaultListComboBox.TabIndex = 5;
            this.vaultListComboBox.SelectedIndexChanged += new System.EventHandler(this.VaultListComboBoxSelectedIndexChanged);
            // 
            // vaultLabel
            // 
            this.vaultLabel.BackColor = System.Drawing.Color.Transparent;
            this.vaultLabel.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.vaultLabel.ForeColor = System.Drawing.Color.White;
            this.vaultLabel.Location = new System.Drawing.Point(3, 0);
            this.vaultLabel.Name = "vaultLabel";
            this.vaultLabel.Size = new System.Drawing.Size(90, 24);
            this.vaultLabel.TabIndex = 6;
            this.vaultLabel.Text = "Vault:";
            this.vaultLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // configureButton
            // 
            this.configureButton.BackColor = System.Drawing.Color.Transparent;
            this.configureButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("configureButton.DownBitmap")));
            this.configureButton.FlatAppearance.BorderSize = 0;
            this.configureButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.configureButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.configureButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.configureButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.configureButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.configureButton.Image = ((System.Drawing.Image)(resources.GetObject("configureButton.Image")));
            this.configureButton.Location = new System.Drawing.Point(3, 3);
            this.configureButton.Name = "configureButton";
            this.configureButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("configureButton.OverBitmap")));
            this.configureButton.Size = new System.Drawing.Size(137, 30);
            this.configureButton.SizeToGraphic = false;
            this.configureButton.TabIndex = 9;
            this.configureButton.Text = "Configure";
            this.configureButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("configureButton.UpBitmap")));
            this.configureButton.UseCustomGraphic = true;
            this.configureButton.UseVisualStyleBackColor = false;
            this.configureButton.Click += new System.EventHandler(this.ConfigureButtonClick);
            // 
            // customMapText
            // 
            this.customMapText.BackColor = System.Drawing.Color.Gold;
            this.customMapText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.customMapText.Dock = System.Windows.Forms.DockStyle.Right;
            this.customMapText.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.customMapText.ForeColor = System.Drawing.Color.Black;
            this.customMapText.Location = new System.Drawing.Point(216, 84);
            this.customMapText.Name = "customMapText";
            this.customMapText.Size = new System.Drawing.Size(380, 23);
            this.customMapText.TabIndex = 11;
            this.customMapText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // showVaulButton
            // 
            this.showVaulButton.BackColor = System.Drawing.Color.Transparent;
            this.showVaulButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("showVaulButton.DownBitmap")));
            this.showVaulButton.FlatAppearance.BorderSize = 0;
            this.showVaulButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.showVaulButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.showVaulButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.showVaulButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.showVaulButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.showVaulButton.Image = ((System.Drawing.Image)(resources.GetObject("showVaulButton.Image")));
            this.showVaulButton.Location = new System.Drawing.Point(146, 3);
            this.showVaulButton.Name = "showVaulButton";
            this.showVaulButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("showVaulButton.OverBitmap")));
            this.showVaulButton.Size = new System.Drawing.Size(137, 30);
            this.showVaulButton.SizeToGraphic = false;
            this.showVaulButton.TabIndex = 12;
            this.showVaulButton.Text = "Show Vault";
            this.showVaulButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("showVaulButton.UpBitmap")));
            this.showVaulButton.UseCustomGraphic = true;
            this.showVaulButton.UseVisualStyleBackColor = false;
            this.showVaulButton.Click += new System.EventHandler(this.PanelSelectButtonClick);
            // 
            // aboutButton
            // 
            this.aboutButton.BackColor = System.Drawing.Color.Transparent;
            this.aboutButton.DownBitmap = null;
            this.aboutButton.FlatAppearance.BorderSize = 0;
            this.aboutButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.aboutButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.aboutButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.aboutButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.aboutButton.Image = ((System.Drawing.Image)(resources.GetObject("aboutButton.Image")));
            this.aboutButton.Location = new System.Drawing.Point(1130, 21);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.OverBitmap = null;
            this.aboutButton.Size = new System.Drawing.Size(48, 38);
            this.aboutButton.SizeToGraphic = false;
            this.aboutButton.TabIndex = 16;
            this.aboutButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("aboutButton.UpBitmap")));
            this.aboutButton.UseCustomGraphic = true;
            this.aboutButton.UseVisualStyleBackColor = false;
            this.aboutButton.Click += new System.EventHandler(this.AboutButtonClick);
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.BackColor = System.Drawing.Color.Transparent;
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(195)))), ((int)(((byte)(112)))));
            this.titleLabel.Location = new System.Drawing.Point(1180, 18);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(136, 37);
            this.titleLabel.TabIndex = 17;
            this.titleLabel.Text = "TQVault";
            // 
            // searchButton
            // 
            this.searchButton.BackColor = System.Drawing.Color.Transparent;
            this.searchButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("searchButton.DownBitmap")));
            this.searchButton.FlatAppearance.BorderSize = 0;
            this.searchButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.searchButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.searchButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.searchButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
            this.searchButton.Location = new System.Drawing.Point(289, 3);
            this.searchButton.Name = "searchButton";
            this.searchButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("searchButton.OverBitmap")));
            this.searchButton.Size = new System.Drawing.Size(137, 30);
            this.searchButton.SizeToGraphic = false;
            this.searchButton.TabIndex = 18;
            this.searchButton.Text = "Search";
            this.searchButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("searchButton.UpBitmap")));
            this.searchButton.UseCustomGraphic = true;
            this.searchButton.UseVisualStyleBackColor = false;
            this.searchButton.Click += new System.EventHandler(this.SearchButtonClick);
            // 
            // backgroundWorkerLoadAllFiles
            // 
            this.backgroundWorkerLoadAllFiles.WorkerReportsProgress = true;
            this.backgroundWorkerLoadAllFiles.WorkerSupportsCancellation = true;
            this.backgroundWorkerLoadAllFiles.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorkerLoadAllFiles_DoWork);
            this.backgroundWorkerLoadAllFiles.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorkerLoadAllFiles_ProgressChanged);
            this.backgroundWorkerLoadAllFiles.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorkerLoadAllFiles_RunWorkerCompleted);
            // 
            // fadeInTimer
            // 
            this.fadeInTimer.Interval = 50;
            this.fadeInTimer.Tick += new System.EventHandler(this.FadeInTimerTick);
            // 
            // flowLayoutPanelVaultSelector
            // 
            this.flowLayoutPanelVaultSelector.AutoSize = true;
            this.flowLayoutPanelVaultSelector.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelVaultSelector.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanelVaultSelector.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flowLayoutPanelVaultSelector.Controls.Add(this.vaultLabel);
            this.flowLayoutPanelVaultSelector.Controls.Add(this.vaultListComboBox);
            this.flowLayoutPanelVaultSelector.Location = new System.Drawing.Point(4, 4);
            this.flowLayoutPanelVaultSelector.Name = "flowLayoutPanelVaultSelector";
            this.flowLayoutPanelVaultSelector.Size = new System.Drawing.Size(587, 38);
            this.flowLayoutPanelVaultSelector.TabIndex = 20;
            // 
            // tableLayoutPanelMain
            // 
            this.tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanelMain.AutoSize = true;
            this.tableLayoutPanelMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelMain.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanelMain.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanelMain.ColumnCount = 3;
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelHighlight, 0, 3);
            this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelRightPanels, 2, 1);
            this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelVaultSelector, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelRightComboBox, 2, 0);
            this.tableLayoutPanelMain.Controls.Add(this.customMapText, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.itemTextPanel, 0, 4);
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(15, 60);
            this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 5;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(1320, 178);
            this.tableLayoutPanelMain.TabIndex = 21;
            // 
            // tableLayoutPanelHighlight
            // 
            this.tableLayoutPanelHighlight.AutoSize = true;
            this.tableLayoutPanelHighlight.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanelHighlight.ColumnCount = 2;
            this.tableLayoutPanelHighlight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelHighlight.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanelHighlight.Controls.Add(this.scalingLabelHighlight, 0, 0);
            this.tableLayoutPanelHighlight.Controls.Add(this.scalingTextBoxHighlight, 1, 0);
            this.tableLayoutPanelHighlight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanelHighlight.Location = new System.Drawing.Point(1, 108);
            this.tableLayoutPanelHighlight.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelHighlight.Name = "tableLayoutPanelHighlight";
            this.tableLayoutPanelHighlight.RowCount = 1;
            this.tableLayoutPanelHighlight.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelHighlight.Size = new System.Drawing.Size(598, 23);
            this.tableLayoutPanelHighlight.TabIndex = 24;
            // 
            // scalingLabelHighlight
            // 
            this.scalingLabelHighlight.AutoSize = true;
            this.scalingLabelHighlight.BackColor = System.Drawing.Color.Transparent;
            this.scalingLabelHighlight.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingLabelHighlight.LinkColor = System.Drawing.Color.Gold;
            this.scalingLabelHighlight.Location = new System.Drawing.Point(3, 0);
            this.scalingLabelHighlight.Name = "scalingLabelHighlight";
            this.scalingLabelHighlight.Size = new System.Drawing.Size(135, 15);
            this.scalingLabelHighlight.TabIndex = 25;
            this.scalingLabelHighlight.TabStop = true;
            this.scalingLabelHighlight.Text = "Highlight items having :";
            this.scalingLabelHighlight.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(255)))), ((int)(((byte)(64)))));
            this.scalingLabelHighlight.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.scalingLabelHighlight_LinkClicked);
            // 
            // scalingTextBoxHighlight
            // 
            this.scalingTextBoxHighlight.Font = new System.Drawing.Font("Albertus MT Light", 9.75F);
            this.scalingTextBoxHighlight.Location = new System.Drawing.Point(141, 0);
            this.scalingTextBoxHighlight.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.scalingTextBoxHighlight.Name = "scalingTextBoxHighlight";
            this.scalingTextBoxHighlight.Size = new System.Drawing.Size(262, 23);
            this.scalingTextBoxHighlight.TabIndex = 2;
            this.scalingTextBoxHighlight.TextChanged += new System.EventHandler(this.scalingTextBoxHighlight_TextChanged);
            // 
            // flowLayoutPanelRightPanels
            // 
            this.flowLayoutPanelRightPanels.AutoSize = true;
            this.flowLayoutPanelRightPanels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelRightPanels.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanelRightPanels.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flowLayoutPanelRightPanels.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelRightPanels.Location = new System.Drawing.Point(606, 77);
            this.flowLayoutPanelRightPanels.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelRightPanels.MinimumSize = new System.Drawing.Size(200, 100);
            this.flowLayoutPanelRightPanels.Name = "flowLayoutPanelRightPanels";
            this.tableLayoutPanelMain.SetRowSpan(this.flowLayoutPanelRightPanels, 4);
            this.flowLayoutPanelRightPanels.Size = new System.Drawing.Size(200, 100);
            this.flowLayoutPanelRightPanels.TabIndex = 22;
            // 
            // flowLayoutPanelRightComboBox
            // 
            this.flowLayoutPanelRightComboBox.AutoSize = true;
            this.flowLayoutPanelRightComboBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelRightComboBox.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanelRightComboBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flowLayoutPanelRightComboBox.Controls.Add(this.characterLabel);
            this.flowLayoutPanelRightComboBox.Controls.Add(this.characterComboBox);
            this.flowLayoutPanelRightComboBox.Controls.Add(this.secondaryVaultListComboBox);
            this.flowLayoutPanelRightComboBox.Location = new System.Drawing.Point(609, 4);
            this.flowLayoutPanelRightComboBox.Name = "flowLayoutPanelRightComboBox";
            this.flowLayoutPanelRightComboBox.Size = new System.Drawing.Size(587, 69);
            this.flowLayoutPanelRightComboBox.TabIndex = 20;
            // 
            // characterLabel
            // 
            this.characterLabel.BackColor = System.Drawing.Color.Transparent;
            this.characterLabel.Font = new System.Drawing.Font("Albertus MT Light", 11.25F);
            this.characterLabel.ForeColor = System.Drawing.Color.White;
            this.characterLabel.Location = new System.Drawing.Point(3, 0);
            this.characterLabel.Name = "characterLabel";
            this.characterLabel.Size = new System.Drawing.Size(90, 24);
            this.characterLabel.TabIndex = 2;
            this.characterLabel.Text = "Character:";
            this.characterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // characterComboBox
            // 
            this.characterComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.characterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.characterComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F);
            this.characterComboBox.Location = new System.Drawing.Point(99, 3);
            this.characterComboBox.MaxDropDownItems = 10;
            this.characterComboBox.Name = "characterComboBox";
            this.characterComboBox.Size = new System.Drawing.Size(481, 28);
            this.characterComboBox.TabIndex = 1;
            this.characterComboBox.SelectedIndexChanged += new System.EventHandler(this.CharacterComboBoxSelectedIndexChanged);
            // 
            // secondaryVaultListComboBox
            // 
            this.secondaryVaultListComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.secondaryVaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.secondaryVaultListComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.secondaryVaultListComboBox.Location = new System.Drawing.Point(3, 37);
            this.secondaryVaultListComboBox.MaxDropDownItems = 10;
            this.secondaryVaultListComboBox.Name = "secondaryVaultListComboBox";
            this.secondaryVaultListComboBox.Size = new System.Drawing.Size(481, 25);
            this.secondaryVaultListComboBox.TabIndex = 15;
            this.secondaryVaultListComboBox.SelectedIndexChanged += new System.EventHandler(this.SecondaryVaultListComboBoxSelectedIndexChanged);
            // 
            // duplicateButton
            // 
            this.duplicateButton.BackColor = System.Drawing.Color.Transparent;
            this.duplicateButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.duplicateButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("duplicateButton.DownBitmap")));
            this.duplicateButton.FlatAppearance.BorderSize = 0;
            this.duplicateButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.duplicateButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.duplicateButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.duplicateButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.duplicateButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.duplicateButton.Image = ((System.Drawing.Image)(resources.GetObject("duplicateButton.Image")));
            this.duplicateButton.Location = new System.Drawing.Point(432, 3);
            this.duplicateButton.Name = "duplicateButton";
            this.duplicateButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("duplicateButton.OverBitmap")));
            this.duplicateButton.Size = new System.Drawing.Size(137, 30);
            this.duplicateButton.SizeToGraphic = false;
            this.duplicateButton.TabIndex = 22;
            this.duplicateButton.Text = "Duplicate";
            this.duplicateButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("duplicateButton.UpBitmap")));
            this.duplicateButton.UseCustomGraphic = true;
            this.duplicateButton.UseVisualStyleBackColor = false;
            this.duplicateButton.Click += new System.EventHandler(this.duplicateButton_Click);
            // 
            // fileSystemWatcherTransferStash
            // 
            this.fileSystemWatcherTransferStash.EnableRaisingEvents = true;
            this.fileSystemWatcherTransferStash.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.CreationTime)));
            this.fileSystemWatcherTransferStash.SynchronizingObject = this;
            this.fileSystemWatcherTransferStash.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcherTransferStash_Changed);
            // 
            // fileSystemWatcherRelicStash
            // 
            this.fileSystemWatcherRelicStash.EnableRaisingEvents = true;
            this.fileSystemWatcherRelicStash.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.CreationTime)));
            this.fileSystemWatcherRelicStash.SynchronizingObject = this;
            this.fileSystemWatcherRelicStash.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcherRelicStash_Changed);
            // 
            // typeAssistant
            // 
            this.typeAssistant.Idled += new System.EventHandler(this.typeAssistant_Idled);
            // 
            // saveButton
            // 
            this.saveButton.BackColor = System.Drawing.Color.Transparent;
            this.saveButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.saveButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("saveButton.DownBitmap")));
            this.saveButton.FlatAppearance.BorderSize = 0;
            this.saveButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.saveButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveButton.Font = new System.Drawing.Font("Albertus MT Light", 12F);
            this.saveButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.saveButton.Image = ((System.Drawing.Image)(resources.GetObject("saveButton.Image")));
            this.saveButton.Location = new System.Drawing.Point(575, 3);
            this.saveButton.Name = "saveButton";
            this.saveButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("saveButton.OverBitmap")));
            this.saveButton.Size = new System.Drawing.Size(137, 30);
            this.saveButton.SizeToGraphic = false;
            this.saveButton.TabIndex = 23;
            this.saveButton.Text = "Save";
            this.saveButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("saveButton.UpBitmap")));
            this.saveButton.UseCustomGraphic = true;
            this.saveButton.UseVisualStyleBackColor = false;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // flowLayoutPanelMenuButtons
            // 
            this.flowLayoutPanelMenuButtons.AutoSize = true;
            this.flowLayoutPanelMenuButtons.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelMenuButtons.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanelMenuButtons.Controls.Add(this.configureButton);
            this.flowLayoutPanelMenuButtons.Controls.Add(this.showVaulButton);
            this.flowLayoutPanelMenuButtons.Controls.Add(this.searchButton);
            this.flowLayoutPanelMenuButtons.Controls.Add(this.duplicateButton);
            this.flowLayoutPanelMenuButtons.Controls.Add(this.saveButton);
            this.flowLayoutPanelMenuButtons.Controls.Add(this.exitButton);
            this.flowLayoutPanelMenuButtons.Location = new System.Drawing.Point(15, 18);
            this.flowLayoutPanelMenuButtons.Name = "flowLayoutPanelMenuButtons";
            this.flowLayoutPanelMenuButtons.Size = new System.Drawing.Size(858, 36);
            this.flowLayoutPanelMenuButtons.TabIndex = 24;
            // 
            // highlightFilters
            // 
            this.highlightFilters.AutoSize = true;
            this.highlightFilters.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.highlightFilters.BackColor = System.Drawing.Color.Gold;
            this.highlightFilters.Location = new System.Drawing.Point(73, 288);
            this.highlightFilters.Name = "highlightFilters";
            this.highlightFilters.Padding = new System.Windows.Forms.Padding(1);
            this.highlightFilters.Size = new System.Drawing.Size(482, 239);
            this.highlightFilters.TabIndex = 25;
            this.highlightFilters.Visible = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1350, 910);
            this.ConstrainToDesignRatio = true;
            this.Controls.Add(this.highlightFilters);
            this.Controls.Add(this.flowLayoutPanelMenuButtons);
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.titleLabel);
            this.DrawCustomBorder = true;
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.ResizeCustomAllowed = true;
            this.ScaleOnResize = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Titan Quest Item Vault";
            this.TitleTextColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainFormClosing);
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.Shown += new System.EventHandler(this.MainFormShown);
            this.ResizeEnd += new System.EventHandler(this.ResizeEndCallback);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainFormKeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainFormKeyPress);
            this.Resize += new System.EventHandler(this.ResizeBeginCallback);
            this.Controls.SetChildIndex(this.titleLabel, 0);
            this.Controls.SetChildIndex(this.aboutButton, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanelMain, 0);
            this.Controls.SetChildIndex(this.flowLayoutPanelMenuButtons, 0);
            this.Controls.SetChildIndex(this.highlightFilters, 0);
            this.itemTextPanel.ResumeLayout(false);
            this.flowLayoutPanelVaultSelector.ResumeLayout(false);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            this.tableLayoutPanelHighlight.ResumeLayout(false);
            this.tableLayoutPanelHighlight.PerformLayout();
            this.flowLayoutPanelRightComboBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcherTransferStash)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcherRelicStash)).EndInit();
            this.flowLayoutPanelMenuButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private BufferedFlowLayoutPanel flowLayoutPanelVaultSelector;
		private BufferedTableLayoutPanel tableLayoutPanelMain;
		private BufferedFlowLayoutPanel flowLayoutPanelRightComboBox;
		private ScalingLabel characterLabel;
		private ScalingComboBox characterComboBox;
		private ScalingComboBox secondaryVaultListComboBox;
		private BufferedFlowLayoutPanel flowLayoutPanelRightPanels;
		private ScalingButton duplicateButton;
		private System.IO.FileSystemWatcher fileSystemWatcherTransferStash;
		private System.IO.FileSystemWatcher fileSystemWatcherRelicStash;
		private ScalingButton saveButton;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelMenuButtons;
		private System.Windows.Forms.ToolTip toolTip;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelHighlight;
		private ScalingTextBox scalingTextBoxHighlight;
		private TypeAssistant typeAssistant;
		private System.Windows.Forms.LinkLabel scalingLabelHighlight;
		private HighlightFilters highlightFilters;
	}
}