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
		private System.ComponentModel.BackgroundWorker backgroundWorker1;

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
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.fadeInTimer = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanelVaultSelector = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanelRightPanels = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanelRightComboBox = new System.Windows.Forms.FlowLayoutPanel();
            this.characterLabel = new TQVaultAE.GUI.Components.ScalingLabel();
            this.characterComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.secondaryVaultListComboBox = new TQVaultAE.GUI.Components.ScalingComboBox();
            this.itemTextPanel.SuspendLayout();
            this.flowLayoutPanelVaultSelector.SuspendLayout();
            this.tableLayoutPanelMain.SuspendLayout();
            this.flowLayoutPanelRightComboBox.SuspendLayout();
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
            this.exitButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.exitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.exitButton.Image = ((System.Drawing.Image)(resources.GetObject("exitButton.Image")));
            this.exitButton.Location = new System.Drawing.Point(542, 30);
            this.exitButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.exitButton.Name = "exitButton";
            this.exitButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("exitButton.OverBitmap")));
            this.exitButton.Size = new System.Drawing.Size(171, 38);
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
            this.itemTextPanel.Location = new System.Drawing.Point(5, 176);
            this.itemTextPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 25);
            this.itemTextPanel.Name = "itemTextPanel";
            this.itemTextPanel.Padding = new System.Windows.Forms.Padding(2);
            this.itemTextPanel.Size = new System.Drawing.Size(740, 28);
            this.itemTextPanel.TabIndex = 4;
            // 
            // itemText
            // 
            this.itemText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
            this.itemText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.itemText.Font = new System.Drawing.Font("Arial Black", 10.3125F);
            this.itemText.Location = new System.Drawing.Point(2, 2);
            this.itemText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.itemText.Name = "itemText";
            this.itemText.Size = new System.Drawing.Size(736, 24);
            this.itemText.TabIndex = 0;
            // 
            // vaultListComboBox
            // 
            this.vaultListComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.vaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vaultListComboBox.Font = new System.Drawing.Font("Albertus MT Light", 16.25F);
            this.vaultListComboBox.Location = new System.Drawing.Point(124, 4);
            this.vaultListComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.vaultListComboBox.Name = "vaultListComboBox";
            this.vaultListComboBox.Size = new System.Drawing.Size(600, 40);
            this.vaultListComboBox.TabIndex = 5;
            this.vaultListComboBox.SelectedIndexChanged += new System.EventHandler(this.VaultListComboBoxSelectedIndexChanged);
            // 
            // vaultLabel
            // 
            this.vaultLabel.BackColor = System.Drawing.Color.Transparent;
            this.vaultLabel.Font = new System.Drawing.Font("Albertus MT Light", 13.75F);
            this.vaultLabel.ForeColor = System.Drawing.Color.White;
            this.vaultLabel.Location = new System.Drawing.Point(4, 0);
            this.vaultLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.vaultLabel.Name = "vaultLabel";
            this.vaultLabel.Size = new System.Drawing.Size(112, 30);
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
            this.configureButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.configureButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.configureButton.Image = ((System.Drawing.Image)(resources.GetObject("configureButton.Image")));
            this.configureButton.Location = new System.Drawing.Point(8, 30);
            this.configureButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.configureButton.Name = "configureButton";
            this.configureButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("configureButton.OverBitmap")));
            this.configureButton.Size = new System.Drawing.Size(171, 38);
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
            this.customMapText.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.customMapText.BackColor = System.Drawing.Color.Gold;
            this.customMapText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.customMapText.Font = new System.Drawing.Font("Albertus MT", 14.0625F);
            this.customMapText.ForeColor = System.Drawing.Color.Black;
            this.customMapText.Location = new System.Drawing.Point(271, 128);
            this.customMapText.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.customMapText.Name = "customMapText";
            this.customMapText.Size = new System.Drawing.Size(474, 37);
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
            this.showVaulButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.showVaulButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.showVaulButton.Image = ((System.Drawing.Image)(resources.GetObject("showVaulButton.Image")));
            this.showVaulButton.Location = new System.Drawing.Point(185, 30);
            this.showVaulButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.showVaulButton.Name = "showVaulButton";
            this.showVaulButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("showVaulButton.OverBitmap")));
            this.showVaulButton.Size = new System.Drawing.Size(171, 38);
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
            this.aboutButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.3125F);
            this.aboutButton.Image = ((System.Drawing.Image)(resources.GetObject("aboutButton.Image")));
            this.aboutButton.Location = new System.Drawing.Point(1412, 26);
            this.aboutButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.OverBitmap = null;
            this.aboutButton.Size = new System.Drawing.Size(60, 48);
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
            this.titleLabel.Font = new System.Drawing.Font("Albertus MT Light", 30F);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(216)))), ((int)(((byte)(195)))), ((int)(((byte)(112)))));
            this.titleLabel.Location = new System.Drawing.Point(1475, 22);
            this.titleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(209, 59);
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
            this.searchButton.Font = new System.Drawing.Font("Albertus MT Light", 15F);
            this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
            this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
            this.searchButton.Location = new System.Drawing.Point(364, 30);
            this.searchButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.searchButton.Name = "searchButton";
            this.searchButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("searchButton.OverBitmap")));
            this.searchButton.Size = new System.Drawing.Size(171, 38);
            this.searchButton.SizeToGraphic = false;
            this.searchButton.TabIndex = 18;
            this.searchButton.Text = "Search";
            this.searchButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("searchButton.UpBitmap")));
            this.searchButton.UseCustomGraphic = true;
            this.searchButton.UseVisualStyleBackColor = false;
            this.searchButton.Click += new System.EventHandler(this.SearchButtonClick);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.BackgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.BackgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.BackgroundWorker1_RunWorkerCompleted);
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
            this.flowLayoutPanelVaultSelector.Location = new System.Drawing.Point(5, 5);
            this.flowLayoutPanelVaultSelector.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flowLayoutPanelVaultSelector.Name = "flowLayoutPanelVaultSelector";
            this.flowLayoutPanelVaultSelector.Size = new System.Drawing.Size(732, 52);
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
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelRightPanels, 2, 1);
            this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelVaultSelector, 0, 0);
            this.tableLayoutPanelMain.Controls.Add(this.flowLayoutPanelRightComboBox, 2, 0);
            this.tableLayoutPanelMain.Controls.Add(this.customMapText, 0, 2);
            this.tableLayoutPanelMain.Controls.Add(this.itemTextPanel, 0, 3);
            this.tableLayoutPanelMain.Location = new System.Drawing.Point(19, 75);
            this.tableLayoutPanelMain.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanelMain.Name = "tableLayoutPanelMain";
            this.tableLayoutPanelMain.RowCount = 4;
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanelMain.Size = new System.Drawing.Size(1650, 230);
            this.tableLayoutPanelMain.TabIndex = 21;
            // 
            // flowLayoutPanelRightPanels
            // 
            this.flowLayoutPanelRightPanels.AutoSize = true;
            this.flowLayoutPanelRightPanels.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanelRightPanels.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanelRightPanels.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.flowLayoutPanelRightPanels.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanelRightPanels.Location = new System.Drawing.Point(757, 105);
            this.flowLayoutPanelRightPanels.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanelRightPanels.MinimumSize = new System.Drawing.Size(249, 124);
            this.flowLayoutPanelRightPanels.Name = "flowLayoutPanelRightPanels";
            this.tableLayoutPanelMain.SetRowSpan(this.flowLayoutPanelRightPanels, 3);
            this.flowLayoutPanelRightPanels.Size = new System.Drawing.Size(249, 124);
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
            this.flowLayoutPanelRightComboBox.Location = new System.Drawing.Point(761, 5);
            this.flowLayoutPanelRightComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.flowLayoutPanelRightComboBox.Name = "flowLayoutPanelRightComboBox";
            this.flowLayoutPanelRightComboBox.Size = new System.Drawing.Size(732, 95);
            this.flowLayoutPanelRightComboBox.TabIndex = 20;
            // 
            // characterLabel
            // 
            this.characterLabel.BackColor = System.Drawing.Color.Transparent;
            this.characterLabel.Font = new System.Drawing.Font("Albertus MT Light", 13.75F);
            this.characterLabel.ForeColor = System.Drawing.Color.White;
            this.characterLabel.Location = new System.Drawing.Point(4, 0);
            this.characterLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.characterLabel.Name = "characterLabel";
            this.characterLabel.Size = new System.Drawing.Size(112, 30);
            this.characterLabel.TabIndex = 2;
            this.characterLabel.Text = "Character:";
            this.characterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // characterComboBox
            // 
            this.characterComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.characterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.characterComboBox.Font = new System.Drawing.Font("Albertus MT Light", 16.25F);
            this.characterComboBox.Location = new System.Drawing.Point(124, 4);
            this.characterComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.characterComboBox.MaxDropDownItems = 10;
            this.characterComboBox.Name = "characterComboBox";
            this.characterComboBox.Size = new System.Drawing.Size(600, 40);
            this.characterComboBox.TabIndex = 1;
            this.characterComboBox.SelectedIndexChanged += new System.EventHandler(this.CharacterComboBoxSelectedIndexChanged);
            // 
            // secondaryVaultListComboBox
            // 
            this.secondaryVaultListComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.secondaryVaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.secondaryVaultListComboBox.Font = new System.Drawing.Font("Albertus MT Light", 13.75F);
            this.secondaryVaultListComboBox.Location = new System.Drawing.Point(4, 52);
            this.secondaryVaultListComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.secondaryVaultListComboBox.MaxDropDownItems = 10;
            this.secondaryVaultListComboBox.Name = "secondaryVaultListComboBox";
            this.secondaryVaultListComboBox.Size = new System.Drawing.Size(600, 35);
            this.secondaryVaultListComboBox.TabIndex = 15;
            this.secondaryVaultListComboBox.SelectedIndexChanged += new System.EventHandler(this.SecondaryVaultListComboBoxSelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.CancelButton = this.exitButton;
            this.ClientSize = new System.Drawing.Size(1688, 1138);
            this.ConstrainToDesignRatio = true;
            this.Controls.Add(this.tableLayoutPanelMain);
            this.Controls.Add(this.searchButton);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.showVaulButton);
            this.Controls.Add(this.configureButton);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.titleLabel);
            this.DrawCustomBorder = true;
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
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
            this.Controls.SetChildIndex(this.exitButton, 0);
            this.Controls.SetChildIndex(this.configureButton, 0);
            this.Controls.SetChildIndex(this.showVaulButton, 0);
            this.Controls.SetChildIndex(this.aboutButton, 0);
            this.Controls.SetChildIndex(this.searchButton, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanelMain, 0);
            this.itemTextPanel.ResumeLayout(false);
            this.flowLayoutPanelVaultSelector.ResumeLayout(false);
            this.tableLayoutPanelMain.ResumeLayout(false);
            this.tableLayoutPanelMain.PerformLayout();
            this.flowLayoutPanelRightComboBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelVaultSelector;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelMain;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelRightComboBox;
		private ScalingLabel characterLabel;
		private ScalingComboBox characterComboBox;
		private ScalingComboBox secondaryVaultListComboBox;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelRightPanels;
	}
}