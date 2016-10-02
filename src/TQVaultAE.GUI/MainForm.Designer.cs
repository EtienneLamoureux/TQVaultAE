//-----------------------------------------------------------------------
// <copyright file="MainForm.Designer.cs" company="None">
//     Copyright (c) Brandon Wallace and Jesse Calhoun. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using TQVaultAE.GUI.Properties;

namespace TQVaultAE.GUI
{
	/// <summary>
	/// MainForm Designer class
	/// </summary>
	internal partial class MainForm
	{
		/// <summary>
		/// Windows Form Exit button
		/// </summary>
		private ScalingButton exitButton;

		/// <summary>
		/// Windows Form Character dropdown
		/// </summary>
		private ScalingComboBox characterComboBox;

		/// <summary>
		/// Windows Form Character Label
		/// </summary>
		private ScalingLabel characterLabel;

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
		private ScalingButton panelSelectButton;

		/// <summary>
		/// Windows Form Search Text Box
		/// </summary>
		private ScalingTextBox searchTextBox;

		/// <summary>
		/// Windows Form Find Label
		/// </summary>
		private ScalingLabel findLabel;

		/// <summary>
		/// Windows Form Secondary Vault List Dropdown
		/// </summary>
		private ScalingComboBox secondaryVaultListComboBox;

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
		/// Windows Form Label for the currently loaded character
		/// </summary>
		private ScalingLabel loadedCharacterLabel;

		/// <summary>
		/// Windows Form Label for the currently loaded vault
		/// </summary>
		private ScalingLabel loadedVaultLabel;

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
			this.exitButton = new ScalingButton();
			this.characterComboBox = new ScalingComboBox();
			this.characterLabel = new ScalingLabel();
			this.itemTextPanel = new System.Windows.Forms.Panel();
			this.itemText = new ScalingLabel();
			this.vaultListComboBox = new ScalingComboBox();
			this.vaultLabel = new ScalingLabel();
			this.configureButton = new ScalingButton();
			this.customMapText = new ScalingLabel();
			this.panelSelectButton = new ScalingButton();
			this.searchTextBox = new ScalingTextBox();
			this.findLabel = new ScalingLabel();
			this.secondaryVaultListComboBox = new ScalingComboBox();
			this.aboutButton = new ScalingButton();
			this.titleLabel = new ScalingLabel();
			this.searchButton = new ScalingButton();
			this.loadedCharacterLabel = new ScalingLabel();
			this.loadedVaultLabel = new ScalingLabel();
			this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
			this.fadeInTimer = new System.Windows.Forms.Timer(this.components);
			this.itemTextPanel.SuspendLayout();
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
			this.exitButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.exitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.exitButton.Image = ((System.Drawing.Image)(resources.GetObject("exitButton.Image")));
			this.exitButton.Location = new System.Drawing.Point(434, 21);
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
			// characterComboBox
			// 
			this.characterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.characterComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
			this.characterComboBox.Location = new System.Drawing.Point(148, 92);
			this.characterComboBox.MaxDropDownItems = 10;
			this.characterComboBox.Name = "characterComboBox";
			this.characterComboBox.Size = new System.Drawing.Size(481, 26);
			this.characterComboBox.TabIndex = 1;
			this.characterComboBox.SelectedIndexChanged += new System.EventHandler(this.CharacterComboBoxSelectedIndexChanged);
			// 
			// characterLabel
			// 
			this.characterLabel.BackColor = System.Drawing.Color.Transparent;
			this.characterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
			this.characterLabel.ForeColor = System.Drawing.Color.White;
			this.characterLabel.Location = new System.Drawing.Point(3, 85);
			this.characterLabel.Name = "characterLabel";
			this.characterLabel.Size = new System.Drawing.Size(139, 37);
			this.characterLabel.TabIndex = 2;
			this.characterLabel.Text = "Character:";
			this.characterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// itemTextPanel
			// 
			this.itemTextPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(188)))), ((int)(((byte)(97)))));
			this.itemTextPanel.Controls.Add(this.itemText);
			this.itemTextPanel.Location = new System.Drawing.Point(6, 606);
			this.itemTextPanel.Name = "itemTextPanel";
			this.itemTextPanel.Size = new System.Drawing.Size(592, 22);
			this.itemTextPanel.TabIndex = 4;
			// 
			// itemText
			// 
			this.itemText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(46)))), ((int)(((byte)(41)))), ((int)(((byte)(31)))));
			this.itemText.Font = new System.Drawing.Font("Arial Black", 8.25F);
			this.itemText.Location = new System.Drawing.Point(2, 2);
			this.itemText.Name = "itemText";
			this.itemText.Size = new System.Drawing.Size(576, 18);
			this.itemText.TabIndex = 0;
			// 
			// vaultListComboBox
			// 
			this.vaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.vaultListComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
			this.vaultListComboBox.Location = new System.Drawing.Point(148, 62);
			this.vaultListComboBox.Name = "vaultListComboBox";
			this.vaultListComboBox.Size = new System.Drawing.Size(481, 26);
			this.vaultListComboBox.TabIndex = 5;
			this.vaultListComboBox.SelectedIndexChanged += new System.EventHandler(this.VaultListComboBoxSelectedIndexChanged);
			// 
			// vaultLabel
			// 
			this.vaultLabel.BackColor = System.Drawing.Color.Transparent;
			this.vaultLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
			this.vaultLabel.ForeColor = System.Drawing.Color.White;
			this.vaultLabel.Location = new System.Drawing.Point(27, 63);
			this.vaultLabel.Name = "vaultLabel";
			this.vaultLabel.Size = new System.Drawing.Size(116, 24);
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
			this.configureButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.configureButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.configureButton.Image = ((System.Drawing.Image)(resources.GetObject("configureButton.Image")));
			this.configureButton.Location = new System.Drawing.Point(6, 21);
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
			this.customMapText.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
			this.customMapText.ForeColor = System.Drawing.Color.Black;
			this.customMapText.Location = new System.Drawing.Point(446, 330);
			this.customMapText.Name = "customMapText";
			this.customMapText.Size = new System.Drawing.Size(253, 46);
			this.customMapText.TabIndex = 11;
			this.customMapText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// panelSelectButton
			// 
			this.panelSelectButton.BackColor = System.Drawing.Color.Transparent;
			this.panelSelectButton.DownBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelSelectButton.DownBitmap")));
			this.panelSelectButton.FlatAppearance.BorderSize = 0;
			this.panelSelectButton.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.panelSelectButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.panelSelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.panelSelectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.panelSelectButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.panelSelectButton.Image = ((System.Drawing.Image)(resources.GetObject("panelSelectButton.Image")));
			this.panelSelectButton.Location = new System.Drawing.Point(148, 21);
			this.panelSelectButton.Name = "panelSelectButton";
			this.panelSelectButton.OverBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelSelectButton.OverBitmap")));
			this.panelSelectButton.Size = new System.Drawing.Size(137, 30);
			this.panelSelectButton.SizeToGraphic = false;
			this.panelSelectButton.TabIndex = 12;
			this.panelSelectButton.Text = "Show Vault";
			this.panelSelectButton.UpBitmap = ((System.Drawing.Bitmap)(resources.GetObject("panelSelectButton.UpBitmap")));
			this.panelSelectButton.UseCustomGraphic = true;
			this.panelSelectButton.UseVisualStyleBackColor = false;
			this.panelSelectButton.Click += new System.EventHandler(this.PanelSelectButtonClick);
			// 
			// searchTextBox
			// 
			this.searchTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.searchTextBox.Location = new System.Drawing.Point(252, 263);
			this.searchTextBox.Name = "searchTextBox";
			this.searchTextBox.Size = new System.Drawing.Size(413, 20);
			this.searchTextBox.TabIndex = 13;
			this.searchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SearchTextBoxKeyDown);
			this.searchTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.SearchTextBoxKeyPress);
			// 
			// findLabel
			// 
			this.findLabel.BackColor = System.Drawing.Color.Transparent;
			this.findLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
			this.findLabel.Location = new System.Drawing.Point(179, 266);
			this.findLabel.Name = "findLabel";
			this.findLabel.Size = new System.Drawing.Size(62, 13);
			this.findLabel.TabIndex = 14;
			this.findLabel.Text = "Find:";
			this.findLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// secondaryVaultListComboBox
			// 
			this.secondaryVaultListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.secondaryVaultListComboBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
			this.secondaryVaultListComboBox.Location = new System.Drawing.Point(148, 106);
			this.secondaryVaultListComboBox.MaxDropDownItems = 10;
			this.secondaryVaultListComboBox.Name = "secondaryVaultListComboBox";
			this.secondaryVaultListComboBox.Size = new System.Drawing.Size(481, 26);
			this.secondaryVaultListComboBox.TabIndex = 15;
			this.secondaryVaultListComboBox.SelectedIndexChanged += new System.EventHandler(this.SecondaryVaultListComboBoxSelectedIndexChanged);
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
			this.aboutButton.Location = new System.Drawing.Point(1047, 18);
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
			this.titleLabel.Location = new System.Drawing.Point(1101, 18);
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
			this.searchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
			this.searchButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(44)))), ((int)(((byte)(28)))));
			this.searchButton.Image = ((System.Drawing.Image)(resources.GetObject("searchButton.Image")));
			this.searchButton.Location = new System.Drawing.Point(291, 21);
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
			// loadedCharacterLabel
			// 
			this.loadedCharacterLabel.AutoSize = true;
			this.loadedCharacterLabel.BackColor = System.Drawing.Color.Transparent;
			this.loadedCharacterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
			this.loadedCharacterLabel.Location = new System.Drawing.Point(12, 376);
			this.loadedCharacterLabel.Name = "loadedCharacterLabel";
			this.loadedCharacterLabel.Size = new System.Drawing.Size(172, 26);
			this.loadedCharacterLabel.TabIndex = 19;
			this.loadedCharacterLabel.Text = "Character Name";
			// 
			// loadedVaultLabel
			// 
			this.loadedVaultLabel.AutoSize = true;
			this.loadedVaultLabel.BackColor = System.Drawing.Color.Transparent;
			this.loadedVaultLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
			this.loadedVaultLabel.Location = new System.Drawing.Point(6, 130);
			this.loadedVaultLabel.Name = "loadedVaultLabel";
			this.loadedVaultLabel.Size = new System.Drawing.Size(127, 26);
			this.loadedVaultLabel.TabIndex = 20;
			this.loadedVaultLabel.Text = "Vault Name";
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
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackgroundImage = Resources.MainForm_NewBackground;
			this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
			this.CancelButton = this.exitButton;
			this.ClientSize = new System.Drawing.Size(1269, 637);
			this.ConstrainToDesignRatio = true;
			this.Controls.Add(this.loadedVaultLabel);
			this.Controls.Add(this.loadedCharacterLabel);
			this.Controls.Add(this.searchButton);
			this.Controls.Add(this.aboutButton);
			this.Controls.Add(this.secondaryVaultListComboBox);
			this.Controls.Add(this.findLabel);
			this.Controls.Add(this.searchTextBox);
			this.Controls.Add(this.panelSelectButton);
			this.Controls.Add(this.customMapText);
			this.Controls.Add(this.configureButton);
			this.Controls.Add(this.vaultLabel);
			this.Controls.Add(this.vaultListComboBox);
			this.Controls.Add(this.itemTextPanel);
			this.Controls.Add(this.characterLabel);
			this.Controls.Add(this.characterComboBox);
			this.Controls.Add(this.exitButton);
			this.Controls.Add(this.titleLabel);
			this.DrawCustomBorder = true;
			this.ForeColor = System.Drawing.Color.White;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
			this.Controls.SetChildIndex(this.characterComboBox, 0);
			this.Controls.SetChildIndex(this.characterLabel, 0);
			this.Controls.SetChildIndex(this.itemTextPanel, 0);
			this.Controls.SetChildIndex(this.vaultListComboBox, 0);
			this.Controls.SetChildIndex(this.vaultLabel, 0);
			this.Controls.SetChildIndex(this.configureButton, 0);
			this.Controls.SetChildIndex(this.customMapText, 0);
			this.Controls.SetChildIndex(this.panelSelectButton, 0);
			this.Controls.SetChildIndex(this.searchTextBox, 0);
			this.Controls.SetChildIndex(this.findLabel, 0);
			this.Controls.SetChildIndex(this.secondaryVaultListComboBox, 0);
			this.Controls.SetChildIndex(this.aboutButton, 0);
			this.Controls.SetChildIndex(this.searchButton, 0);
			this.Controls.SetChildIndex(this.loadedCharacterLabel, 0);
			this.Controls.SetChildIndex(this.loadedVaultLabel, 0);
			this.itemTextPanel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}